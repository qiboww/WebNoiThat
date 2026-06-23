using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanNoiThat.Data;
using BanNoiThat.Models;

namespace BanNoiThat.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { message = "Giỏ hàng không được để trống." });
            }

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            {
                return BadRequest(new { message = "Địa chỉ nhận hàng là bắt buộc." });
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { message = "Số điện thoại nhận hàng là bắt buộc." });
            }

            // Lấy ID người dùng hiện tại
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để đặt hàng." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        return NotFound(new { message = $"Không tìm thấy sản phẩm có ID {item.ProductId}." });
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ số lượng tồn kho. Hiện chỉ còn {product.StockQuantity} chiếc." });
                    }

                    // Giảm số lượng tồn kho
                    product.StockQuantity -= item.Quantity;

                    var itemTotal = product.Price * item.Quantity;
                    totalAmount += itemTotal;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                // Luật phí vận chuyển: Miễn phí cho đơn từ 5.000.000₫ trở lên, ngược lại tính 50.000₫
                decimal shippingFee = totalAmount >= 5000000 ? 0 : 50000;
                totalAmount += shippingFee;

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    ShippingAddress = request.ShippingAddress,
                    PhoneNumber = request.PhoneNumber,
                    Status = "Pending",
                    TotalAmount = totalAmount,
                    OrderDetails = orderDetails
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { success = true, orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý đơn hàng.", error = ex.Message });
            }
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để xem lịch sử mua hàng." });
            }

            try
            {
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        orderId = o.OrderId,
                        orderDate = o.OrderDate,
                        totalAmount = o.TotalAmount,
                        shippingAddress = o.ShippingAddress,
                        phoneNumber = o.PhoneNumber,
                        status = o.Status,
                        orderDetails = o.OrderDetails.Select(d => new
                        {
                            orderDetailId = d.OrderDetailId,
                            productId = d.ProductId,
                            productName = d.Product != null ? d.Product.Name : "Sản phẩm đã bị xóa",
                            productImageUrl = d.Product != null ? d.Product.ImageUrl : "",
                            quantity = d.Quantity,
                            unitPrice = d.UnitPrice,
                            subtotal = d.Quantity * d.UnitPrice
                        })
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy lịch sử đơn hàng.", error = ex.Message });
            }
        }
    }
}
