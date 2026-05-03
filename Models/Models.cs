using System.ComponentModel.DataAnnotations;

namespace CollabNest.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [StringLength(500)]
        public string Bio { get; set; } = "";

        [StringLength(300)]
        public string Skills { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<CollabRequest> SentRequests { get; set; } = new List<CollabRequest>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    }

    public class Project
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [StringLength(300)]
        public string RequiredSkills { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? Owner { get; set; }

        public ICollection<CollabRequest> CollabRequests { get; set; } = new List<CollabRequest>();
    }

    public class CollabRequest
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        [StringLength(500)]
        public string Message { get; set; } = "";

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class PasswordResetToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string Token { get; set; } = "";

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}