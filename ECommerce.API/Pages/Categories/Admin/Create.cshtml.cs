
using ECommerce.Application.Features.Categories.Commands.CreateCategory;
using ECommerce.Application.Features.Categories.Queries.GetAllCategories;

namespace ECommerce.API.Pages.Categories.Admin;

public class CreateModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CategoryDto> ParentOptions { get; set; } = new();

    public class InputModel
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
    }

    public async Task OnGetAsync()
    {
        ParentOptions = await _mediator.Send(new GetAllCategoriesQuery());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ParentOptions = await _mediator.Send(new GetAllCategoriesQuery());

        var request = new CreateCategoryRequest
        {
            Name = Input.Name,
            Slug = Input.Slug,
            ParentCategoryId = Input.ParentCategoryId
        };

        var result = await _mediator.Send(new CreateCategoryCommand(request, CurrentUserId));

        if (!result.Succeeded)
        {
            if (result.Error?.Contains("Slug", StringComparison.OrdinalIgnoreCase) == true)
            {
                ModelState.AddModelError("Input.Slug", result.Error);
            }
            else
            {
                ModelState.AddModelError("Input.Name", result.Error ?? "Error");
            }
            SetError(result.Error);
            return Page();
        }

        SetSuccess(_localizer);
        return RedirectToPage("/Categories/Index");
    }
}

