using AvitoClone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvitoClone.Data;

namespace AvitoClone.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /User/Register (регистрация)
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register (обработка регистрации)
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Ad"); // После регистрации → на главную
            }
            return View(user);
        }

        // GET: /User/Login (вход)
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login (проверка логина/пароля)
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == user.Password);

            if (existingUser == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(user);
            }

            // TODO: Добавить куки/сессии (пока просто редирект)
            return RedirectToAction("Index", "Ad");
        }
    }
}