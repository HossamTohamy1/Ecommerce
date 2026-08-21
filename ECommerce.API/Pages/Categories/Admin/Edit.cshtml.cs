
using ECommerce.Application.Features.Categories.Commands.UpdateCategory;
using ECommerce.Application.Features.Categories.Queries.GetAllCategories;
using ECommerce.Application.Features.Categories.Queries.GetCategoryById;

namespace ECommerce.API.Pages.Categories.Admin;

public class EditModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CategoryDto> ParentOptions { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150, MinimumLength = 2)]
        [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
        public string Slug { get; set; } = string.Empty;

        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        ParentOptions = (await _mediator.Send(new GetAllCategoriesQuery())).Where(c => c.Id != Id).ToList();

        var result = await _mediator.Send(new GetCategoryByIdQuery(Id));
        if (!result.Succeeded)
        {
            return RedirectToPage("/Categories/Index");
        }

        Input = new InputModel
        {
            Name = result.Data!.Name,
            Slug = result.Data.Slug,
            ParentCategoryId = result.Data.ParentCategoryId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ParentOptions = (await _mediator.Send(new GetAllCategoriesQuery())).Where(c => c.Id != Id).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new UpdateCategoryCommand(Id, new UpdateCategoryRequest
        {
            Name = Input.Name,
            Slug = Input.Slug,
            ParentCategoryId = Input.ParentCategoryId,
            IsActive = Input.IsActive
        }, CurrentUserId));

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

