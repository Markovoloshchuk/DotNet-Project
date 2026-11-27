using Catalog.Bll.DTOs.Categories;
using Catalog.Bll.DTOs.MedicalServices;
using FluentValidation;

namespace Catalog.Api.Validators;

public sealed class CreateServiceCategoryDtoValidator
    : AbstractValidator<CreateServiceCategoryDto>
{
    public CreateServiceCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
    }
}

public sealed class UpdateServiceCategoryDtoValidator
    : AbstractValidator<UpdateServiceCategoryDto>
{
    public UpdateServiceCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
    }
}

public sealed class CreateMedicalServiceDtoValidator
    : AbstractValidator<CreateMedicalServiceDto>
{
    private const decimal MaximumPrice = 99_999_999.99m;

    public CreateMedicalServiceDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0L)
            .WithMessage("Category ID must be greater than zero.");

        RuleFor(x => x.EquipmentId)
            .Must(value => !value.HasValue || value.Value > 0)
            .WithMessage("Equipment ID must be greater than zero.");

        RuleFor(x => x.ServiceName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Service name is required.")
            .MaximumLength(150)
            .WithMessage("Service name cannot exceed 150 characters.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0m, MaximumPrice)
            .WithMessage(
                $"Price must be between 0 and {MaximumPrice}.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than zero.");
    }
}

public sealed class UpdateMedicalServiceDtoValidator
    : AbstractValidator<UpdateMedicalServiceDto>
{
    private const decimal MaximumPrice = 99_999_999.99m;

    public UpdateMedicalServiceDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0L)
            .WithMessage("Category ID must be greater than zero.");

        RuleFor(x => x.EquipmentId)
            .Must(value => !value.HasValue || value.Value > 0)
            .WithMessage("Equipment ID must be greater than zero.");

        RuleFor(x => x.ServiceName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Service name is required.")
            .MaximumLength(150)
            .WithMessage("Service name cannot exceed 150 characters.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0m, MaximumPrice)
            .WithMessage(
                $"Price must be between 0 and {MaximumPrice}.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than zero.");
    }
}

public sealed class SearchMedicalServicesDtoValidator
    : AbstractValidator<SearchMedicalServicesDto>
{
    private const decimal MaximumPrice = 99_999_999.99m;

    private static readonly string[] AllowedSortFields =
    [
        "serviceName",
        "price",
        "durationMinutes",
        "isActive",
        "serviceId"
    ];

    public SearchMedicalServicesDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(150)
            .WithMessage("Search name cannot exceed 150 characters.");

        RuleFor(x => x.CategoryId)
            .Must(value => !value.HasValue || value.Value > 0)
            .WithMessage("Category ID must be greater than zero.");

        RuleFor(x => x.MinPrice)
            .Must(value =>
                !value.HasValue ||
                (value.Value >= 0 && value.Value <= MaximumPrice))
            .WithMessage(
                $"Minimum price must be between 0 and {MaximumPrice}.");

        RuleFor(x => x.MaxPrice)
            .Must(value =>
                !value.HasValue ||
                (value.Value >= 0 && value.Value <= MaximumPrice))
            .WithMessage(
                $"Maximum price must be between 0 and {MaximumPrice}.");

        RuleFor(x => x.MaxPrice)
            .Must((filter, maxPrice) =>
                !filter.MinPrice.HasValue ||
                !maxPrice.HasValue ||
                maxPrice.Value >= filter.MinPrice.Value)
            .WithMessage(
                "Maximum price cannot be lower than minimum price.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.PageNumber)
            .Must((filter, pageNumber) =>
                filter.PageSize <= 0 ||
                pageNumber <= int.MaxValue / filter.PageSize)
            .WithMessage("Requested page number is too large.");

        RuleFor(x => x.SortBy)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Sort field is required.")
            .Must(value =>
                AllowedSortFields.Contains(
                    value.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                "Unsupported sort field. Allowed values: " +
                "serviceName, price, durationMinutes, isActive, serviceId.");
    }
}