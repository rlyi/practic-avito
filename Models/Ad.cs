// Models/Ad.cs
namespace AvitoClone.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImagePath { get; set; } // Путь к изображению
        //public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
    
    
}