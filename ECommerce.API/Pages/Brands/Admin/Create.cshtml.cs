using ECommerce.Application.Features.Brands.Commands.CreateBrand;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Pages.Brands.Admin;

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

    public class InputModel
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public IFormFile? Logo { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new CreateBrandCommand(new CreateBrandRequest
        {
            Name = Input.Name,
            Logo = Input.Logo
        }, CurrentUserId));

        if (!result.Succeeded)
        {
            ModelState.AddModelError("Input.Name", result.Error ?? "Error");
            SetError(result.Error);
            return Page();
        }

        SetSuccess(_localizer);
        return RedirectToPage("/Brands/Index");
    }
}

