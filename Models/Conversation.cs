using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        
        public int User1Id { get; set; }
        public User User1 { get; set; } = null!;
        
        public int User2Id { get; set; }
        public User User2 { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime LastMessageAt { get; set; } = DateTime.Now;
    }
} 