using AvitoClone.Models;
using AvitoClone.Repositories;
using AvitoClone.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AvitoClone.Controllers
{
    public class ChatController : Controller
    {
        private readonly IMessageRepository _messageRepository;
        private readonly AppDbContext _context;

        public ChatController(IMessageRepository messageRepository, AppDbContext context)
        {
            _messageRepository = messageRepository;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] int adId, [FromForm] string text) // Изменили на [FromForm]
        {
            try
            {
                var username = HttpContext.Session.GetString("CurrentUser");
                if (string.IsNullOrEmpty(username))
                    return Json(new { error = "Требуется авторизация" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    HttpContext.Session.Remove("CurrentUser");
                    return Json(new { error = "Пользователь не найден" });
                }

                var message = new Message
                {
                    AdId = adId,
                    UserId = user.Id,
                    Text = text,
                    SentAt = DateTime.UtcNow
                };

                await _messageRepository.AddMessageAsync(message);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int adId)
        {
            var messages = await _context.Messages
                .Where(m => m.AdId == adId)
                .Include(m => m.User) // Важно: подгружаем пользователя
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    Sender = m.User.Username, // Явно указываем имена свойств
                    Text = m.Text,
                    Time = m.SentAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Json(new { Messages = messages }); // Явное именование массива
        }

        public class MessageRequest
        {
            public int AdId { get; set; }
            public string Text { get; set; } = null!; // Добавляем = null! для non-nullable свойства
        }
    }
}