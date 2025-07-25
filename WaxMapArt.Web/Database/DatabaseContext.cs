﻿using Microsoft.EntityFrameworkCore;

namespace WaxMapArt.Web.Database;

public class DatabaseContext(IConfiguration config, ILogger<DatabaseContext> logger) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {                               
        optionsBuilder.UseNpgsql(config["DatabaseConnectionString"]);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<Palette> Palettes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Block>()
            .HasIndex(b => b.MinecraftId)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
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
    public string? ImageUrl { get; set; }
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