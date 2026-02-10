using System.Diagnostics;
using E_project_Insu.Models;
using E_project_Insu.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_project_Insu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch top 3 active insights for homepage
            var insights = await _context.Insights
                .Where(i => i.Status == "Active")
                .OrderByDescending(i => i.PublishedDate)
                .Take(3)
                .ToListAsync();
            
            // Fetch active benefits for homepage
            var benefits = await _context.Benefits
                .Where(b => b.IsActive)
                .OrderBy(b => b.Number)
                .ToListAsync();
            
            // Fetch active services for homepage
            var services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();
            
            ViewBag.Insights = insights;
            ViewBag.Benefits = benefits;
            ViewBag.Services = services;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Inquiry model)
        {
            if (ModelState.IsValid)
            {
                _context.Inquiries.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thank you for contacting us. We will get back to you shortly.";
                return RedirectToAction("Contact");
            }
            return View(model);
        }

        public IActionResult Team()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // 1. Check User Table
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserFirstName", user.FirstName ?? "User");
                HttpContext.Session.SetString("Role", "PolicyHolder");
                return RedirectToAction("Dashboard", "User");
            }

            // 2. Check Admin Table
            var admin = _context.Admins.FirstOrDefault(a => a.Email == email && a.Password == password);
            if (admin != null)
            {
                HttpContext.Session.SetString("UserEmail", admin.Email);
                HttpContext.Session.SetString("UserFirstName", "Admin");
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToAction("Dashboard", "Admin");
            }

            // 3. Failed
            ViewBag.Error = "Invalid Email or Password";
            return View();
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(User model)
        {
            if (ModelState.IsValid)
            {
                // Check if email exists in User or Admin table
                if (_context.Users.Any(u => u.Email == model.Email) || _context.Admins.Any(a => a.Email == model.Email))
                {
                    ViewBag.Error = "Email already exists.";
                    return View(model);
                }

                model.Role = "PolicyHolder"; // Force role
                model.CreatedDate = DateTime.Now;

                _context.Users.Add(model);
                _context.SaveChanges();

                // Auto Login Logic could go here, but kept simple redirect for now per previous code. 
                // Wait, typically redirect to login is safer for academic project.
                // But if we want consistent session if they log in:
                // Let's redirect to Login as requested.
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult CalculateQuote(string type, int age, decimal sumAssured, double level)
        {
            // Server-side logic for "backend connected" calculator
            double rate = 0.0005; // Default Life
            
            switch (type?.ToLower())
            {
                case "life": rate = 0.0005; break;
                case "medical": rate = 0.0008; break;
                case "motor": rate = 0.02; break; // Annual-ish base
                case "home": rate = 0.0003; break;
            }

            double ageFactor = 1.0;
            if (type?.ToLower() == "life" || type?.ToLower() == "medical")
            {
                ageFactor = 1.0 + ((age - 20) * 0.02);
                if (ageFactor < 1.0) ageFactor = 1.0;
            }
            
            // Logic: (Sum * Rate * Level * AgeFactor) / 12 for Monthly
            double monthlyPremium = ((double)sumAssured * rate * level * ageFactor) / 12.0;
            
            if (type?.ToLower() == "motor") 
            {
                // Motor usually less dependent on age in this simple model, matching frontend logic
                monthlyPremium = ((double)sumAssured * rate * level) / 12.0;
            }
            
            // Save to DB ("Lead Generation")
            var quote = new QuoteRequest
            {
                PolicyType = type,
                Age = age,
                SumAssured = sumAssured,
                CoverageLevel = level.ToString(),
                EstimatedPremium = (decimal)Math.Floor(monthlyPremium),
                RequestDate = DateTime.Now
            };
            
            _context.QuoteRequests.Add(quote);
            _context.SaveChanges();

            return Json(new { premium = Math.Floor(monthlyPremium) });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
