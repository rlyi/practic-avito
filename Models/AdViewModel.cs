// Models/AdViewModel.cs
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations; // Добавляем для атрибутов валидации
using Microsoft.AspNetCore.Http; // Для IFormFile

namespace AvitoClone.Models
{
    public class AdViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Обязательное поле")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Обязательное поле")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        public int CategoryId { get; set; }

        [Display(Name = "Изображение")]
        public IFormFile? ImageFile { get; set; }
    }
}