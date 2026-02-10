using System;
using System.ComponentModel.DataAnnotations;

namespace E_project_Insu.Models
{
    public class AdminTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string Title { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }
        
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
    }
}
