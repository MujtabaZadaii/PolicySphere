using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_project_Insu.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "PolicyHolder";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }

    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        public string Email { get; set; }

        public string Username { get; set; } = "Admin";

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "Admin";
    }
}
