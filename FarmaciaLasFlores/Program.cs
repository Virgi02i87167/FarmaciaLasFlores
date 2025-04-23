using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using FarmaciaLasFlores.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de autenticaci�n con cookies antes de construir la aplicaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";  // P�gina de inicio de sesi�n
        options.LogoutPath = "/Login/"; // P�gina de cierre de sesi�n
    });

// Agregar servicios de controladores con vistas
builder.Services.AddControllersWithViews();

// Aqu� se configura la conexi�n a la base de datos (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession();

builder.Services.AddScoped<VentasService>();

QuestPDF.Settings.License = LicenseType.Community;

// Ahora construimos la aplicaci�n
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

// Activar la autenticaci�n y autorizaci�n
app.UseAuthentication();  // Agregar la autenticaci�n antes de UseAuthorization
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

