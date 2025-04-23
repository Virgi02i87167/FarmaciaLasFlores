using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using FarmaciaLasFlores.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de autenticación con cookies antes de construir la aplicación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";  // Página de inicio de sesión
        options.LogoutPath = "/Login/"; // Página de cierre de sesión
    });

// Agregar servicios de controladores con vistas
builder.Services.AddControllersWithViews();

// Aquí se configura la conexión a la base de datos (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession();

builder.Services.AddScoped<VentasService>();

QuestPDF.Settings.License = LicenseType.Community;

// Ahora construimos la aplicación
var app = builder.Build();

// Configurar el pipeline de HTTP

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Activar la autenticación y autorización
app.UseAuthentication();  // Agregar la autenticación antes de UseAuthorization
app.UseAuthorization();

// Bloque para migrar la base de datos y crear roles si es necesario
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Roles { NombreRoles = "Administrador" },
            new Roles { NombreRoles = "Empleado" }
        );
        context.SaveChanges();
    }
}

// Mapear las rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();

