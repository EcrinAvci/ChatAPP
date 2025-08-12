using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatApiController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public ChatApiController(ChatDbContext context)
        {
            _context = context;
        }

        // GET: api/ChatApi/messages?userId=1&otherId=2
        [HttpGet("messages")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages([FromQuery] int userId, [FromQuery] int otherId)
        {
            // Sadece iki kullanıcı arasındaki mesajlar
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherId) || (m.SenderId == otherId && m.ReceiverId == userId))
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .ToListAsync();
        }

        // GET: api/ChatApi/unread-count?userId=1
        [HttpGet("unread-count")]
        public async Task<ActionResult<Dictionary<int, int>>> GetUnreadCount([FromQuery] int userId)
        {
            var unreadCounts = await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = unreadCounts.ToDictionary(x => x.SenderId ?? 0, x => x.Count);
            return result;
        }

        // POST: api/ChatApi/message
        [HttpPost("message")]
        public async Task<ActionResult<Message>> PostMessage([FromForm] IFormCollection form)
        {
            var content = form["content"].ToString();
            var senderIdStr = form["senderId"].ToString();
            var receiverIdStr = form["receiverId"].ToString();
            
            if (!int.TryParse(senderIdStr, out int senderId) || !int.TryParse(receiverIdStr, out int receiverId))
            {
                return BadRequest("Geçersiz SenderId veya ReceiverId.");
            }

            var sender = await _context.Users.FindAsync(senderId);
            var receiver = await _context.Users.FindAsync(receiverId);
            if (sender == null || receiver == null)
            {
                return BadRequest("Kullanıcı(lar) bulunamadı.");
            }

            var message = new Message
            {
                Content = content ?? string.Empty,
                SenderName = sender.Name,
                SenderId = senderId,
                ReceiverId = receiverId,
                Timestamp = DateTime.Now,
                IsRead = false
            };

            // Fotoğraf yükleme işlemi
            var imageFile = form.Files.FirstOrDefault(f => f.Name == "image");
            if (imageFile != null && imageFile.Length > 0)
            {
                // Dosya boyut kontrolü (100MB)
                const long maxFileSize = 100 * 1024 * 1024; // 100MB
                if (imageFile.Length > maxFileSize)
                {
                    return BadRequest($"Dosya boyutu çok büyük. Maksimum dosya boyutu: 100MB");
                }

                // Dosya türü kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Sadece resim dosyaları kabul edilir: jpg, jpeg, png, gif, bmp, webp");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Resim URL'sini tam URL olarak kaydet
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                message.ImageUrl = $"{baseUrl}/uploads/{fileName}";
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessages), new { userId = senderId, otherId = receiverId }, message);
        }

        // POST: api/ChatApi/mark-read
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkReadRequest request)
        {
            if (!request.UserId.HasValue || !request.SenderId.HasValue)
            {
                return BadRequest("UserId ve SenderId alanları gereklidir.");
            }

            var messages = await _context.Messages
                .Where(m => m.ReceiverId == request.UserId && m.SenderId == request.SenderId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/ChatApi/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.OrderBy(u => u.Name).ToListAsync();
        }

        // GET: api/ChatApi/friends?userId=1
        [HttpGet("friends")]
        public async Task<ActionResult<IEnumerable<User>>> GetFriends([FromQuery] int userId)
        {
            // Kullanıcının kabul ettiği arkadaşlık isteklerini bul
            var acceptedRequests = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Where(fr => fr.Status == FriendRequestStatus.Accepted && 
                           (fr.SenderId == userId || fr.ReceiverId == userId))
                .ToListAsync();

            var friends = new List<User>();
            foreach (var request in acceptedRequests)
            {
                if (request.SenderId == userId)
                {
                    friends.Add(request.Receiver);
                }
                else
                {
                    friends.Add(request.Sender);
                }
            }

            return friends.OrderBy(u => u.Name).ToList();
        }

        // GET: api/ChatApi/user/{id}
        [HttpGet("user/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // GET: api/ChatApi/user/by-code/{code}
        [HttpGet("user/by-code/{code}")]
        public async Task<ActionResult<User>> GetUserByCode(string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UniqueCode == code);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }
            return user;
        }

        // POST: api/ChatApi/user
        [HttpPost("user")]
        public async Task<ActionResult<User>> PostUser([FromBody] UserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name alanı gereklidir.");
            }

            // Aynı SessionId ile kullanıcı varsa onu döndür
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.SessionId == request.SessionId);
                if (existing != null)
                {
                    existing.LastSeen = DateTime.Now;
                    existing.IsOnline = true;
                    await _context.SaveChangesAsync();
                    return existing;
                }
            }

            // Her zaman yeni kullanıcı oluştur (isim aynı olsa bile)
            var user = new User
            {
                Name = request.Name,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                UniqueCode = await GenerateUniqueCode(),
                CreatedAt = DateTime.Now,
                LastSeen = DateTime.Now,
                IsOnline = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        // POST: api/ChatApi/user/update-status
        [HttpPost("user/update-status")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateStatusRequest request)
        {
            if (!request.UserId.HasValue)
            {
                return BadRequest("UserId alanı gereklidir.");
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            user.IsOnline = request.IsOnline;
            user.LastSeen = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/ChatApi/user/{id}
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Kullanıcıya ait tüm mesajları sil
            var messages = await _context.Messages
                .Where(m => m.SenderId == id || m.ReceiverId == id)
                .ToListAsync();
            _context.Messages.RemoveRange(messages);

            // Kullanıcıya ait tüm arkadaşlık isteklerini sil
            var friendRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == id || fr.ReceiverId == id)
                .ToListAsync();
            _context.FriendRequests.RemoveRange(friendRequests);

            // Kullanıcıya ait tüm konuşmaları sil
            var conversations = await _context.Conversations
                .Where(c => c.User1Id == id || c.User2Id == id)
                .ToListAsync();
            _context.Conversations.RemoveRange(conversations);

            // Kullanıcıyı sil
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/ChatApi/friend-requests?userId=1
        [HttpGet("friend-requests")]
        public async Task<ActionResult<IEnumerable<FriendRequest>>> GetFriendRequests([FromQuery] int userId)
        {
            return await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        // POST: api/ChatApi/friend-request
        [HttpPost("friend-request")]
        public async Task<ActionResult<FriendRequest>> SendFriendRequest([FromBody] FriendRequestRequest request)
        {
            if (!request.SenderId.HasValue || !request.ReceiverId.HasValue)
            {
                return BadRequest("SenderId ve ReceiverId alanları gereklidir.");
            }

            // Aynı istek zaten var mı kontrol et
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    (fr.SenderId == request.SenderId && fr.ReceiverId == request.ReceiverId) ||
                    (fr.SenderId == request.ReceiverId && fr.ReceiverId == request.SenderId));

            if (existingRequest != null)
            {
                return BadRequest("Bu kullanıcıya zaten arkadaşlık isteği gönderilmiş.");
            }

            var sender = await _context.Users.FindAsync(request.SenderId);
            var receiver = await _context.Users.FindAsync(request.ReceiverId);
            
            if (sender == null || receiver == null)
            {
                return BadRequest("Kullanıcı bulunamadı.");
            }

            var friendRequest = new FriendRequest
            {
                SenderId = request.SenderId.Value,
                ReceiverId = request.ReceiverId.Value,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFriendRequests), new { userId = request.ReceiverId }, friendRequest);
        }

        // POST: api/ChatApi/friend-request/{id}/accept
        [HttpPost("friend-request/{id}/accept")]
        public async Task<IActionResult> AcceptFriendRequest(int id)
        {
            var friendRequest = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .FirstOrDefaultAsync(fr => fr.Id == id);

            if (friendRequest == null)
            {
                return NotFound("Arkadaşlık isteği bulunamadı.");
            }

            friendRequest.Status = FriendRequestStatus.Accepted;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/ChatApi/friend-request/{id}/reject
        [HttpPost("friend-request/{id}/reject")]
        public async Task<IActionResult> RejectFriendRequest(int id)
        {
            var friendRequest = await _context.FriendRequests.FindAsync(id);
            if (friendRequest == null)
            {
                return NotFound("Arkadaşlık isteği bulunamadı.");
            }

            friendRequest.Status = FriendRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/ChatApi/test-image/{fileName}
        [HttpGet("test-image/{fileName}")]
        public IActionResult TestImage(string fileName)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(filePath);
                return File(fileBytes, contentType);
            }
            
            return NotFound($"Dosya bulunamadı: {fileName}");
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private async Task<string> GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            
            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (await _context.Users.AnyAsync(u => u.UniqueCode == code));
            
            return code;
        }

        // POST: api/ChatApi/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Kullanıcı adı zorunludur.");

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.Name);
            if (existing != null)
                return BadRequest("Bu kullanıcı adı zaten kayıtlı.");

            var user = new User
            {
                Name = request.Name,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                UniqueCode = await GenerateUniqueCode(),
                CreatedAt = DateTime.Now,
                LastSeen = DateTime.Now,
                IsOnline = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // POST: api/ChatApi/login
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] UserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Kullanıcı adı zorunludur.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.Name);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı. Lütfen önce kayıt olun.");

            user.LastSeen = DateTime.Now;
            user.IsOnline = true;
            await _context.SaveChangesAsync();
            return user;
        }
    }

    public class MessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
    }

    public class UserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }

    public class MarkReadRequest
    {
        public int? UserId { get; set; }
        public int? SenderId { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int? UserId { get; set; }
        public bool IsOnline { get; set; }
    }

    public class FriendRequestRequest
    {
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
    }
} 