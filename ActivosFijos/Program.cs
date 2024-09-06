using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

//localization & globalization
builder.Services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddControllersWithViews(options =>
   options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
);

builder.Services.AddRazorPages();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<aftContext>(options =>
{
    //options.UseInMemoryDatabase("AgilScrum").UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.UseSqlServer(builder.Configuration.GetConnectionString("aftConnection"), opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
});

builder.Services.AddDbContext<aftContextAux>(options =>
{
    //options.UseInMemoryDatabase("AgilScrum").UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.UseSqlServer(builder.Configuration.GetConnectionString("aftConnection"), opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultScheme = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie("Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = new PathString("/ctAutenticacion");
    options.AccessDeniedPath = new PathString("/ctAutenticacion");
    options.ReturnUrlParameter = "returnUrl";
});
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ActivosFijos.Interfaces.iUtiles, repoUtiles>();
builder.Services.AddScoped<repoAreasDeResponsabilidad>();
builder.Services.AddScoped<repoValidaciones>();
builder.Services.AddScoped<repoCargos>();
builder.Services.AddScoped<repoResponsables>();
builder.Services.AddScoped<repoConfiguracion>();
builder.Services.AddScoped<repoCentrosDeCosto>();
builder.Services.AddScoped<repoActivosFijos>();
builder.Services.AddScoped<repoImportarDatos>();
builder.Services.AddScoped<DataTable>();
builder.Services.AddScoped<repoServcioActas>();
builder.Services.AddScoped<repoUsuarios>();
builder.Services.AddScoped<repoUtilesHerramientas>();
builder.Services.AddScoped<repoClasificador>();
builder.Services.AddHttpContextAccessor();

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

app.UseRouting();

app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=ctAutenticacion}/{action=Autenticacion}/{id?}");

    endpoints.MapControllerRoute(
            name: "core",
            pattern: "{controller=ctUsuario}/{action=ListadoDeUsuarios}/{filterscount?}/{groupscount?}/{pagenum?}/{pagesize?}/{recordstartindex?}/{recordendindex?}/{_?}");
});

app.Run();
