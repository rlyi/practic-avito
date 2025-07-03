// Models/Ad.cs
using System.ComponentModel.DataAnnotations; // Для атрибута [Required]
namespace AvitoClone.Models
{
    public class Ad
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }

        [Required]
        public Category Category { get; set; } = null!;
    }
}