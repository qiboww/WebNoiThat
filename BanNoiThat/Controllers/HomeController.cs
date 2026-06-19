using BanNoiThat.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BanNoiThat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Products(int? categoryId)
        {
            ViewBag.SelectedCategoryId = categoryId;
            return View();
        }

        public IActionResult ProductDetails(int id)
        {
            ViewBag.ProductId = id;
            return View();
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
