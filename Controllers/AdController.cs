using AvitoClone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvitoClone.Data;
using System.Security.Claims;
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

        // GET: /Ad (список всех объявлений)
        public async Task<IActionResult> Index()
        {
            var ads = await _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
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

            // return RedirectToAction(nameof(Index));
            //return RedirectToAction("Index", "Ad"); // вместо nameof(Index)
            return Redirect($"/ads/ad_{ad.Id}.html");
        }


        // private void GenerateAdPage(Ad ad)
        // {
        //     // 1. Путь к шаблону
        //     var templatePath = Path.Combine(_env.WebRootPath, "ads_templates", "ad_template.html");

        //     // 2. Читаем шаблон
        //     var templateContent = System.IO.File.ReadAllText(templatePath);

        //     // 3. Заменяем плейсхолдеры
        //     var htmlContent = templateContent
        //         .Replace("{{TITLE}}", ad.Title)
        //         .Replace("{{AUTHOR}}", ad.User.Username)
        //         .Replace("{{DATE}}", ad.CreatedAt.ToString("dd.MM.yyyy"))
        //         .Replace("{{CATEGORY}}", ad.Category.Name)
        //         .Replace("{{DESCRIPTION}}", ad.Description)
        //         .Replace("{{PRICE}}", ad.Price.ToString("C"));

        //     // 4. Блок с изображением (если есть)
        //     var imageSection = !string.IsNullOrEmpty(ad.ImagePath)
        //         ? $"<img src=\"{ad.ImagePath}\" class=\"img-fluid ad-img mb-3\">"
        //         : "";
        //     htmlContent = htmlContent.Replace("{{IMAGE_SECTION}}", imageSection);

        //     // 5. Сохраняем готовую страницу
        //     var outputPath = Path.Combine(_env.WebRootPath, "ads", $"ad_{ad.Id}.html");
        //     Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!); 
        //     System.IO.File.WriteAllText(outputPath, htmlContent);
        // }

        private void GenerateAdPage(Ad ad)
        {
            // 1. Проверяем, что шаблон существует
            var templatePath = Path.Combine(_env.WebRootPath, "ads_templates", "ad_template.html");
            if (!System.IO.File.Exists(templatePath))
            {
                throw new FileNotFoundException("Шаблон ad_template.html не найден!");
            }

            // 2. Читаем содержимое шаблона
            var templateContent = System.IO.File.ReadAllText(templatePath);
            if (string.IsNullOrEmpty(templateContent))
            {
                throw new InvalidOperationException("Шаблон пустой!");
            }

            // 3. Проверяем, что связанные данные (User и Category) загружены
            if (ad.User == null || ad.Category == null)
            {
                // Явно подгружаем данные, если они не загружены
                _context.Entry(ad).Reference(a => a.User).Load();
                _context.Entry(ad).Reference(a => a.Category).Load();
            }

            // 4. Заменяем плейсхолдеры с проверкой на null
            var htmlContent = templateContent
                .Replace("{{TITLE}}", ad.Title ?? "")
                .Replace("{{AUTHOR}}", ad.User?.Username ?? "")
                .Replace("{{DATE}}", ad.CreatedAt.ToString("dd.MM.yyyy"))
                .Replace("{{CATEGORY}}", ad.Category?.Name ?? "")
                .Replace("{{DESCRIPTION}}", ad.Description ?? "")
                .Replace("{{PRICE}}", ad.Price.ToString("C"));

            // 5. Блок с изображением (если есть)
            var imageSection = !string.IsNullOrEmpty(ad.ImagePath)
                ? $"<img src=\"{ad.ImagePath}\" class=\"img-fluid ad-img mb-3\">"
                : "";
            htmlContent = htmlContent.Replace("{{IMAGE_SECTION}}", imageSection);

            // 6. Создаем папку, если её нет
            var outputDir = Path.Combine(_env.WebRootPath, "ads");
            Directory.CreateDirectory(outputDir); // Гарантированно создаст папку

            // 7. Сохраняем HTML-страницу
            var outputPath = Path.Combine(outputDir, $"ad_{ad.Id}.html");
            System.IO.File.WriteAllText(outputPath, htmlContent);
        }

    }
}