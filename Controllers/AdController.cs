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
    // Только авторизованные пользователи
    public class AdController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            // Временный код для проверки
            Console.WriteLine("Количество объявлений в БД: " + _context.Ads.Count());

            var ads = await _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            Console.WriteLine("Загружено объявлений: " + ads.Count);
            return View(ads);
        }

        // GET: /Ad/Create (форма добавления)
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(model);
            }

            var username = HttpContext.Session.GetString("CurrentUser");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return RedirectToAction("Login", "User");

            string? imagePath = null;
            if (model.ImageFile is { Length: > 0 })
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var safeFileName = Path.GetFileName(model.ImageFile.FileName) ?? "image.jpg";
                var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/uploads/{uniqueFileName}";
            }

            var ad = new Ad
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                ImagePath = imagePath,
                UserId = user.Id,
                CategoryId = model.CategoryId,
                CreatedAt = DateTime.Now
            };

            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            GenerateAdPage(ad);

            return RedirectToRoute("generated_ads", new { id = ad.Id });

        }

        private void GenerateAdPage(Ad ad)
        {
            // 1. Загружаем шаблон
            var templatePath = Path.Combine(_env.ContentRootPath, "Views", "Ad", "AdTemplate.cshtml");
            var templateContent = System.IO.File.ReadAllText(templatePath);

            // 2. Подготавливаем данные
            var adData = new
            {
                Title = ad.Title ?? "",
                Author = ad.User?.Username ?? "",
                Date = ad.CreatedAt.ToString("dd.MM.yyyy"),
                Category = ad.Category?.Name ?? "",
                Description = ad.Description ?? "",
                Price = ad.Price.ToString("C"),
                ImagePath = ad.ImagePath ?? ""
            };

            // 3. Заменяем плейсхолдеры
            var resultContent = templateContent
                .Replace("@Model.Title", adData.Title)
                .Replace("@Model.User?.Username", adData.Author)
                .Replace("@Model.CreatedAt.ToString(\"dd.MM.yyyy\")", adData.Date)
                .Replace("@Model.Category?.Name", adData.Category)
                .Replace("@Model.Description", adData.Description)
                .Replace("@Model.Price.ToString(\"C\")", adData.Price)
                .Replace("@Model.ImagePath", adData.ImagePath);

            // 4. Сохраняем
            var outputDir = Path.Combine(_env.ContentRootPath, "Views", "GeneratedAds");
            Directory.CreateDirectory(outputDir);
            System.IO.File.WriteAllText(
                Path.Combine(outputDir, $"Ad_{ad.Id}.cshtml"),
                resultContent
            );
        }

        public IActionResult ViewGenerated(int id)
        {
            var ad = _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return NotFound();
            }

            return View($"Ad_{id}", ad);
        }

    }
}