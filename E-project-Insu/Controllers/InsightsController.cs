using Microsoft.AspNetCore.Mvc;
using E_project_Insu.Data;
using System.Linq;

namespace E_project_Insu.Controllers
{
    public class InsightsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InsightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var insights = _context.Insights
                .Where(i => i.Status == "Active")
                .OrderByDescending(i => i.PublishedDate)
                .ToList();
            return View(insights);
        }

        public IActionResult Details(int id)
        {
            var insight = _context.Insights.FirstOrDefault(i => i.InsightId == id);
            if (insight == null) return NotFound();
            
            return View(insight);
        }
    }
}
