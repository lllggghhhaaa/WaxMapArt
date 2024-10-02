using Microsoft.EntityFrameworkCore;
using WaxMapArt.Web.Database;

namespace WaxMapArt.Web.Services;

public class BlockService(IDbContextFactory<DatabaseContext> dbContextFactory)
{
    public async Task<List<Block>> GetBlocksAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Blocks.OrderBy(block => block.MapId).ToListAsync();
    }

    public async Task<Block?> GetBlockByIdAsync(Guid blockId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Blocks.FirstOrDefaultAsync(b => b.Id == blockId);
    }

    public async Task<List<Block>> GetBlocksByIdsAsync(List<Guid> blockIds)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Blocks
            .Where(b => blockIds.Contains(b.Id))
            .ToListAsync();
    }
    
    public async Task<Block> AddOrUpdateBlockAsync(Block block)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var existingBlock = await context.Blocks
            .FirstOrDefaultAsync(b => b.MinecraftId == block.MinecraftId);

        if (existingBlock != null)
        {
            // Update existing block
            context.Entry(existingBlock).CurrentValues.SetValues(block);
            block = existingBlock;
        }
        else
        {
            // Add new block
            await context.Blocks.AddAsync(block);
        }

        await context.SaveChangesAsync();
        return block;
    }

    public async Task DeleteBlockAsync(Guid blockId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var block = await context.Blocks.FindAsync(blockId);
        if (block != null)
        {
            context.Blocks.Remove(block);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Database.Palette?> GetPaletteByIdAsync(Guid paletteId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Palettes
            .Include(p => p.Blocks)
            .Include(p => p.PlaceholderBlock)
            .FirstOrDefaultAsync(p => p.Id == paletteId);
    }

    public async Task<List<Database.Palette>> GetPalettesAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Palettes
            .Include(p => p.Blocks)
            .Include(p => p.PlaceholderBlock)
            .ToListAsync();
    }

    public async Task AddPaletteAsync(Database.Palette palette, ApplicationUser user)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        palette.UserId = user.Id;
        context.Palettes.Add(palette);
        await context.SaveChangesAsync();
    }

    public async Task UpdatePaletteAsync(Database.Palette palette)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        var existingPalette = await context.Palettes
            .Include(p => p.Blocks)
            .FirstOrDefaultAsync(p => p.Id == palette.Id);

        if (existingPalette is null)
        {
            throw new InvalidOperationException("Palette not found");
        }

        context.Entry(existingPalette).CurrentValues.SetValues(palette);

        existingPalette.Blocks.Clear();
        foreach (var block in palette.Blocks)
        {
            var existingBlock = await context.Blocks
                .FirstOrDefaultAsync(b => b.MinecraftId == block.MinecraftId);
            existingPalette.Blocks.Add(existingBlock ?? block);
        }

        var placeholderBlock = await context.Blocks
            .FirstOrDefaultAsync(b => b.Id == palette.PlaceholderBlockId);
        if (placeholderBlock is null)
            throw new InvalidOperationException("PlaceholderBlock not found");
        
        existingPalette.PlaceholderBlock = placeholderBlock;

        await context.SaveChangesAsync();
    }

    public async Task DeletePaletteAsync(Guid paletteId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var palette = await context.Palettes.FindAsync(paletteId);
        if (palette != null)
        {
            context.Palettes.Remove(palette);
            await context.SaveChangesAsync();
        }
    }
}