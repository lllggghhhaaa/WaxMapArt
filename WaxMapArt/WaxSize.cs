namespace WaxMapArt;

public record struct WaxSize(int X, int Y)
{
    public static WaxSize operator *(WaxSize waxSize, int multiplier) =>
        new(waxSize.X * multiplier,
            waxSize.Y * multiplier);

    public WaxSize ClampMax(int maxSize)
    {
        double ratio = Math.Min((double)maxSize / X, (double)maxSize / Y);
        int x = (int)(X * ratio);
        int y = (int)(Y * ratio);

        return new(x, y);
    }
}