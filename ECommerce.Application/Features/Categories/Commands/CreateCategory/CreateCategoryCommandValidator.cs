using FluentValidation;

namespace ECommerce.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Name)
                .RequiredField(localizer)
                .LengthBetween(2, 150, localizer);

            RuleFor(x => x.Request.Slug)
                .RequiredField(localizer)
                .LengthBetween(2, 150, localizer)
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage(localizer["Validation.InvalidSlug"].Value);
        });
    }
}
