using SharpDevelopMVC4.Accounts;
using SharpDevelopMVC4.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SharpDevelopMVC4.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        //public ActionResult Index()
        //{
        //    return RedirectToAction("Login");
        //}

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            var user = MockUserRepository.Authenticate(model.Email, model.Password);

            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);

                var authTicket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(20), false, user.Roles);
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                HttpContext.Response.Cookies.Add(authCookie);

                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
        }


        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}


namespace SharpDevelopMVC4.Accounts
{
    public class MockUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }
    }

    public static class MockUserRepository
    {
        static List<MockUser> users = new List<MockUser>() 
        {
            new MockUser() {Email="admin@gmail.com", Roles="Admin,Editor", Password="admin123" },
            new MockUser() {Email="user1@gmail.com", Roles="Editor", Password="user123" },
            new MockUser() {Email="user2@gmail.com", Roles="", Password="userxyz" }
        };

        public static MockUser Authenticate(string email, string password)
        {
            try
            {
                return users.Where(u => u.Email.ToLower() == email.ToLower() && u.Password == password).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}

