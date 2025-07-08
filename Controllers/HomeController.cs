using AvitoClone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvitoClone.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace AvitoClone.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Получаем объявления с включением связанных данных
            var ads = await _context.Ads
                .Include(a => a.User)       // Подгружаем данные пользователя
                .Include(a => a.Category)   // Подгружаем данные категории
                .OrderByDescending(a => a.CreatedAt) // Сортируем по дате (новые сначала)
                .ToListAsync();

            // Передаем список объявлений в представление
            return View(ads);
        }

    }
}