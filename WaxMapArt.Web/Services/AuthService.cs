using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WaxMapArt.Web.Database;

namespace WaxMapArt.Web.Services;

public class AuthService(IDbContextFactory<DatabaseContext> dbFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<AuthService> logger)
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Name == username);
        
        if (user?.Name is null)
            return false;

        var passwordEquals = await ComparePassword(user, password);
        if (!passwordEquals)
            return false;

        var token = GenerateJwtToken(user);
        SetAuthCookie(token);

        return true;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();

        if (await dbContext.Users.AnyAsync(u => u.Name == username))
            return false;

        var salt = CreateSalt();

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 2,
        };

        var hash = await argon2.GetBytesAsync(512);
        
        if (hash is null) return false;
        
        var user = new User
        {
            Name = username,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        var token = GenerateJwtToken(user);
        SetAuthCookie(token);
        
        return true;
    }

    public Task LogoutAsync()
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete("AuthToken");
        return Task.CompletedTask;
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
    }
    
    public User? GetCurrentUser()
    {
        var dbContext = dbFactory.CreateDbContext();

        var userId = GetCurrentUserId();
        return userId.HasValue ? dbContext.Users.FirstOrDefault(u => u.Id == userId) : null;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
        var jwtIssuer = configuration["JwtSettings:Issuer"] ?? "WaxMapArt";
        var jwtAudience = configuration["JwtSettings:Audience"] ?? "WaxMapArt";
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SetAuthCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };

        httpContextAccessor.HttpContext?.Response.Cookies.Append("AuthToken", token, cookieOptions);
    }

    private byte[] CreateSalt()
    {
        var salt = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        return salt;
    }

    public async Task<bool> ComparePassword(User user, string password)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = user.PasswordSalt,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 2
        };
        
        var hash = await argon2.GetBytesAsync(512);

        return hash is not null && hash.SequenceEqual(user.PasswordHash);
    }
    
    public async Task<bool> UpdateUsernameAsync(Guid userId, string newUsername)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (await dbContext.Users.AnyAsync(u => u.Name == newUsername && u.Id != userId))
            return false;

        user.Name = newUsername;
        await dbContext.SaveChangesAsync();
    
        return true;
    }

    public async Task<bool> UpdatePasswordAsync(User user, string newPassword)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();

        var salt = CreateSalt();

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(newPassword))
        {
            Salt = salt,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 2,
        };

        var hash = await argon2.GetBytesAsync(512);
    
        if (hash is null) return false;

        user.PasswordHash = hash;
        user.PasswordSalt = salt;
    
        await dbContext.SaveChangesAsync();
    
        return true;
    }
}