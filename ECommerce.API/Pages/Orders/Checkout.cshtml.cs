using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;
using ECommerce.Application.Features.Carts.Queries.GetMyCart;
using ECommerce.Application.Features.Orders.Commands.CreateOrder;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Pages.Orders;

public class CheckoutModel : RazorPageBase
{
    private readonly IMediator _mediator;

    public CheckoutModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public CartDto Cart { get; set; } = new();
    public List<AddressDto> Addresses { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public Guid ShippingAddressId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Cart = await _mediator.Send(new GetMyCartQuery(CurrentUserId));
        Addresses = await _mediator.Send(new GetMyAddressesQuery(CurrentUserId));

        if (Cart.Items.Count == 0)
        {
            return RedirectToPage("/Cart/Index");
        }

        var defaultAddress = Addresses.FirstOrDefault(a => a.IsDefault) ?? Addresses.FirstOrDefault();
        if (defaultAddress is not null)
        {
            Input.ShippingAddressId = defaultAddress.Id;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Cart = await _mediator.Send(new GetMyCartQuery(CurrentUserId));
        Addresses = await _mediator.Send(new GetMyAddressesQuery(CurrentUserId));

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new CreateOrderCommand(CurrentUserId, new CreateOrderRequest
        {
            ShippingAddressId = Input.ShippingAddressId,
            PaymentMethod = Input.PaymentMethod
        }));

        if (!result.Succeeded)
        {
            SetError(result.Error);
            return Page();
        }

        return RedirectToPage("/Orders/Details", new { id = result.Data!.Id });
    }
}

