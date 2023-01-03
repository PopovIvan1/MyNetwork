﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;


namespace MyNetwork.Controllers
{
    public class MyPageController : Controller
    {
        private ApplicationContext db;
        private static string category = "all";
        private static string sortOrder = "no sort";
        private readonly SignInManager<IdentityUser> _signInManager;

        public MyPageController(ApplicationContext db, SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
            this.db = db;
        }

        public IActionResult MyPage()
        {
            if (CurrentUserSettings.AdminMode == "") CurrentUserSettings.AdminMode = CurrentUserSettings.CurrentUser.IsAdmin == "No        " ? "not available" : "available";
            ViewData.Model = db;
            ViewData.Add("username", CurrentUserSettings.CurrentUser.UserName);
            ViewData.Add("category", category);
            ViewData.Add("sortOrder", sortOrder);
            return View();
        }

        public IActionResult AdminMode()
        {
            ViewData.Model = db.AspNetUsers;
            return View();
        }

        public IActionResult NewReview()
        {
            ViewData.Model = db.Tags;
            return View();
        }

        public async Task<IActionResult> AddReviewToDbAsync(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile image)
        {
            if (description.Contains(TextModel.Context["typing description"])) description = "";
            string imgName = await ImageService.GetImageName(image);
            Review review = new Review() { Name = reviewName, CreationName = creationName, Category = category, Date = DateTime.Now, Description = description, AuthorRate = int.Parse(rate), AuthorId = CurrentUserSettings.CurrentUser.Id, ImageUrl = imgName };
            db.Reviews.Add(review);
            db.SaveChanges();
            await db.SetTagsToDb(tags, (await db.Reviews.FirstOrDefaultAsync(reviewFromDb => reviewFromDb.Date == review.Date && reviewFromDb.AuthorId == review.AuthorId)).Id);
            return RedirectToAction("MyPage", "MyPage");
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
                CurrentUserSettings.CurrentUser = await db.FindUserByNameAsync(selectedUser);
                CurrentUserSettings.AdminMode = TextModel.Context["in admin mode"] + CurrentUserSettings.CurrentUser;
            }
            return RedirectToAction("MyPage", "MyPage");
        }

        public IActionResult ChangeParameters(string categoryFromView, string sortOrderFromView)
        {
            category = categoryFromView;
            sortOrder = sortOrderFromView;
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> DeleteProfile(string userId = "")
        {
            User currentUser;
            if (!string.IsNullOrEmpty(userId)) currentUser = await db.GetUserByIdAsync(userId);
            else currentUser = CurrentUserSettings.CurrentUser;
            await checkAdminMode(currentUser);
            await db.RemoveUser(currentUser);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> MakeAdmin(string userId)
        {
            (await db.GetUserByIdAsync(userId)).IsAdmin = "admin";
            db.SaveChanges();
            return RedirectToAction("AdminMode", "MyPage");
        }

        /*public async Task<IActionResult> BlockUser(string userId)
        {
            User currentUser;
            if (!string.IsNullOrEmpty(userId)) currentUser = await db.GetUserByIdAsync(userId);
            else currentUser = CurrentUserSettings.CurrentUser;
            await checkAdminMode(currentUser);
            await db.RemoveUser(currentUser);
            return RedirectToAction("Index", "Home");
        }*/

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
            CurrentUserSettings.CurrentUser = await db.FindUserByNameAsync(User.Identity?.Name!);
        }
    }
}
