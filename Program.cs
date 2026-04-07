using Microsoft.EntityFrameworkCore;
using piuttec.Models;

var builder = WebApplication.CreateBuilder(args);

// Permite guardar datos como UserId y Rol en memoria del servidor
builder.Services.AddSession();

//Configurar DbContext con MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        "server=localhost;port=3306;database=piuttec;user=root;password=root",
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

// 
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Redirección inicial a Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Login");
    return Task.CompletedTask;
});

// Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();