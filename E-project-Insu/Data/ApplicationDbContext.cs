using Microsoft.EntityFrameworkCore;
using E_project_Insu.Models;

namespace E_project_Insu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Scheme> Schemes { get; set; }
        public DbSet<QuoteRequest> QuoteRequests { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<Insight> Insights { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminTask> AdminTasks { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Benefit> Benefits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User-Policy Relationship
            modelBuilder.Entity<Policy>()
                .HasOne(p => p.User)
                .WithMany(u => u.Policies)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Policy-Loan Relationship
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Policy)
                .WithMany()
                .HasForeignKey(l => l.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Policy-Payment Relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Policy)
                .WithMany()
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
