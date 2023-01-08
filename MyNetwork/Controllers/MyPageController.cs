using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;


namespace MyNetwork.Controllers
{
    public class MyPageController : Controller
    {
        private ApplicationContext _db;
        private static string _category = "all";
        private static string _sortOrder = "no sort";
        private readonly SignInManager<IdentityUser> _signInManager;

        public MyPageController(ApplicationContext db, SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
            _db = db;
        }

        public async Task<IActionResult> MyPage()
        {
            if (CurrentUserSettings.AdminMode == "") CurrentUserSettings.AdminMode = CurrentUserSettings.CurrentUser.IsAdmin == "No        " ? "not available" : "available";
            ViewData.Model = await _db.Services.Users.GetFullUserData
                (await _db.AspNetUsers.FirstOrDefaultAsync(user => user.Id == CurrentUserSettings.CurrentUser.Id));
            ViewData.Add("category", _category);
            ViewData.Add("sortOrder", _sortOrder);
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
                CurrentUserSettings.CurrentUser = await _db.Services.Users.GetFullUserData
                (await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == selectedUser));
                CurrentUserSettings.AdminMode = TextModel.Context["in admin mode"] + CurrentUserSettings.CurrentUser.UserName;
            }
            return RedirectToAction("MyPage", "MyPage");
        }

        public IActionResult ChangeParameters(string categoryFromView, string sortOrderFromView)
        {
            _category = categoryFromView;
            _sortOrder = sortOrderFromView;
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> DeleteProfile(string userId = "")
        {
            User currentUser;
            if (!string.IsNullOrEmpty(userId)) currentUser = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.Id == userId);
            else currentUser = CurrentUserSettings.CurrentUser;
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
                CurrentUserSettings.CurrentUser = new User();
                CurrentUserSettings.AdminMode = "";
            }
            else
            {
                await setAdminSettingsAsync();
            }
        }

        private async Task setAdminSettingsAsync()
        {
            CurrentUserSettings.AdminMode = "available";
            CurrentUserSettings.CurrentUser = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);
        }
    }
}
