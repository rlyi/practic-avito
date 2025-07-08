// Controllers/GeneratedAdsController.cs
using AvitoClone.Models;
using AvitoClone.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class GeneratedAdsController : Controller
{
    private readonly AppDbContext _context;

    public GeneratedAdsController(AppDbContext context)
    {
        _context = context;
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

        return View($"~/Views/GeneratedAds/Ad_{id}.cshtml", ad);
    }
}