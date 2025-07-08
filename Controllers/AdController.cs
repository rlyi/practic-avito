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

        // GET: /Ad (список всех объявлений)
        // public async Task<IActionResult> Index()
        // {
        //     var ads = await _context.Ads
        //         .Include(a => a.User)
        //         .Include(a => a.Category)
        //         .OrderByDescending(a => a.CreatedAt)
        //         .ToListAsync();
        //     return View(ads);
        // }

        // public async Task<IActionResult> Index()
        // {
        //     var ads = await _context.Ads
        //         .Include(a => a.User)
        //         .Include(a => a.Category)
        //         .OrderByDescending(a => a.CreatedAt)
        //         .ToListAsync();

        //     // Добавляем проверку на null и возвращаем пустой список, если нет данных
        //     return View(ads ?? new List<Ad>());
        // }

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

            // return RedirectToAction(nameof(Index));
            //return RedirectToAction("Index", "Ad"); // вместо nameof(Index)
            //верный редирект на html ||||| return Redirect($"/ads/ad_{ad.Id}.html");
            //return Redirect($"/GeneratedAds/Ad_{ad.Id}.cshtml");
            // Редирект через именованный маршрут
            return RedirectToRoute("generated_ads", new { id = ad.Id });

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

        // private void GenerateAdPage(Ad ad)
        // {
        //     // 1. Проверяем, что шаблон существует
        //     var templatePath = Path.Combine(_env.WebRootPath, "ads_templates", "ad_template.html");
        //     if (!System.IO.File.Exists(templatePath))
        //     {
        //         throw new FileNotFoundException("Шаблон ad_template.html не найден!");
        //     }

        //     // 2. Читаем содержимое шаблона
        //     var templateContent = System.IO.File.ReadAllText(templatePath);
        //     if (string.IsNullOrEmpty(templateContent))
        //     {
        //         throw new InvalidOperationException("Шаблон пустой!");
        //     }

        //     // 3. Проверяем, что связанные данные (User и Category) загружены
        //     if (ad.User == null || ad.Category == null)
        //     {
        //         // Явно подгружаем данные, если они не загружены
        //         _context.Entry(ad).Reference(a => a.User).Load();
        //         _context.Entry(ad).Reference(a => a.Category).Load();
        //     }

        //     // 4. Заменяем плейсхолдеры с проверкой на null
        //     var htmlContent = templateContent
        //         .Replace("{{TITLE}}", ad.Title ?? "")
        //         .Replace("{{AUTHOR}}", ad.User?.Username ?? "")
        //         .Replace("{{DATE}}", ad.CreatedAt.ToString("dd.MM.yyyy"))
        //         .Replace("{{CATEGORY}}", ad.Category?.Name ?? "")
        //         .Replace("{{DESCRIPTION}}", ad.Description ?? "")
        //         .Replace("{{PRICE}}", ad.Price.ToString("C"));

        //     // 5. Блок с изображением (если есть)
        //     var imageSection = !string.IsNullOrEmpty(ad.ImagePath)
        //         ? $"<img src=\"{ad.ImagePath}\" class=\"img-fluid ad-img mb-3\">"
        //         : "";
        //     htmlContent = htmlContent.Replace("{{IMAGE_SECTION}}", imageSection);

        //     // 6. Создаем папку, если её нет
        //     var outputDir = Path.Combine(_env.WebRootPath, "ads");
        //     Directory.CreateDirectory(outputDir); // Гарантированно создаст папку

        //     // 7. Сохраняем HTML-страницу
        //     var outputPath = Path.Combine(outputDir, $"ad_{ad.Id}.html");
        //     System.IO.File.WriteAllText(outputPath, htmlContent);
        // }

        private void GenerateAdPage1(Ad ad)
        {
            // 1. Загружаем шаблон
            var templatePath = Path.Combine(_env.ContentRootPath, "Views", "Ad", "AdTemplate.cshtml");
            var templateContent = System.IO.File.ReadAllText(templatePath);

            // 2. Подготавливаем данные для замены
            var replacements = new Dictionary<string, string>
            {
                ["@Model.Title"] = ad.Title ?? "",
                ["@Model.User?.Username"] = ad.User?.Username ?? "",
                ["@Model.CreatedAt.ToString(\"dd.MM.yyyy\")"] = ad.CreatedAt.ToString("dd.MM.yyyy"),
                ["@Model.Category?.Name"] = ad.Category?.Name ?? "",
                ["@Model.Description"] = ad.Description ?? "",
                ["@Model.Price.ToString(\"C\")"] = ad.Price.ToString("C"),
                ["@Model.ImagePath"] = ad.ImagePath ?? ""
            };

            // 3. Заменяем плейсхолдеры
            foreach (var replacement in replacements)
            {
                templateContent = templateContent.Replace(replacement.Key, replacement.Value);
            }

            // 4. Генерируем путь для сохранения
            var outputDir = Path.Combine(_env.ContentRootPath, "Views", "GeneratedAds");
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, $"Ad_{ad.Id}.cshtml");

            // 5. Сохраняем CSHTML-файл
            System.IO.File.WriteAllText(outputPath, templateContent);
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

        public IActionResult ViewGenerated1(int id)
        {
            // Проверяем существование файла
            var viewPath = $"~/Views/GeneratedAds/Ad_{id}.cshtml";
            var fullPath = Path.Combine(_env.ContentRootPath, "Views", "GeneratedAds", $"Ad_{id}.cshtml");

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            // Загружаем данные объявления
            var ad = _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return NotFound();
            }

            return View(viewPath, ad);
        }

    }
}