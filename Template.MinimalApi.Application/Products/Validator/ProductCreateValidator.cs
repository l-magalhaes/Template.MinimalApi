using FluentValidation;

namespace Template.MinimalApi.Application.Products.Validator
{
    public sealed class ProductCreateValidator : AbstractValidator<ProductDtos.Create>
    {
        public ProductCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The product name is required.")
                .MaximumLength(100).WithMessage("The name must have a maximum of 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("The price must be greater than zero.");
        }
    }
}