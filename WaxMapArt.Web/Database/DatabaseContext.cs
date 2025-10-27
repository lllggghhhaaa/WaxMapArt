using Microsoft.EntityFrameworkCore;

namespace WaxMapArt.Web.Database;

public class DatabaseContext(IConfiguration config) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {                               
        optionsBuilder.UseNpgsql(config["Database:ConnectionString"]);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<Palette> Palettes { get; set; }
    public DbSet<UserImage> Images { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Block>()
            .HasIndex(b => b.MinecraftId)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasOne(u => u.AvatarImage)
            .WithOne(i => i.User)
            .HasForeignKey<User>(u => u.AvatarImageId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Palette>()
            .HasMany(p => p.Blocks)
            .WithMany(b => b.Palettes)
            .UsingEntity(j => j.ToTable("PaletteBlocks"));

        modelBuilder.Entity<Palette>()
            .HasOne(p => p.PlaceholderBlock)
            .WithMany()
            .HasForeignKey(p => p.PlaceholderBlockId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Palette>()
            .HasOne(p => p.User)
            .WithMany(u => u.Palettes)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? Name { get; set; }
    
    public Guid? AvatarImageId { get; set; }
    public UserImage? AvatarImage { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public List<Palette> Palettes { get; set; } = [];
}

public enum UserRole
{
    User,
    Developer,
}


public class Block
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string MinecraftId { get; set; }
    public string? Name { get; set; }
    public int MapId { get; set; }
    public string? ImageUrl { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new();
    public bool NeedSupport { get; set; }

    public List<Palette> Palettes { get; set; } = [];

    public PaletteColor ToPaletteColor() => new()
    {
        Id = MinecraftId,
        MapId = MapId,
        Properties = Properties,
        GeneratorProperties = new GeneratorProperties
        {
            NeedSupport = NeedSupport
        }
    };
}

public class Palette
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }

    public Guid PlaceholderBlockId { get; set; }
    public Block PlaceholderBlock { get; set; }

    public List<Block> Blocks { get; set; } = [];

    public Guid UserId { get; set; }
    public User User { get; set; }

    public static implicit operator WaxMapArt.Palette(Palette value) => new()
    {
        Name = value.Name,
        PlaceholderColor = value.PlaceholderBlock.ToPaletteColor(),
        Colors = value.Blocks.Select(b => b.ToPaletteColor()).ToArray()
    };
}

public class UserImage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FileName { get; set; } = null!;
    public string ObjectName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User? User { get; set; }
}