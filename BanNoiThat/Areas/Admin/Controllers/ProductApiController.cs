using BanNoiThat.Models;
using BanNoiThat.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BanNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Product")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;

        public ProductController(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        // ==========================================
        // VIEW ROUTES (Giao diện HTML)
        // ==========================================

        // GET: /Admin/Product
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Product/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // GET: /Admin/Product/Details/5
        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }

        // GET: /Admin/Product/Edit/5
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }

        // ==========================================
        // REST API ENDPOINTS (JSON Data)
        // ==========================================

        // GET: /api/products (Public for customer grid)
        [AllowAnonymous]
        [HttpGet("/api/products")]
        public async Task<IActionResult> GetProductsApi([FromQuery] int? categoryId)
        {
            var products = await _productRepo.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }

        // GET: /api/products/5 (Public)
        [AllowAnonymous]
        [HttpGet("/api/products/{id}")]
        public async Task<IActionResult> GetProductApi(int id)
        {
            var product = await _productRepo.GetProductWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
            return Ok(product);
        }

        // POST: /api/products (Admin only)
        [HttpPost("/api/products")]
        public async Task<IActionResult> CreateProductApi([FromForm] Product product, IFormFile? imageFile, IFormFile? model3dFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(imagesFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/product/" + fileName;
            }

            if (model3dFile != null && model3dFile.Length > 0)
            {
                var modelsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models", "product");
                if (!Directory.Exists(modelsFolder))
                {
                    Directory.CreateDirectory(modelsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model3dFile.FileName);
                var filePath = Path.Combine(modelsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model3dFile.CopyToAsync(stream);
                }

                product.Model3DUrl = "/models/product/" + fileName;
            }

            await _productRepo.AddProductAsync(product);
            return Ok(product);
        }

        // PUT: /api/products/5 (Admin only)
        [HttpPut("/api/products/{id}")]
        public async Task<IActionResult> UpdateProductApi(int id, [FromForm] Product product, IFormFile? imageFile, IFormFile? model3dFile)
        {
            if (id != product.ProductId)
            {
                return BadRequest(new { message = "ID in route path does not match ID in model." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbProduct = await _productRepo.GetProductByIdAsync(id);
            if (dbProduct == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            // Update text properties
            dbProduct.Name = product.Name;
            dbProduct.Price = product.Price;
            dbProduct.StockQuantity = product.StockQuantity;
            dbProduct.Material = product.Material;
            dbProduct.Description = product.Description;
            dbProduct.CategoryId = product.CategoryId;

            // Image file upload handler
            if (imageFile != null && imageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                if (!string.IsNullOrEmpty(dbProduct.ImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(imagesFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                dbProduct.ImageUrl = "/images/product/" + fileName;
            }

            // 3D model file upload handler
            if (model3dFile != null && model3dFile.Length > 0)
            {
                var modelsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models", "product");
                if (!Directory.Exists(modelsFolder))
                {
                    Directory.CreateDirectory(modelsFolder);
                }

                if (!string.IsNullOrEmpty(dbProduct.Model3DUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbProduct.Model3DUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model3dFile.FileName);
                var filePath = Path.Combine(modelsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model3dFile.CopyToAsync(stream);
                }

                dbProduct.Model3DUrl = "/models/product/" + fileName;
            }

            try
            {
                await _productRepo.UpdateProductAsync(dbProduct);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _productRepo.ProductExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(dbProduct);
        }

        // DELETE: /api/products/5 (Admin only)
        [HttpDelete("/api/products/{id}")]
        public async Task<IActionResult> DeleteProductApi(int id)
        {
            var product = await _productRepo.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            if (!string.IsNullOrEmpty(product.Model3DUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Model3DUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _productRepo.DeleteProductAsync(id);
            return Ok(new { success = true, message = "Product deleted successfully." });
        }
    }
}
