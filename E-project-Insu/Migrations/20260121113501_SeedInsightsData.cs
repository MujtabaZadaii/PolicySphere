using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_project_Insu.Migrations
{
    /// <inheritdoc />
    public partial class SeedInsightsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO Insights (Title, Category, Content, ImageUrl, Status, PublishedDate)
                VALUES 
                (
                    'New Life Coverage Plans for 2026',
                    'Product Update',
                    'Redefining security with flexible terms that adapt seamlessly to your changing career path and lifestyle.

We''re excited to announce our enhanced life insurance coverage plans for 2026. These new offerings provide:

• Flexible premium payment options
• Increased coverage limits up to $5 million
• Faster claim processing (48-hour turnaround)
• Digital policy management through mobile app
• Family protection add-ons at no extra cost

Our team has worked tirelessly to create insurance solutions that truly understand modern life. Whether you''re starting a family, changing careers, or planning retirement, our new plans adapt to your journey.

Contact our team today to learn more about how these plans can protect what matters most to you.',
                    'https://images.unsplash.com/photo-1451187580459-43490279c0fa?q=80&w=1200&auto=format&fit=crop',
                    'Active',
                    DATEADD(day, -5, GETDATE())
                ),
                (
                    'Smarter Premium Calculations Now Live',
                    'Platform News',
                    'Our upgraded AI engine now offers even more precision, ensuring you pay exactly what matches your risk profile.

We''ve just launched our most advanced premium calculation system yet. Built on cutting-edge machine learning technology, our new engine analyzes over 200 data points to provide you with the most accurate premium quotes in the industry.

Key improvements include:

• Real-time risk assessment
• Personalized pricing based on your unique profile
• Transparent breakdown of all factors
• Instant quote generation (under 30 seconds)
• Mobile-optimized calculator interface

This means better prices for low-risk customers and fairer rates for everyone. Try our new calculator today and see the difference precision makes.',
                    'https://images.unsplash.com/photo-1551288049-bebda4e38f71?q=80&w=1200&auto=format&fit=crop',
                    'Active',
                    DATEADD(day, -10, GETDATE())
                ),
                (
                    'Digital Policy Access Expanded',
                    'Service Expansion',
                    'Manage your family''s entire portfolio from a single dashboard, now available in 5 new regions.

We''re thrilled to announce the expansion of our digital policy management platform to five new regions across the country. This expansion brings our award-winning digital services to millions more families.

New features in expanded regions:

• 24/7 policy access from any device
• Instant document downloads
• One-click claim filing
• Real-time claim status tracking
• Family member management
• Automated renewal reminders

Our digital-first approach means less paperwork, faster service, and complete transparency. Join thousands of satisfied customers who have already made the switch to digital policy management.',
                    'https://images.unsplash.com/photo-1573164713714-d95e436ab8d6?q=80&w=1200&auto=format&fit=crop',
                    'Active',
                    DATEADD(day, -15, GETDATE())
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM Insights 
                WHERE Title IN (
                    'New Life Coverage Plans for 2026', 
                    'Smarter Premium Calculations Now Live', 
                    'Digital Policy Access Expanded'
                )
            ");
        }
    }
}
