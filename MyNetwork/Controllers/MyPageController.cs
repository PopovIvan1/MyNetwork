using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;


namespace MyNetwork.Controllers
{
    public class MyPageController : Controller
    {
        private ApplicationContext _db;
        private readonly SignInManager<IdentityUser> _signInManager;

        public MyPageController(ApplicationContext db, SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
            _db = db;
        }

        public async Task<IActionResult> MyPage()
        {
            if (string.IsNullOrEmpty(Response.HttpContext.Request.Cookies["adminMode"]))
            {
                User user = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == Response.HttpContext.Request.Cookies["currentUser"]);
                Response.Cookies.Append("adminMode", user.IsAdmin == "No        " ? "not available" : "available");
            }
            ViewData.Model = await _db.Services.Users.GetFullUserData
                (await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == Response.HttpContext.Request.Cookies["currentUser"]));
            return View();
        }

        public IActionResult AdminMode()
        {
            ViewData.Model = _db.AspNetUsers;
            return View();
        }

        public IActionResult BackToMyPage()
        {
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> BackToAdminMode()
        {
            await setAdminSettingsAsync();
            return RedirectToAction("AdminMode", "MyPage");
        }

        public async Task<IActionResult> BackToNotAdminMode()
        {
            await setAdminSettingsAsync();
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> SelectUser(string selectedUser)
        {
            if (selectedUser != null)
            {
                Response.Cookies.Append("currentUser", selectedUser);
                Response.Cookies.Append("adminMode", TextModel.Context["in admin mode"] + selectedUser);
            }
            return RedirectToAction("MyPage", "MyPage");
        }

        public IActionResult ChangeParameters(string categoryFromView, string sortOrderFromView)
        {
            Response.Cookies.Append("myPageCategory", categoryFromView);
            Response.Cookies.Append("myPageSortOrder", sortOrderFromView);
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> DeleteProfile(string userId = "")
        {
            User currentUser;
            if (!string.IsNullOrEmpty(userId)) currentUser = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.Id == userId);
            else currentUser = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == Response.HttpContext.Request.Cookies["currentUser"]);
            await checkAdminMode(currentUser);
            await _db.Services.Users.RemoveUser(currentUser);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> MakeAdmin(string userId)
        {
            (await _db.AspNetUsers.FirstOrDefaultAsync(user => user.Id == userId)).IsAdmin = "admin";
            _db.SaveChanges();
            return RedirectToAction("AdminMode", "MyPage");
        }

        public async Task<IActionResult> BlockUser(string userId)
        {
            User currentUser = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.Id == userId);
            currentUser.IsBlock = currentUser.IsBlock == "block" ? "" : "block";
            _db.SaveChanges();
            return RedirectToAction("AdminMode", "MyPage");
        }

        public async Task CheckUserStatus(string userName)
        {
            User user = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == userName);
            if (user == null || user.IsBlock == "block")
            {
                await _signInManager.SignOutAsync();
            }
        }

        private async Task checkAdminMode(User currentUser)
        {
            if (currentUser.UserName == User?.Identity!.Name)
            {
                await _signInManager.SignOutAsync();
                Response.Cookies.Append("currentUser", "");
                Response.Cookies.Append("adminMode", "");
            }
            else
            {
                await setAdminSettingsAsync();
            }
        }

        private async Task setAdminSettingsAsync()
        {
            Response.Cookies.Append("adminMode", "available");
            Response.Cookies.Append("currentUser", User.Identity.Name);
        }
    }
}
