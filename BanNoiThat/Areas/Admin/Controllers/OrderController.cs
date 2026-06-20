using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BanNoiThat.Data;
using BanNoiThat.Models;

namespace BanNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Order")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // VIEW ROUTES (Giao diện HTML)
        // ==========================================

        // GET: /Admin/Order
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Order/Details/5
        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // ==========================================
        // REST API ENDPOINTS (JSON Data)
        // ==========================================

        // GET: /api/admin/orders
        [HttpGet("/api/admin/orders")]
        public async Task<IActionResult> GetOrdersApi()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        orderId = o.OrderId,
                        userId = o.UserId,
                        userEmail = o.User != null ? o.User.Email : "",
                        orderDate = o.OrderDate,
                        totalAmount = o.TotalAmount,
                        shippingAddress = o.ShippingAddress,
                        phoneNumber = o.PhoneNumber,
                        status = o.Status
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng.", error = ex.Message });
            }
        }

        // GET: /api/admin/orders/{id}
        [HttpGet("/api/admin/orders/{id}")]
        public async Task<IActionResult> GetOrderApi(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}." });
                }

                var response = new
                {
                    orderId = order.OrderId,
                    userId = order.UserId,
                    userEmail = order.User != null ? order.User.Email : "",
                    orderDate = order.OrderDate,
                    totalAmount = order.TotalAmount,
                    shippingAddress = order.ShippingAddress,
                    phoneNumber = order.PhoneNumber,
                    status = order.Status,
                    orderDetails = order.OrderDetails.Select(d => new
                    {
                        orderDetailId = d.OrderDetailId,
                        productId = d.ProductId,
                        productName = d.Product != null ? d.Product.Name : "Sản phẩm đã bị xóa",
                        productImageUrl = d.Product != null ? d.Product.ImageUrl : "",
                        quantity = d.Quantity,
                        unitPrice = d.UnitPrice,
                        subtotal = d.Quantity * d.UnitPrice
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết đơn hàng.", error = ex.Message });
            }
        }

        // PUT: /api/admin/orders/{id}/status
        [HttpPut("/api/admin/orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatusApi(int id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ." });
            }

            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}." });
                }

                order.Status = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, status = order.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái đơn hàng.", error = ex.Message });
            }
        }

        // DELETE: /api/admin/orders/{id}
        [HttpDelete("/api/admin/orders/{id}")]
        public async Task<IActionResult> DeleteOrderApi(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}." });
                }

                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã xóa đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa đơn hàng.", error = ex.Message });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }
}
