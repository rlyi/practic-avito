// using AvitoClone.Models;
// using AvitoClone.Data;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace AvitoClone.Controllers
// {
//     public class UserController : Controller
//     {
//         private readonly AppDbContext _context;

//         public UserController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // GET: /User/Register
//         [HttpGet]
//         public IActionResult Register()
//         {
//             return View();
//         }

//         // POST: /User/Register
//         [HttpPost]
//         public async Task<IActionResult> Register(User user)
//         {
//             if (ModelState.IsValid)
//             {
//                 if (await _context.Users.AnyAsync(u => u.Username == user.Username))
//                 {
//                     ModelState.AddModelError("Username", "Имя пользователя занято");
//                     return View(user);
//                 }

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();
//                 return RedirectToAction("Index", "Ad");
//             }
//             return View(user);
//         }

//         // GET: /User/Login
//         [HttpGet]
//         public IActionResult Login()
//         {
//             return View();
//         }

//         // POST: /User/Login
//         [HttpPost]
//         public async Task<IActionResult> Login(User user)
//         {
//             var existingUser = await _context.Users
//                 .FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == user.Password);

//             if (existingUser == null)
//             {
//                 ModelState.AddModelError("", "Неверный логин или пароль");
//                 return View(user);
//             }

//             // Простейшая "авторизация" через ViewData
//             ViewData["CurrentUser"] = existingUser.Username;
//             return RedirectToAction("Index" , "Ad" );
//         }
//     }
// }
using AvitoClone.Models;
using AvitoClone.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace AvitoClone.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /User/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Проверка на уникальность имени пользователя
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Имя пользователя занято");
                    return View(user);
                }

                // Минимальная валидация пароля
                if (string.IsNullOrEmpty(user.Password) || user.Password.Length < 4)
                {
                    ModelState.AddModelError("Password", "Пароль должен содержать минимум 4 символа");
                    return View(user);
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Автоматический вход после регистрации
                HttpContext.Session.SetString("CurrentUser", user.Username);
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        // GET: /User/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Если уже авторизован - редирект на главную
            if (HttpContext.Session.GetString("CurrentUser") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /User/Login
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

            // Сохраняем в сессии
            HttpContext.Session.SetString("CurrentUser", existingUser.Username);
            return RedirectToAction("Index", "Home");
        }

        // GET: /User/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Очищаем сессию
            HttpContext.Session.Remove("CurrentUser");
            return RedirectToAction("Index", "Home");
        }

        // GET: /User/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("CurrentUser");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
                
            if (user == null)
            {
                HttpContext.Session.Remove("CurrentUser");
                return RedirectToAction("Login");
            }

            return View(user);
        }
    }
}