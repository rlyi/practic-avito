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

        // public async Task<IActionResult> Index()
        // {
        //     // Получаем объявления с включением связанных данных
        //     var ads = await _context.Ads
        //         .Include(a => a.User)       // Подгружаем данные пользователя
        //         .Include(a => a.Category)   // Подгружаем данные категории
        //         .OrderByDescending(a => a.CreatedAt) // Сортируем по дате (новые сначала)
        //         .ToListAsync();

        //     // Передаем список объявлений в представление
        //     return View(ads);
        // }

        public async Task<IActionResult> Index1(string searchQuery, int? categoryId)
        {
            var query = _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(a =>
                    a.Title.Contains(searchQuery) ||
                    a.Category.Name.Contains(searchQuery));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId);
            }

            var ads = await query.ToListAsync();
            return View(ads);
        }

        public async Task<IActionResult> Index(string searchQuery, int? categoryId)
        {
            // Загружаем категории для выпадающего списка
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var query = _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(a =>
                    a.Title.Contains(searchQuery) ||
                    a.Description.Contains(searchQuery) ||
                    a.Category.Name.Contains(searchQuery));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId);
            }

            var ads = await query.ToListAsync();
            return View(ads);
        }

    }
}