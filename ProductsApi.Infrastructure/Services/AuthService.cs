using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductsApi.Application.DTOs.Auth;
using ProductsApi.Application.Interfaces;
using ProductsApi.Domain.Entities;
using ProductsApi.Domain.Enums;
using ProductsApi.Domain.Exceptions;
using ProductsApi.Infrastructure.Data;

namespace ProductsApi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        IJwtService jwtService,
        AppDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new ValidationException(new[] { "Passwords do not match." });

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new ValidationException(new[] { "Email already in use." });

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, UserRole.User);

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid credentials.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var stored = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired or revoked.");

        stored.IsRevoked = true;
        await _context.SaveChangesAsync();

        return await BuildAuthResponseAsync(stored.User);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken)
            ?? throw new NotFoundException("RefreshToken", refreshToken);

        stored.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var expirationDays = int.Parse(
            _configuration["JWT:RefreshTokenExpirationInDays"] ?? "7");

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JWT:ExpirationInMinutes"] ?? "60"));

        return new AuthResponse(accessToken, refreshToken, expiresAt, user.Id, user.Email!, roles);
    }
}
