using AvitoClone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvitoClone.Data;

namespace AvitoClone.Controllers
{
    public class AdController : Controller
    {
        private readonly AppDbContext _context;

        public AdController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Ad (список всех объявлений)
        public async Task<IActionResult> Index()
        {
            var ads = await _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .ToListAsync();
            return View(ads);
        }

        // GET: /Ad/Details/5 (просмотр одного объявления)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad == null) return NotFound();

            return View(ad);
        }

        // GET: /Ad/Create (форма добавления)
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList(); // Список категорий для выпадающего меню
            return View();
        }

        // POST: /Ad/Create (обработка формы)
        [HttpPost]
        public async Task<IActionResult> Create(Ad ad)
        {
            if (ModelState.IsValid)
            {
                _context.Ads.Add(ad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(ad);
        }
    }
}