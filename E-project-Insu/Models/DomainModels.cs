using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_project_Insu.Models
{
    public class Policy
    {
        [Key]
        public int PolicyId { get; set; }

        public string PolicyNumber { get; set; }

        public string PolicyType { get; set; } // Life, Medical, etc.

        public decimal CoverageAmount { get; set; }

        public decimal PremiumAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = "Active"; // Active, Expired, Pending

        public string? ImageUrl { get; set; } // Path to background image

        // Foreign Key
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class Scheme
    {
        [Key]
        public int SchemeId { get; set; }

        public string SchemeName { get; set; }

        public string InsuranceType { get; set; } // Life, Medical, Motor, Home

        public string? Description { get; set; }

        public string? Eligibility { get; set; }

        public string Status { get; set; } = "Active";

        public string? ImageUrl { get; set; } // Uploaded image path

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class QuoteRequest
    {
        [Key]
        public int RequestId { get; set; }

        public string PolicyType { get; set; }

        public int Age { get; set; }

        public decimal SumAssured { get; set; }

        public string CoverageLevel { get; set; }

        public decimal EstimatedPremium { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;
    }

    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        public decimal RequestedAmount { get; set; }

        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public string LoanStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        // Foreign Key
        public int PolicyId { get; set; }
        public virtual Policy Policy { get; set; }
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string PaymentStatus { get; set; } = "Pending"; // Pending, Verified, Failed

        public string TransactionReference { get; set; } // Bank Transaction ID

        public string PaymentMethod { get; set; } // Bank Transfer, Credit Card, etc.

        // Foreign Key
        public int PolicyId { get; set; }
        public virtual Policy Policy { get; set; }
    }

    public class Insight
    {
        [Key]
        public int InsightId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Category { get; set; } // Product Update, Platform News, etc.

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string Status { get; set; } = "Active"; // Active, Inactive

        public DateTime PublishedDate { get; set; } = DateTime.Now;
    }

    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Type { get; set; } // "Info", "Success", "Warning", "Alert"

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public string Link { get; set; } // URL to the relevant action
    }
    public class Inquiry
    {
        [Key]
        public int InquiryId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }

    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Icon { get; set; } // SVG Path or Icon Class

        public string ImageUrl { get; set; } // Background Image

        public string LinkUrl { get; set; } // Read More Link

        public bool IsActive { get; set; } = true;
    }

    public class Benefit
    {
        [Key]
        public int BenefitId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Number { get; set; } // e.g. "01", "02"

        public bool IsActive { get; set; } = true;
    }
}
