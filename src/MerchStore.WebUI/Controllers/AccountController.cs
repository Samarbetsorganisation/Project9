using System.Security.Claims;
using MerchStore.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace MerchStore.Controllers
{
    public class AccountController : Controller
    {
        // Mocked user "database" for demonstration purposes.
        private const string MockedUsername = "john.doe";
        private const string MockedPassword = "pass"; // Note: NEVER hard-code passwords in real apps.

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            // Check model validators
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verify the user's credentials
            if (model.Username == MockedUsername && model.Password == MockedPassword)
            {
                // ✅ Success: set up auth cookie
                var claims = new[] { new Claim(ClaimTypes.Name, model.Username) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // ❌ Failed login: show Gandalf
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                // Flag & URL for the view
                ViewBag.ShowGandalf     = true;
                ViewBag.GandalfVideoUrl = Url.Content("~/Videos/0521(1).mp4");

                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
