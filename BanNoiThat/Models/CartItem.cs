using System.Collections.Generic;

namespace BanNoiThat.Models
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckoutRequest
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
    }
}
