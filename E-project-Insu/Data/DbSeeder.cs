using E_project_Insu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace E_project_Insu.Data
{
    public static class DbSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Look for any schemes.
                if (context.Schemes.Any())
                {
                    return;   // DB has been seeded
                }

                var schemes = new Scheme[]
                {
                    new Scheme
                    {
                        SchemeName = "SafeLife Plus",
                        InsuranceType = "Life",
                        Description = "Comprehensive life coverage for you and your family with guaranteed returns and tax benefits.",
                        Eligibility = "Age 18-60, Medical Checkup required",
                        Status = "Active",
                        ImageUrl = "/policy images/life policy.png"
                    },
                    new Scheme
                    {
                        SchemeName = "HealthGuard Premier",
                        InsuranceType = "Medical",
                        Description = "Full medical coverage including hospitalization, surgery, day-care procedures, and critical illness.",
                        Eligibility = "No age limit, Pre-existing conditions covered after 2 years",
                        Status = "Active",
                        ImageUrl = "/policy images/medical policy.jfif"
                    },
                    new Scheme
                    {
                        SchemeName = "DriveSecure Pro",
                        InsuranceType = "Motor",
                        Description = "Complete protection for your vehicle against accidents, theft, fire, and third-party liabilities.",
                        Eligibility = "Vehicle less than 10 years old",
                        Status = "Active",
                        ImageUrl = "/policy images/MOTOR policy.jfif"
                    },
                    new Scheme
                    {
                        SchemeName = "HomeShield Elite",
                        InsuranceType = "Home",
                        Description = "Protect your home structure and belongings against fire, burglary, natural calamities, and more.",
                        Eligibility = "Homeowners and Tenants",
                        Status = "Active",
                        ImageUrl = "/policy images/Home-Insurance.jpg"
                    }
                };

                context.Schemes.AddRange(schemes);
                context.SaveChanges();
            }
        }
    }
}
