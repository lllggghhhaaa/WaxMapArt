using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using WaxMapArt.Web.Components;
using WaxMapArt.Web.Database;
using WaxMapArt.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContextFactory<DatabaseContext>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication("CustomAuth")
    .AddCookie("CustomAuth", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection(); 
}

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

var nodeModulesPath = Path.Combine(builder.Environment.ContentRootPath, "node_modules");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(nodeModulesPath),
    RequestPath = "/node_modules"
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();