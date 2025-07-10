// Models/Message.cs
namespace AvitoClone.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        // Связь с объявлением
        public int AdId { get; set; }
        public Ad Ad { get; set; } = null!;
        
        // Связь с пользователем (отправитель)
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}