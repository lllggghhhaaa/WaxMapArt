using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WaxMapArt.Web.Database;

namespace WaxMapArt.Web.Services;

public class CustomAuthenticationStateProvider(
    IHttpContextAccessor httpContextAccessor,
    IDbContextFactory<DatabaseContext> dbFactory,
    IConfiguration configuration,
    ILogger<CustomAuthenticationStateProvider> logger)
    : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || !httpContext.Request.Cookies.TryGetValue("AuthToken", out var token) || string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
            var jwtIssuer = configuration["JwtSettings:Issuer"] ?? "WaxMapArt";
            var jwtAudience = configuration["JwtSettings:Audience"] ?? "WaxMapArt";

            var validationParameters = new TokenValidationParameters
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

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            await using var dbContext = await dbFactory.CreateDbContextAsync();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                logger.LogWarning("User {UserId} not found in database, invalidating token", userId);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name ?? string.Empty),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "JWT");
            var freshPrincipal = new ClaimsPrincipal(identity);

            return new AuthenticationState(freshPrincipal);
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Invalid JWT token");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
