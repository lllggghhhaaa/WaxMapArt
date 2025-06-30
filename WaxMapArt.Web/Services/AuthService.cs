using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WaxMapArt.Web.Database;

namespace WaxMapArt.Web.Services;

public class AuthService(IDbContextFactory<DatabaseContext> dbFactory, IHttpContextAccessor httpContextAccessor)
{
    private DatabaseContext _dbContext = dbFactory.CreateDbContext();
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Name == username);
        
        if (user?.Name is null)
            return false;

        var passwordEquals = await ComparePassword(user, password);
        if (!passwordEquals)
            return false;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        var principal = new ClaimsPrincipal(identity);

        await httpContextAccessor.HttpContext!.SignInAsync("CustomAuth", principal);

        return true;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Name == username))
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

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task LogoutAsync()
    {
        await httpContextAccessor.HttpContext!.SignOutAsync("CustomAuth");
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
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
            DegreeOfParallelism = 2,
        };
        
        var hash = await argon2.GetBytesAsync(512);

        return hash is not null && hash.SequenceEqual(user.PasswordHash);
    }
}