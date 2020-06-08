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
                ViewBag.AdminId = (int)userId;
                return View(users);
            }
        }

        [HttpGet("users/{userId}")]
        public IActionResult UserPage(int userId)
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            if (loggedId == null)
            {
                return RedirectToAction("Index");
            }
            User user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            ViewBag.Messages = _context.Messages
                                       .Include(m => m.Sender)
                                       .Include(m => m.MessageComments)
                                       .ThenInclude(c => c.Commenter)
                                       .Where(m => m.Recipient.UserId == userId)
                                       .OrderByDescending(m => m.CreatedAt)
                                       .ToList();
            ViewBag.User = user;
            ViewBag.Logged = true;
            return View();
        }
        
        [HttpPost("users/{userId}")]
        public IActionResult AddMessage(int userId, Message newMessage)
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            User loggedUser = _context.Users.FirstOrDefault(u => u.UserId == loggedId);
            User recipient = _context.Users.FirstOrDefault(u => u.UserId == userId);
            newMessage.Sender = loggedUser;
            newMessage.Recipient = recipient;
            _context.Messages.Add(newMessage);
            _context.SaveChanges();
            return RedirectToAction("UserPage", new {userId = userId});
        }

        [HttpGet("users/edit")]
        public IActionResult UserProfile()
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            if (loggedId == null)
            {
                return RedirectToAction("Index");
            }
            User user = _context.Users.FirstOrDefault(u => u.UserId == loggedId);
            ViewBag.Logged = true;
            ViewBag.Admin = false;
            ViewBag.UserId = loggedId;
            return View(user);
        }

        [HttpGet("users/edit/{userId}")]
        public IActionResult EditUser(int userId)
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            if (loggedId == null)
            {
                return RedirectToAction("Index");
            }
            if (loggedId == userId)
            {
                RedirectToAction("UserProfile");
            }
            User user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            ViewBag.Logged = true;
            ViewBag.Admin = true;
            ViewBag.UserId = userId;
            return View(user);
        }

        [HttpPost("profile/editinfo/{userId}")]
        public IActionResult EditInfo(int userId, UserInfo editForm)
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            User updateUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (ModelState.IsValid)
            {
                updateUser.FirstName = editForm.FirstName;
                updateUser.LastName = editForm.LastName;
                updateUser.Email = editForm.Email;
                updateUser.AdminLevel = editForm.AdminLevel;
                updateUser.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                if ((int)loggedId == userId)
                {
                    HttpContext.Session.SetString("LoggedName", editForm.FirstName);
                }
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.Logged=true;
                if ((int)loggedId == userId)
                {
                    ViewBag.Admin = false;
                }
                else
                {
                    ViewBag.Admin = true;
                }
                ViewBag.UserId = userId;
                return View("UserProfile", editForm);
            }
        }

        [HttpPost("profile/editPassword/{userId}")]
        public IActionResult EditPassword(int userId, UserPassword editForm)
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            User updateUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if(ModelState.IsValid)
            {
                PasswordHasher<UserPassword> hasher = new PasswordHasher<UserPassword>();
                updateUser.Password = hasher.HashPassword(editForm, editForm.Password);
                updateUser.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.Logged = true;
                if ((int)loggedId == userId)
                {
                    ViewBag.Admin = false;
                }
                else
                {
                    ViewBag.Admin = true;
                }
                ViewBag.UserId = userId;
                return View("UserProfile", updateUser);
            }
        }

        [HttpPost("profile/editDesc")]
        public IActionResult EditDesc(UserDesc editForm)
        {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            User updateUser = _context.Users.FirstOrDefault(u => u.UserId == (int)userId);
            if(ModelState.IsValid)
            {
                updateUser.Description = editForm.Description;
                updateUser.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.Logged = true;
                return View("UserProfile", updateUser);
            }
        }

        [HttpGet("users/new")]
        public IActionResult AddUser() {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Logged = true;
            return View();
        }

        [HttpPost("users/new")]
        public IActionResult NewUser(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered!");
                    ViewBag.Logged = true;
                    return View("AddUser");
                }
                else
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    return RedirectToAction("AdminDashboard");
                }
            }
            else
            {
                ViewBag.Logged = true;
                return View ("AddUser");
            }
        }

        [HttpGet("remove/{userId}")]
        public IActionResult RemoveUser(int userId)
        {
            int? loggedId = HttpContext.Session.GetInt32("LoggedId");
            if (loggedId == null)
            {
                return RedirectToAction("Index");
            }
            if ((int)loggedId == userId)
            {
                return RedirectToAction("Dashboard");
            }
            User removeUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
            _context.Users.Remove(removeUser);
            _context.SaveChanges();
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
