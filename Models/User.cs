using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Avatar { get; set; }
        
        public bool IsOnline { get; set; } = false;
        
        public DateTime LastSeen { get; set; } = DateTime.Now;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Benzersiz oturum kimliği
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        // Benzersiz kullanıcı kodu (arkadaşlık istekleri için)
        [StringLength(10)]
        public string? UniqueCode { get; set; }
    }
} 