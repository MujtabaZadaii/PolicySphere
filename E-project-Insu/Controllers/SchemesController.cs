using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_project_Insu.Data;
using E_project_Insu.Models;

namespace E_project_Insu.Controllers
{
    public class SchemesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchemesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /schemes
        // Public/User view of all active schemes
        // GET: /schemes
        // Public/User view of all active schemes
        [Route("schemes")]
        public async Task<IActionResult> Index(string type)
        {
            var query = _context.Schemes.AsQueryable()
                                        .Where(s => s.Status == "Active");

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(s => s.InsuranceType == type);
                ViewData["CurrentFilter"] = type;
            }

            var activeSchemes = await query
                .OrderBy(s => s.InsuranceType)
                .ThenBy(s => s.SchemeName)
                .ToListAsync();

            return View(activeSchemes);
        }

        // GET: /schemes/details/{id}
        // View detailed information about a specific scheme
        [Route("schemes/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var scheme = await _context.Schemes
                .FirstOrDefaultAsync(s => s.SchemeId == id && s.Status == "Active");

            if (scheme == null)
                return NotFound();

            return View(scheme);
        }
    }
}
