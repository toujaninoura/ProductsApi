using FluentValidation;
using ProductsApi.Application.DTOs;

namespace ProductsApi.Application.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.Prix)
            .GreaterThan(0).WithMessage("Le prix doit être supérieur à 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Le stock ne peut pas être négatif.");

        RuleFor(x => x.Categorie)
            .NotEmpty().WithMessage("La catégorie est obligatoire.");
    }
}
