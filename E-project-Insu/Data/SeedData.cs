using E_project_Insu.Data;
using E_project_Insu.Models;

namespace E_project_Insu.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Seed Benefits if none exist
            if (!context.Benefits.Any())
            {
                context.Benefits.AddRange(
                    new Benefit
                    {
                        Number = "01",
                        Title = "Comprehensive Coverage",
                        Description = "Complete protection that wraps around your life. From health crises to property damage, we stand as your unwavering shield against the unknown.",
                        IsActive = true
                    },
                    new Benefit
                    {
                        Number = "02",
                        Title = "Urban Resilience",
                        Description = "Built for the city. Coverage designed specifically for urban lifestyles and modern challenges.",
                        IsActive = true
                    },
                    new Benefit
                    {
                        Number = "03",
                        Title = "Future Vision",
                        Description = "Investing in forever. Long-term protection that grows with you and adapts to your changing needs.",
                        IsActive = true
                    },
                    new Benefit
                    {
                        Number = "04",
                        Title = "Digital First",
                        Description = "Manage your entire portfolio from your pocket. Instant claims, real-time tracking, and zero paperwork.",
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }
            
            // Seed Services if none exist
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service
                    {
                        Title = "Life Insurance",
                        Description = "Comprehensive life coverage ensuring your family's future is secure. We provide financial stability when it's needed most, with plans adaptable to every stage of life.",
                        Icon = "🛡️",
                        ImageUrl = "https://images.unsplash.com/photo-1518136247453-74e7b5265980?auto=format&fit=crop&q=80&w=1800",
                        LinkUrl = "/Schemes/Index?type=Life",
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Medical Insurance",
                        Description = "Priority access to world-class healthcare. From routine checkups to critical surgeries, we handle the bills so you can focus on recovery.",
                        Icon = "⚕️",
                        ImageUrl = "https://images.unsplash.com/photo-1631815589968-fdb09a223b1e?auto=format&fit=crop&q=80&w=1800",
                        LinkUrl = "/Schemes/Index?type=Medical",
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Motor Insurance",
                        Description = "Drive with absolute peace of mind. Our comprehensive motor coverage protects against accidents, theft, and third-party liabilities instantly.",
                        Icon = "🚗",
                        ImageUrl = "https://i.pinimg.com/736x/02/f9/f7/02f9f711dd828e3abdd54649e46003a4.jpg",
                        LinkUrl = "/Schemes/Index?type=Motor",
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Home Insurance",
                        Description = "Your sanctuary, fully secured. We cover everything from natural disasters to burglary, ensuring your home remains your safest place.",
                        Icon = "🏠",
                        ImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&q=80&w=1800",
                        LinkUrl = "/Schemes/Index?type=Home",
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
