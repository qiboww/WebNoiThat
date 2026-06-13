using BanNoiThat.Models;
using BanNoiThat.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BanNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ProductController(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepo.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile, IFormFile? model3dFile)
        {
            if (ModelState.IsValid)
            { //.Length dung lượng file (tính theo đơn vị Bytes)
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Đảm bảo thư mục wwwroot/images/product tồn tại
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Tạo tên file duy nhất để tránh trùng lặp ảnh trùng tên
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    // Lưu file vào thư mục wwwroot/images/product
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Gán đường dẫn ảnh cho sản phẩm
                    product.ImageUrl = "/images/product/" + fileName;
                }

                if (model3dFile != null && model3dFile.Length > 0)
                {
                    // Đảm bảo thư mục wwwroot/models/product tồn tại
                    var modelsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models", "product");
                    if (!Directory.Exists(modelsFolder))
                    {
                        Directory.CreateDirectory(modelsFolder);
                    }

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model3dFile.FileName);
                    var filePath = Path.Combine(modelsFolder, fileName);

                    // Lưu file vào thư mục wwwroot/models/product
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model3dFile.CopyToAsync(stream);
                    }

                    // Gán đường dẫn mô hình 3D cho sản phẩm
                    product.Model3DUrl = "/models/product/" + fileName;
                }

                await _productRepo.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productRepo.GetProductWithCategoryAsync(id.Value);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productRepo.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, IFormFile? model3dFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product");
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Lưu ảnh mới
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(imagesFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh mới
                        product.ImageUrl = "/images/product/" + fileName;
                    }

                    if (model3dFile != null && model3dFile.Length > 0)
                    {
                        var modelsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models", "product");
                        if (!Directory.Exists(modelsFolder))
                        {
                            Directory.CreateDirectory(modelsFolder);
                        }

                        // Xóa mô hình cũ nếu có
                        if (!string.IsNullOrEmpty(product.Model3DUrl))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Model3DUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Lưu mô hình mới
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model3dFile.FileName);
                        var filePath = Path.Combine(modelsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model3dFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn mô hình mới
                        product.Model3DUrl = "/models/product/" + fileName;
                    }

                    await _productRepo.UpdateProductAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _productRepo.ProductExistsAsync(product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Delete?5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productRepo.GetProductWithCategoryAsync(id.Value);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Product/Delete?5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepo.GetProductByIdAsync(id);
            if (product != null)
            {
                // Xóa ảnh sản phẩm trên server
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Xóa mô hình 3D trên server
                if (!string.IsNullOrEmpty(product.Model3DUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Model3DUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                await _productRepo.DeleteProductAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
