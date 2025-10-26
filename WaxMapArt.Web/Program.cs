using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Minio;
using WaxMapArt.Web.Components;
using WaxMapArt.Web.Database;
using WaxMapArt.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
    .SetApplicationName("WaxMapArt");

builder.Services.AddHttpContextAccessor();

builder.AddNpgsqlDbContext<DatabaseContext>("waxmapart-db");
builder.Services.AddDbContextFactory<DatabaseContext>();
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(builder.Configuration["MinIO:Endpoint"] ?? throw new InvalidOperationException("MinIO Endpoint is not configured"))
    .WithCredentials(
        builder.Configuration["MinIO:AccessKey"] ?? throw new InvalidOperationException("MinIO Access Key is not configured"),
        builder.Configuration["MinIO:SecretKey"] ?? throw new InvalidOperationException("MinIO Secret Key is not configured"))
    .Build());

builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<UserProfileState>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped<MapGeneratorService>();

var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "WaxMapArt";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "WaxMapArt";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection(); 
}
else
{
    // Even in development, redirect to HTTPS
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();
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