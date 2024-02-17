using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});

builder.Services.AddTransient<IRepositorioTipoCuenta, RepositorioTipoCuenta>();
builder.Services.AddTransient<IServicioUsuario, ServicioUsuario>();
builder.Services.AddTransient<IRepositorioCuenta,RepositorioCuenta>();
builder.Services.AddTransient<IRepositorioCuenta,RepositorioCuenta>();
builder.Services.AddTransient<IRepositorioCategoria, RepositorioCategoria>();
builder.Services.AddTransient<IRepositorioTransaccion, RepositorioTransaccion>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireNonAlphanumeric = false;
    opciones.Password.RequireUppercase = false;

});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme,opciones =>
{
    opciones.LoginPath = "/usuarios/login";
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

app.UseRouting();

app.UseAuthentication();


app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transaccion}/{action=Index}/{id?}");

app.Run();
