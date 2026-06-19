using BanNoiThat.Models;
using BanNoiThat.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BanNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // ==========================================
        // VIEW ROUTES (Giao diện HTML)
        // ==========================================

        // GET: /Admin/Category
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Category/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // GET: /Admin/Category/Details/5
        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            ViewBag.CategoryId = id;
            return View();
        }

        // GET: /Admin/Category/Edit/5
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewBag.CategoryId = id;
            return View();
        }

        // ==========================================
        // REST API ENDPOINTS (JSON Data)
        // ==========================================

        // GET: /api/categories (Public for customer menu)
        [AllowAnonymous]
        [HttpGet("/api/categories")]
        public async Task<IActionResult> GetCategoriesApi()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: /api/categories/5 (Public)
        [AllowAnonymous]
        [HttpGet("/api/categories/{id}")]
        public async Task<IActionResult> GetCategoryApi(int id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            return Ok(category);
        }

        // GET: /api/categories/5/with-products (Public)
        [AllowAnonymous]
        [HttpGet("/api/categories/{id}/with-products")]
        public async Task<IActionResult> GetCategoryWithProductsApi(int id)
        {
            var category = await _categoryRepo.GetCategoryWithProductsAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            return Ok(category);
        }

        // POST: /api/categories (Admin only)
        [HttpPost("/api/categories")]
        public async Task<IActionResult> CreateCategoryApi([FromForm] Category category, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "category");
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

                category.ImageUrl = "/images/category/" + fileName;
            }

            await _categoryRepo.AddCategoryAsync(category);
            return Ok(category);
        }

        // PUT: /api/categories/5 (Admin only)
        [HttpPut("/api/categories/{id}")]
        public async Task<IActionResult> UpdateCategoryApi(int id, [FromForm] Category category, IFormFile? imageFile)
        {
            if (id != category.CategoryId)
            {
                return BadRequest(new { message = "ID in route path does not match ID in model." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbCategory = await _categoryRepo.GetCategoryByIdAsync(id);
            if (dbCategory == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            dbCategory.Name = category.Name;
            dbCategory.Description = category.Description;

            if (imageFile != null && imageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "category");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                if (!string.IsNullOrEmpty(dbCategory.ImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbCategory.ImageUrl.TrimStart('/'));
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

                dbCategory.ImageUrl = "/images/category/" + fileName;
            }

            try
            {
                await _categoryRepo.UpdateCategoryAsync(dbCategory);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _categoryRepo.CategoryExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(dbCategory);
        }

        // DELETE: /api/categories/5 (Admin only)
        [HttpDelete("/api/categories/{id}")]
        public async Task<IActionResult> DeleteCategoryApi(int id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _categoryRepo.DeleteCategoryAsync(id);
            return Ok(new { success = true, message = "Category deleted successfully." });
        }
    }
}
