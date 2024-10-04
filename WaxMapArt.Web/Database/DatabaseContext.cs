using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WaxMapArt.Web.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration config) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(config["DatabaseConnectionString"]);

    public DbSet<Block> Blocks { get; set; }
    public DbSet<Palette> Palettes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Block>()
            .Property(b => b.Id);

        modelBuilder.Entity<Palette>()
            .Property(p => p.Id);

        
        modelBuilder.Entity<Block>()
            .HasIndex(b => b.MinecraftId)
            .IsUnique();

        modelBuilder.Entity<Palette>()
            .HasMany(p => p.Blocks)
            .WithMany(b => b.Palettes)
            .UsingEntity(j => j.ToTable("PaletteBlocks"));

        modelBuilder.Entity<Palette>()
            .HasOne(p => p.PlaceholderBlock)
            .WithMany()
            .HasForeignKey(p => p.PlaceholderBlockId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Palettes)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);
        
        modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.HasKey(r => new { r.UserId, r.RoleId });
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
        });
    }
}

public class Block
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string MinecraftId { get; set; }
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
        GeneratorProperties = new()
        {
            NeedSupport = NeedSupport
        }
    };
}

public class Palette
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }

    public Guid PlaceholderBlockId { get; set; }
    public Block PlaceholderBlock { get; set; }

    public List<Block> Blocks { get; set; } = [];

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public static implicit operator WaxMapArt.Palette(Palette value) => new()
    {
        Name = value.Name,
        PlaceholderColor = value.PlaceholderBlock.ToPaletteColor(),
        Colors = value.Blocks.Select(b => b.ToPaletteColor()).ToArray()
    };
}

public class ApplicationUser : IdentityUser
{
    public List<Palette> Palettes { get; set; } = [];
}