using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAdmin.Models;

namespace UserAdmin.Controllers
{
    public class HomeController : Controller
    {
        private AdminContext _context;

        public HomeController(AdminContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [HttpGet("register")]
        public IActionResult Registration()
        {
            ViewBag.Logged = false;
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered!");
                    ViewBag.Logged = false;
                    return View("Registration");
                }
                else
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    if (_context.Users.ToList().Count == 0)
                    {
                        newUser.AdminLevel = 9;
                    }
                    newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    User justMade = _context.Users.FirstOrDefault(u => u.Email == newUser.Email);
                    HttpContext.Session.SetInt32("LoggedId", justMade.UserId);
                    HttpContext.Session.SetString("LoggedName", justMade.FirstName);
                    HttpContext.Session.SetInt32("LoggedAdmin", justMade.AdminLevel);
                    if (justMade.AdminLevel == 9)
                    {
                        return RedirectToAction("AdminDashboard");
                    }
                    else
                    {
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            else
            {
                ViewBag.Logged = false;
                return View ("Registration");
            }
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            ViewBag.Logged = false;
            return View();
        }

        [HttpPost("login")]
        public IActionResult ConfirmLogin(UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(u => u.Email == userLogin.Email);
                if (userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    ViewBag.Logged = false;
                    return View("Login");
                }
                else
                {
                    PasswordHasher<UserLogin> hasher = new PasswordHasher<UserLogin>();
                    PasswordVerificationResult check = hasher.VerifyHashedPassword(userLogin, userInDb.Password, userLogin.Password);
                    if (check == 0)
                    {
                        ModelState.AddModelError("Email", "Invalid Email/Password");
                        ViewBag.Logged = false;
                        return View("Login");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("LoggedId", userInDb.UserId);
                        HttpContext.Session.SetString("LoggedName", userInDb.FirstName);
                        HttpContext.Session.SetInt32("LoggedAdmin", userInDb.AdminLevel);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            else
            {
                ViewBag.Logged = false;
                return View("Login");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            int? userAdmin = HttpContext.Session.GetInt32("LoggedAdmin");
            string userName = HttpContext.Session.GetString("LoggedName");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            else if ((int)userAdmin == 9)
            {
                return RedirectToAction("AdminDashboard");
            }
            else
            {
                List<User> users = _context.Users.ToList();
                ViewBag.UserName = userName;
                ViewBag.Logged = true;
                return View(users);
            }
        }

        [HttpGet("dashboard/admin")]
        public IActionResult AdminDashboard()
        {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            int? userAdmin = HttpContext.Session.GetInt32("LoggedAdmin");
            string userName = HttpContext.Session.GetString("LoggedName");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            else if ((int)userAdmin != 9)
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                List<User> users = _context.Users.ToList();
                ViewBag.UserName = userName;
                ViewBag.Logged = true;
                return View(users);
            }
        }

        [HttpGet("profile")]
        public IActionResult UserProfile()
        {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            User user = _context.Users.SingleOrDefault(u => u.UserId == (int)userId);
            ViewBag.Logged = true;
            return View(user);
        }

        [HttpPost("profile/editinfo")]
        public IActionResult EditInfo(User editUser)
        {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            User updateUser = _context.Users.SingleOrDefault(u => u.UserId == (int)userId);
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
