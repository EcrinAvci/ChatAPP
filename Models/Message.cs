using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string SenderName { get; set; } = string.Empty;
        
        public int? SenderId { get; set; }
        public User? Sender { get; set; }
        
        public int? ReceiverId { get; set; }
        public User? Receiver { get; set; }
        
        public int? ConversationId { get; set; }
        public Conversation? Conversation { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
        
        public string? ImageUrl { get; set; }
    }
} 