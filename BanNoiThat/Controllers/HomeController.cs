using BanNoiThat.Models;
using BanNoiThat.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BanNoiThat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IProductRepository _productRepo;

        public HomeController(ICategoryRepository categoryRepo, IProductRepository productRepo)
        {
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Home/Products
        public async Task<IActionResult> Products(int? categoryId)
        {
            var products = await _productRepo.GetProductsByCategoryAsync(categoryId);
            if (categoryId.HasValue)
            {
                var category = await _categoryRepo.GetCategoryByIdAsync(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
            }

            ViewBag.Categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.SelectedCategoryId = categoryId;
            return View(products);
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productRepo.GetProductWithCategoryAsync(id.Value);

            if (product == null) return NotFound();

            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
