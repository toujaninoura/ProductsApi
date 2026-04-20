using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductsApi.Domain.Entities;

namespace ProductsApi.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nom).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Prix).HasPrecision(18, 2);
        builder.Property(p => p.Categorie).IsRequired().HasMaxLength(100);
    }
}
