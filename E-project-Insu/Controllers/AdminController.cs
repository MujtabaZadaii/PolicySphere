using Microsoft.AspNetCore.Mvc;
using E_project_Insu.Data;
using E_project_Insu.Models;
using Microsoft.EntityFrameworkCore;

namespace E_project_Insu.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to validate Admin Session
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            
            // Stats
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalPolicies = _context.Policies.Count();
            ViewBag.ActiveSchemes = _context.Schemes.Count(s => s.Status == "Active");
            ViewBag.PendingLoans = _context.Loans.Count(l => l.LoanStatus == "Pending");
            ViewBag.PendingPolicies = _context.Policies.Count(p => p.Status == "Pending");

            // Notifications
            ViewBag.Notifications = _context.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToList();

            // REAL DATA FOR DASHBOARD
            
            // 1. Pending Loans List (for Accordion)
            ViewBag.PendingLoanList = _context.Loans
                .Include(l => l.Policy).ThenInclude(p => p.User)
                .Where(l => l.LoanStatus == "Pending")
                .OrderBy(l => l.RequestedDate)
                .Take(5)
                .ToList();

            // 2. Policy Renewals (Expiring in 30 days)
            ViewBag.PolicyRenewals = _context.Policies
                .Include(p => p.User)
                .Where(p => p.EndDate > DateTime.Now && p.EndDate <= DateTime.Now.AddDays(30) && p.Status == "Active")
                .OrderBy(p => p.EndDate)
                .Take(5)
                .ToList();

            // 3. Admin Tasks
            ViewBag.AdminTasks = _context.AdminTasks
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            // 4. Activity Chart Data (Mocking 8 hours daily for consistent activity)
            // In a real app, this would query a SessionLog or AuditLog table
            ViewBag.ActivityData = new int[] { 6, 9, 8, 4, 7, 5, 2 }; 


            // Total Revenue
            ViewBag.TotalRevenue = _context.Payments.Where(p => p.PaymentStatus == "Verified").Sum(p => (decimal?)p.Amount) ?? 0;

            return View();
        }

        // --- TASK ACTIONS ---
        [HttpPost]
        public IActionResult AddTask(string title, string priority)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (!string.IsNullOrWhiteSpace(title))
            {
                var task = new AdminTask
                {
                    Title = title,
                    Priority = priority ?? "Medium",
                    CreatedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.AdminTasks.Add(task);
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult ToggleTask(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var task = _context.AdminTasks.Find(id);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult DeleteTask(int id)
        {
             if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var task = _context.AdminTasks.Find(id);
            if (task != null)
            {
                _context.AdminTasks.Remove(task);
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var users = _context.Users.Include(u => u.Policies).ToList();
            return View(users);
        }

        // --- USER ACTIONS ---
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
             if (!IsAdmin()) return RedirectToAction("Login", "Home");
             var user = _context.Users.Find(id);
             if (user == null) return NotFound();
             return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User user)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            // Remove navigation properties from validation
            ModelState.Remove("Policies");
            ModelState.Remove("Password"); // If password change not handled here
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                var existing = _context.Users.Find(user.UserId);
                if (existing != null)
                {
                    existing.FirstName = user.FirstName;
                    existing.LastName = user.LastName;
                    existing.Email = user.Email;
                    // existing.Phone = user.Phone; // Not in model
                    // existing.Address = user.Address; // Not in model
                    existing.Role = user.Role;
                    // Don't update Password or CreatedDate unless needed
                    
                    _context.SaveChanges();
                }
                return RedirectToAction("Users");
            }
            return View(user);
        }

        public IActionResult Policies()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            // Fixed: Removed Include(Scheme) as Policy doesn't have it
            var policies = _context.Policies.Include(p => p.User).ToList();
            return View(policies);
        }

        // --- POLICY ACTIONS ---
        [HttpGet]
        public IActionResult AddPolicy()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            ViewBag.Users = _context.Users.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddPolicy(Policy policy)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            // Basic validation
            if (ModelState.IsValid)
            {
                _context.Policies.Add(policy);
                _context.SaveChanges();
                return RedirectToAction("Policies");
            }
            ViewBag.Users = _context.Users.ToList();
            return View(policy);
        }

        [HttpGet]
        public IActionResult EditPolicy(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var policy = _context.Policies.Find(id);
            if (policy == null) return NotFound();
            ViewBag.Users = _context.Users.ToList();
            return View(policy);
        }

        [HttpPost]
        public IActionResult EditPolicy(Policy policy)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            
            ModelState.Remove("User");
            ModelState.Remove("ImageUrl"); // In case it's null, we might not want to update it if it's already there? No, we set hidden field. But if existing logic expects it. Let's be safe.
            // Actually, hidden ImageUrl handles the value passing correctly. But let's remove User navigation property validation.

            if (ModelState.IsValid)
            {
                // To be safe, let's fetch existing and update only what we want, OR just rely on the completed model.
                // The provided code used Update(policy). 
                // Let's stick to the user's pattern but remove validation.

                _context.Policies.Update(policy);
                _context.SaveChanges();
                return RedirectToAction("Policies");
            }
            ViewBag.Users = _context.Users.ToList();
            return View(policy);
        }

        public IActionResult DeletePolicy(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var policy = _context.Policies.Find(id);
            if (policy != null)
            {
                _context.Policies.Remove(policy);
                _context.SaveChanges();
            }
            return RedirectToAction("Policies");
        }

        public IActionResult ApprovePolicy(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var policy = _context.Policies.Find(id);
            if (policy != null)
            {
                policy.Status = "Awaiting Payment";
                _context.SaveChanges();
            }
            return RedirectToAction("Policies");
        }

        public IActionResult RejectPolicy(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var policy = _context.Policies.Find(id);
            if (policy != null)
            {
                policy.Status = "Rejected";
                _context.SaveChanges();
            }
            return RedirectToAction("Policies");
        }

        public IActionResult Schemes()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var schemes = _context.Schemes.ToList();
            return View(schemes);
        }

        // --- SCHEME ACTIONS ---
        [HttpGet]
        public IActionResult AddScheme()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddScheme(Scheme scheme, IFormFile schemeImage)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                if (schemeImage != null && schemeImage.Length > 0)
                {
                    // Create folder if not exists
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "schemes");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // unique filename
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(schemeImage.FileName);
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await schemeImage.CopyToAsync(stream);
                    }

                    scheme.ImageUrl = "/images/schemes/" + fileName;
                }
                else
                {
                    // Default fallback if no image uploaded
                    scheme.ImageUrl = "/policy images/life policy.png"; // Default
                }

                _context.Schemes.Add(scheme);
                _context.SaveChanges();
                return RedirectToAction("Schemes");
            }
            return View(scheme);
        }

        [HttpGet]
        public IActionResult EditScheme(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var scheme = _context.Schemes.Find(id);
            if (scheme == null) return NotFound();
            return View(scheme);
        }

        [HttpPost]
        public async Task<IActionResult> EditScheme(Scheme scheme, IFormFile schemeImage)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            
            // We don't want to validate CreatedDate from the form submission as we keep the original
            ModelState.Remove("CreatedDate");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Description");
            ModelState.Remove("Eligibility");

            if (ModelState.IsValid)
            {
                var existing = _context.Schemes.Find(scheme.SchemeId);
                if (existing != null)
                {
                    existing.SchemeName = scheme.SchemeName;
                    existing.InsuranceType = scheme.InsuranceType;
                    existing.Description = scheme.Description;
                    existing.Eligibility = scheme.Eligibility;
                    existing.Status = scheme.Status;
                    
                    // Handle Image Upload
                    if (schemeImage != null && schemeImage.Length > 0)
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "schemes");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(schemeImage.FileName);
                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await schemeImage.CopyToAsync(stream);
                        }

                        existing.ImageUrl = "/images/schemes/" + fileName;
                    }
                    // If no new image, keep existing one
                    
                    _context.SaveChanges();
                }
                return RedirectToAction("Schemes");
            }
            return View(scheme);
        }

        public IActionResult DeleteScheme(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var scheme = _context.Schemes.Find(id);
            if (scheme != null)
            {
                _context.Schemes.Remove(scheme);
                _context.SaveChanges();
            }
            return RedirectToAction("Schemes");
        }

        public IActionResult Loans()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            // Fixed: DbSet is Loans, Included Policy.User since Loan doesn't have User directly
            var loans = _context.Loans.Include(l => l.Policy).ThenInclude(p => p.User).ToList();
            return View(loans);
        }

        [HttpPost]
        public IActionResult UpdateLoanStatus(int loanId, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            
            var loan = _context.Loans.Find(loanId);
            if (loan != null)
            {
                loan.LoanStatus = status;
                _context.SaveChanges();
            }
            return RedirectToAction("Loans");
        }

        public IActionResult Payments()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            // Fixed: Payment has Policy, Policy has User. Payment doesn't have User directly.
            var payments = _context.Payments.Include(p => p.Policy).ThenInclude(pol => pol.User).ToList();
            return View(payments);
        }

        public IActionResult VerifyPayment(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            
            var payment = _context.Payments.Include(p => p.Policy).FirstOrDefault(p => p.PaymentId == id);
            
            if (payment != null && payment.PaymentStatus == "Pending")
            {
                // 1. Mark Payment as Verified
                payment.PaymentStatus = "Verified";

                // 2. Check if Policy needs activation
                if (payment.Policy != null && payment.Policy.Status == "Awaiting Payment")
                {
                    payment.Policy.Status = "Active";
                }

                _context.SaveChanges();
            }
            return RedirectToAction("Payments");
        }

        public IActionResult Settings()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var email = HttpContext.Session.GetString("UserEmail");
            var admin = _context.Admins.FirstOrDefault(a => a.Email == email);
            return View(admin);
        }
        
        [HttpPost]
        public IActionResult UpdateSettings(Admin model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            
            var email = HttpContext.Session.GetString("UserEmail");
            var admin = _context.Admins.FirstOrDefault(a => a.Email == email);
            
            if (admin != null)
            {
                // Update fields
                admin.Username = model.Username;
                admin.Email = model.Email; // Allow email change, but user should relogin in real app. For now just update.
                
                _context.SaveChanges();
                
                // Update Session
                HttpContext.Session.SetString("UserEmail", admin.Email);
                HttpContext.Session.SetString("UserFirstName", admin.Username); // Assuming Username acts as Name
                
                ViewBag.Success = "Profile updated successfully!";
            }
            return View("Settings", admin); // Return to settings view with updated model
        }

        [HttpGet]
        public IActionResult Notifications()
        {
             if (!IsAdmin()) return RedirectToAction("Login", "Home");
             var notifications = _context.Notifications.OrderByDescending(n => n.CreatedDate).ToList();
             return View(notifications);
        }

        [HttpGet]
        public IActionResult MarkNotificationRead(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var note = _context.Notifications.Find(id);
            if(note != null) {
                note.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Notifications");
        }

        [HttpGet]
        public IActionResult CleanupData()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");

            // 1. Cleanup Duplicate Schemes (Keep the one with highest ID i.e. latest)
            var schemes = _context.Schemes.ToList();
            var uniqueSchemes = schemes
                .GroupBy(s => new { s.SchemeName, s.InsuranceType })
                .Select(g => g.OrderByDescending(x => x.SchemeId).First())
                .ToList();
            
            var schemesToDelete = schemes.Except(uniqueSchemes).ToList();
            if (schemesToDelete.Any())
            {
                _context.Schemes.RemoveRange(schemesToDelete);
            }

            // 2. Cleanup Duplicate Payments (Keep the first one per policy)
            var payments = _context.Payments.ToList();
            var uniquePayments = payments
                .GroupBy(p => p.PolicyId)
                .Select(g => g.OrderBy(x => x.PaymentId).First()) // Keep oldest/first payment
                .ToList();

            var paymentsToDelete = payments.Except(uniquePayments).ToList();
            if (paymentsToDelete.Any())
            {
                _context.Payments.RemoveRange(paymentsToDelete);
            }

            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        // --- INSIGHT ACTIONS ---
        public IActionResult Insights()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var insights = _context.Insights.OrderByDescending(i => i.PublishedDate).ToList();
            return View(insights);
        }

        // --- INQUIRY ACTIONS ---
        public IActionResult Inquiries()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var inquiries = _context.Inquiries.OrderByDescending(i => i.Date).ToList();
            return View(inquiries);
        }

        [HttpGet]
        public IActionResult AddInsight()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult AddInsight(Insight insight)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                _context.Insights.Add(insight);
                _context.SaveChanges();
                return RedirectToAction("Insights");
            }
            return View(insight);
        }

        [HttpGet]
        public IActionResult EditInsight(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var insight = _context.Insights.Find(id);
            if (insight == null) return NotFound();
            return View(insight);
        }

        [HttpPost]
        public IActionResult EditInsight(Insight insight)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                _context.Insights.Update(insight);
                _context.SaveChanges();
                return RedirectToAction("Insights");
            }
            return View(insight);
        }

        public IActionResult DeleteInsight(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var insight = _context.Insights.Find(id);
            if (insight != null)
            {
                _context.Insights.Remove(insight);
                _context.SaveChanges();
            }
            return RedirectToAction("Insights");
        }

        // --- SERVICE & BENEFIT ACTIONS ---
        public IActionResult ContentManager()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            ViewBag.Services = _context.Services.ToList();
            ViewBag.Benefits = _context.Benefits.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddService(Service service)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                _context.SaveChanges();
            }
            return RedirectToAction("ContentManager");
        }

        [HttpGet]
        public IActionResult EditService(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var service = _context.Services.Find(id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        public IActionResult EditService(Service service)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                _context.Services.Update(service);
                _context.SaveChanges();
            }
            return RedirectToAction("ContentManager");
        }

        public IActionResult DeleteService(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var service = _context.Services.Find(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                _context.SaveChanges();
            }
            return RedirectToAction("ContentManager");
        }

        [HttpPost]
        public IActionResult AddBenefit(Benefit benefit)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                _context.Benefits.Add(benefit);
                _context.SaveChanges();
            }
            return RedirectToAction("ContentManager");
        }

        [HttpGet]
        public IActionResult EditBenefit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var benefit = _context.Benefits.Find(id);
            if (benefit == null) return NotFound();
            return View(benefit);
        }

        [HttpPost]
        public IActionResult EditBenefit(Benefit benefit)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (ModelState.IsValid)
            {
                _context.Benefits.Update(benefit);
                _context.SaveChanges();
            }
            return RedirectToAction("ContentManager");
        }

        public IActionResult DeleteBenefit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            var benefit = _context.Benefits.Find(id);
            if (benefit != null)
            {
                _context.Benefits.Remove(benefit);
                _context.SaveChanges();
            }
            return RedirectToAction("ContentManager");
        }

        [HttpGet]
        public IActionResult Login()
        {
             if (IsAdmin()) return RedirectToAction("Dashboard");
             return RedirectToAction("Login", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
