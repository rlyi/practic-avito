// Models/Category.cs
using System.ComponentModel.DataAnnotations; // Для атрибута [Required]
namespace AvitoClone.Models

{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public List<Ad> Ads { get; set; } = new List<Ad>();
    }
}