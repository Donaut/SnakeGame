using System.Drawing;

namespace SnakeCore;

internal static class ColorExtensions
{
    public static Color Dark(this Color color, float percOfDarkDark)
    {
        return Color.FromArgb(
             (byte)(color.R * (1 - percOfDarkDark)),
             (byte)(color.G * (1 - percOfDarkDark)),
             (byte)(color.B * (1 - percOfDarkDark)));
    }
}
