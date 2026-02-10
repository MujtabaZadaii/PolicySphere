using E_project_Insu.Data;
using E_project_Insu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_project_Insu.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _context;

        public UserController(ILogger<UserController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetString("Role") == "PolicyHolder";
        }

        private int? GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Home");

            // 1. Summary Counts
            var totalPolicies = await _context.Policies.CountAsync(p => p.UserId == userId);
            var activePolicies = await _context.Policies.CountAsync(p => p.UserId == userId && p.Status == "Active");
            var pendingLoans = await _context.Loans.CountAsync(l => l.Policy.UserId == userId && l.LoanStatus == "Pending");

            ViewBag.TotalPolicies = totalPolicies;
            ViewBag.ActivePolicies = activePolicies;
            ViewBag.PendingLoans = pendingLoans;

            // 2. Next Due Date Logic (For ALL Active Policies)
            var userPolicies = await _context.Policies
                .Where(p => p.UserId == userId && p.Status == "Active")
                .ToListAsync();

            var upcomingDues = new List<dynamic>();

            foreach (var policy in userPolicies)
            {
                var lastPayment = await _context.Payments
                    .Where(p => p.PolicyId == policy.PolicyId && p.PaymentStatus == "Verified")
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync();

                DateTime nextDate;
                if (lastPayment != null) nextDate = lastPayment.PaymentDate.AddMonths(1);
                else nextDate = policy.StartDate.AddMonths(1);

                var daysLeft = (nextDate - DateTime.Now).Days;

                upcomingDues.Add(new {
                    Policy = policy.PolicyType,
                    DueDate = nextDate,
                    DaysLeft = daysLeft,
                    Status = daysLeft < 0 ? "Overdue" : "Upcoming"
                });
            }

            ViewBag.UpcomingDues = upcomingDues.OrderBy(d => d.DueDate).ToList();

            // 3. Alerts System (Real-time checks)
            var alerts = new List<dynamic>();

            // Alert: Recent Policy Approvals (Active < 7 days)
            var recentActivePolicies = userPolicies.Where(p => p.StartDate > DateTime.Now.AddDays(-7)).ToList();
            foreach(var p in recentActivePolicies)
            {
                alerts.Add(new { Type = "Success", Title="Policy Approved", Message = $"{p.PolicyType} is now Active." });
            }

            // Alert: Loan Status Updates (Not Pending)
            var recentLoans = await _context.Loans
                .Include(l => l.Policy)
                .Where(l => l.Policy.UserId == userId && l.LoanStatus != "Pending")
                .OrderByDescending(l => l.RequestedDate)
                .Take(3)
                .ToListAsync();
            
            foreach(var l in recentLoans)
            {
                string type = l.LoanStatus == "Rejected" ? "Danger" : "Success";
                alerts.Add(new { Type = type, Title=$"Loan {l.LoanStatus}", Message = $"Your request for {l.RequestedAmount:C} was {l.LoanStatus.ToLower()}." });
            }

            // Alert: Scheme Status (Showing recently added active schemes)
            var activeSchemes = await _context.Schemes
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.SchemeId)
                .Take(3)
                .ToListAsync();

            foreach(var s in activeSchemes)
            {
                alerts.Add(new { Type = "Info", Title="New Scheme", Message = $"{s.SchemeName} is now available!" });
            }

            ViewBag.Alerts = alerts;


            // 4. Recent Activity (Limit 3)
            var activities = new List<dynamic>();

            var payments = await _context.Payments
                .Include(p => p.Policy)
                .Where(p => p.Policy.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(3)
                .ToListAsync();

            foreach(var p in payments)
            {
                activities.Add(new { 
                    Type = "Payment", 
                    Title = "Premium Paid", 
                    Subtitle = p.Policy.PolicyType, 
                    Date = p.PaymentDate, 
                    Amount = p.Amount 
                });
            }

            var loans = await _context.Loans
                .Include(l => l.Policy)
                .Where(l => l.Policy.UserId == userId)
                .OrderByDescending(l => l.RequestedDate)
                .Take(3)
                .ToListAsync();

            foreach(var l in loans)
            {
                activities.Add(new { 
                    Type = "Loan", 
                    Title = "Loan Request", 
                    Subtitle = l.Policy.PolicyType, 
                    Date = l.RequestedDate, 
                    Amount = l.RequestedAmount 
                });
            }

            ViewBag.RecentActivity = activities.OrderByDescending(a => a.Date).Take(3).ToList();

            return View();
        }

        public async Task<IActionResult> Policies()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            var policies = await _context.Policies
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return View(policies);
        }

        [HttpGet]
        public IActionResult BuyPolicy()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BuyPolicy(string policyType, string policyName, decimal coverageAmount, int durationYears)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Home");

            // Simple Premium Calculation Logic
            decimal rate = 0.02m;
            switch (policyType?.ToLower())
            {
                case "life": rate = 0.02m; break;
                case "medical": rate = 0.03m; break;
                case "motor": rate = 0.025m; break;
                case "home": rate = 0.018m; break;
            }
            decimal premium = (coverageAmount * rate) / 12;

            var policy = new Policy
            {
                UserId = userId.Value,
                PolicyNumber = "POL-" + new Random().Next(10000, 99999), 
                PolicyType = policyType + " - " + (policyName ?? "Custom"),
                CoverageAmount = coverageAmount,
                PremiumAmount = Math.Round(premium, 2),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddYears(durationYears),
                Status = "Pending", // Requires Admin Approval
                ImageUrl = $"/policy images/{policyType?.ToLower()} policy.png" 
            };

            _context.Policies.Add(policy);

            // Notification for Admin
            var notification = new Notification
            {
                Title = "New Policy Purchased",
                Message = $"User purchased a {policyType} policy. Approval needed.",
                Type = "Policy",
                Link = "/Admin/Policies"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction("Policies");
        }

        [Route("user/policies/{id}")]
        public async Task<IActionResult> PolicyDetails(int id)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            var policy = await _context.Policies
                .FirstOrDefaultAsync(p => p.PolicyId == id && p.UserId == userId);

            if (policy == null) return NotFound();

            return View(policy);
        }

        [HttpGet]
        public IActionResult CalculatePremium()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult CalculatePremium(string policyType, int age, decimal coverageAmount, int policyTerm, string paymentFrequency)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            decimal calculatedPremium = 0;
            decimal rate = 0;

            switch (policyType?.ToLower())
            {
                case "life": rate = 0.02m; break;
                case "medical": rate = 0.03m; break;
                case "motor": rate = 0.025m; break;
                case "home": rate = 0.018m; break;
                default: rate = 0.02m; break;
            }

            if (policyTerm > 0)
            {
                calculatedPremium = (coverageAmount * rate) / policyTerm;
            }

            ViewBag.CalculatedPremium = calculatedPremium;
            ViewBag.Disclaimer = "This is an estimate only. Final premium may vary based on health checks and other factors.";
            
            // Retain inputs
            ViewBag.PolicyType = policyType;
            ViewBag.Age = age;
            ViewBag.CoverageAmount = coverageAmount;
            ViewBag.PolicyTerm = policyTerm;
            ViewBag.PaymentFrequency = paymentFrequency;

            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> Loans()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            
            var userId = GetUserId();
            var loans = await _context.Loans
                .Include(l => l.Policy)
                .Where(l => l.Policy.UserId == userId)
                .OrderByDescending(l => l.RequestedDate)
                .ToListAsync();
            
            return View(loans);
        }

        [HttpGet]
        public async Task<IActionResult> ApplyLoan()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            
            var userId = GetUserId();
            // Fetch active policies for dropdown
            var activePolicies = await _context.Policies
                .Where(p => p.UserId == userId && p.Status == "Active")
                .ToListAsync();
            
            ViewBag.ActivePolicies = activePolicies;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyLoan(int policyId, decimal requestedAmount)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            var policy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyId == policyId && p.UserId == userId);

            if (policy == null || policy.Status != "Active")
            {
                ViewBag.Error = "Invalid Policy selected.";
                return await ApplyLoan(); // Reload view
            }

            // Validation: Loan must be between 20% and 40% of coverage
            decimal minLoan = policy.CoverageAmount * 0.20m;
            decimal maxLoan = policy.CoverageAmount * 0.40m;

            if (requestedAmount < minLoan || requestedAmount > maxLoan)
            {
                ViewBag.Error = $"Loan amount must be between {minLoan:C} and {maxLoan:C} (20%-40% of coverage).";
                return await ApplyLoan();
            }

            var loan = new Loan
            {
                PolicyId = policyId,
                RequestedAmount = requestedAmount,
                LoanStatus = "Pending",
                RequestedDate = DateTime.Now
            };

            _context.Loans.Add(loan);
            // Fixed duplicate line
            
            
             // NOTIFICATION
            var notification = new Notification
            {
                Title = "New Loan Application",
                Message = $"User applied for a loan of {requestedAmount:C} on Policy #{policy.PolicyNumber}.",
                Type = "Loan",
                Link = "/Admin/Loans"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            ViewBag.Success = "Loan application submitted successfully!";
            return await ApplyLoan();
        }

        [HttpGet]
        public async Task<IActionResult> ApplyScheme(int schemeId)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            
            var scheme = await _context.Schemes.FindAsync(schemeId);
            // Ensure only Active schemes can be applied for
            if (scheme == null || scheme.Status != "Active") return NotFound();

            ViewBag.Scheme = scheme;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyScheme(int schemeId, decimal coverageAmount, int durationYears)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            
            var scheme = await _context.Schemes.FindAsync(schemeId);
            // Ensure only Active schemes can be applied for
            if (scheme == null || scheme.Status != "Active") return NotFound();

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Home");

            // Calculate estimated premium (simple logic)
            // Premium = (Coverage * 0.02) / 12  (monthly)
            decimal rate = 0.02m;
            decimal premium = (coverageAmount * rate) / 12;

            var policy = new Policy
            {
                UserId = userId.Value,
                PolicyNumber = "SCH-" + new Random().Next(1000, 9999) + "-" + scheme.InsuranceType.Substring(0, 3).ToUpper(),
                PolicyType = scheme.InsuranceType + " - " + scheme.SchemeName, // Save correct name
                CoverageAmount = coverageAmount,
                PremiumAmount = Math.Round(premium, 2),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddYears(durationYears),
                Status = "Pending",
                ImageUrl = !string.IsNullOrEmpty(scheme.ImageUrl) ? scheme.ImageUrl : "/policy images/life policy.png"
            };

            _context.Policies.Add(policy);


             // NOTIFICATION
            var notification = new Notification
            {
                Title = "New Policy Request",
                Message = $"User purchased scheme {scheme.SchemeName}. Approval needed.",
                Type = "Policy",
                Link = "/Admin/Policies"
            };
            _context.Notifications.Add(notification);
            
            await _context.SaveChangesAsync();

            return RedirectToAction("Policies"); // Go to My Policies to see it
        }

        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");
            
            var userId = GetUserId();
            var payments = await _context.Payments
                .Include(p => p.Policy)
                .Where(p => p.Policy.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            
            return View("Payments", payments); // Reusing a view name "Payments"
        }

        [HttpGet]
        public async Task<IActionResult> PayPremium()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            // Include Active (for renewal) and Awaiting Payment (for new activation)
            var unpaidPolicies = await _context.Policies
                .Where(p => p.UserId == userId && (p.Status == "Active" || p.Status == "Awaiting Payment")) 
                .ToListAsync();

            ViewBag.Policies = unpaidPolicies;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PayPremium(int policyId, decimal amount, string method, string transactionDetails)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            var policy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyId == policyId && p.UserId == userId);

            if (policy == null) return NotFound();

            // Check if there is already a PENDING payment
            var pendingPayment = await _context.Payments.AnyAsync(p => p.PolicyId == policyId && p.PaymentStatus == "Pending");
            if (pendingPayment)
            {
                 ViewBag.Error = "A payment is already pending verification for this policy.";
                 return await PayPremium();
            }

            var payment = new Payment
            {
                PolicyId = policyId,
                Amount = amount,
                PaymentDate = DateTime.Now,
                PaymentStatus = "Pending", // Admin must verify
                PaymentMethod = method,
                TransactionReference = transactionDetails ?? "N/A"
            };

            _context.Payments.Add(payment);
            
             // NOTIFICATION
            var notification = new Notification
            {
                Title = "New Payment Received",
                Message = $"User sent {amount:C} via {method}. Ref: {transactionDetails}.",
                Type = "Payment",
                Link = "/Admin/Payments"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            ViewBag.Success = "Payment submitted for verification. Policy will be active once approved.";
            return await PayPremium();
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(User model, string newPassword)
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Home");

            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return RedirectToAction("Logout");

            // Update allowed fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.Password = newPassword; // Plain text per academic project requirement
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Update session if name changed
            HttpContext.Session.SetString("UserFirstName", user.FirstName ?? "User");

            ViewBag.Success = "Profile updated successfully.";
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
