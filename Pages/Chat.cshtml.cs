using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Pages
{
    public class ChatModel : PageModel
    {
        private readonly ChatDbContext _context;

        public ChatModel(ChatDbContext context)
        {
            _context = context;
        }

        public IList<Message> Messages { get; set; } = new List<Message>();
        public IList<User> Users { get; set; } = new List<User>();
        public IList<Conversation> Conversations { get; set; } = new List<Conversation>();

        public async Task OnGetAsync()
        {
            Users = await _context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();

            Conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }
    }
} 