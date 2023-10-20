namespace VideoConverter.VideoInformation.Models;

public readonly record struct Resolution(int Width, int Height)
{
    public Resolution ResizeToHeight(int newHeight)
    {
        int newWidth = Width * newHeight / Height;
        return this with { Width = newWidth, Height = newHeight };
    }

    public override readonly string ToString() => $"{Width}x{Height}";
}