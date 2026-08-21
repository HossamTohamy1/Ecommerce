using ECommerce.Application.Features.Brands.Commands.UpdateBrand;
using ECommerce.Application.Features.Brands.Commands.UploadBrandLogo;
using ECommerce.Application.Features.Brands.Queries.GetBrandById;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Pages.Brands.Admin;

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

    public BrandDto? Brand { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public IFormFile? Logo { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetBrandByIdQuery(Id));
        if (!result.Succeeded)
        {
            return RedirectToPage("/Brands/Index");
        }

        Brand = result.Data;
        Input = new InputModel { Name = result.Data!.Name };
        return Page();

    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Brand = (await _mediator.Send(new GetBrandByIdQuery(Id))).Data;
            return Page();
        }

        var result = await _mediator.Send(new UpdateBrandCommand(Id, new UpdateBrandRequest
        {
            Name = Input.Name,
            IsActive = Input.IsActive
        }, CurrentUserId));

        if (!result.Succeeded)
        {
            ModelState.AddModelError("Input.Name", result.Error ?? "Error");
            SetError(result.Error);
            Brand = result.Data ?? (await _mediator.Send(new GetBrandByIdQuery(Id))).Data;
            return Page();
        }

        if (Input.Logo is not null)
        {
            var logoResult = await _mediator.Send(new UploadBrandLogoCommand(Id, Input.Logo, CurrentUserId));
            if (!logoResult.Succeeded)
            {
                SetError(logoResult.Error);
                Brand = logoResult.Data;
                return Page();
            }
        }

        SetSuccess(_localizer);
        return RedirectToPage("/Brands/Index");
    }
}

