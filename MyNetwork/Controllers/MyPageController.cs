using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Data;
using MyNetwork.Models;

namespace MyNetwork.Controllers
{
    public class MyPageController : Controller
    {
        private ApplicationContext db;

        public MyPageController(ApplicationContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            ViewData.Model = db;
            return View();
        }

        public IActionResult AdminMode()
        {
            return View();
        }

        public IActionResult NewReview()
        {
            return View();
        }
    }
}
