using AvitoClone.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using AvitoClone.Repositories;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Хранение сессий в памяти
builder.Services.AddSession(); // Включение сессий
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Views/GeneratedAds/{0}" + RazorViewEngine.ViewExtension);
});

// Program.cs
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Сохраняет регистр свойств
    });

// Добавляем DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB лимит
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//pattern: "{controller=Ad}/{action=Index}/{id?}");
    app.MapControllerRoute(
    name: "generated_ads",
    pattern: "ads/{id}",
    defaults: new { controller = "Ad", action = "ViewGenerated" });

    app.MapControllerRoute(
    name: "generated_ads",
    pattern: "GeneratedAds/ViewGenerated/{id}",
    defaults: new { controller = "GeneratedAds", action = "ViewGenerated" });
app.Run();
//program.cs