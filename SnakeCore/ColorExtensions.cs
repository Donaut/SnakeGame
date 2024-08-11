using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeCore;

internal static class ColorExtensions
{
    public static Color Light(this Color color, float percOfLightLight)
    {
        return Color.FromArgb(
            (byte)(color.R - (byte)((byte.MaxValue - color.R) * percOfLightLight)),
            (byte)(color.G - (byte)((byte.MaxValue - color.G) * percOfLightLight)),
            (byte)(color.B - (byte)((byte.MaxValue - color.B) * percOfLightLight)));
    }

    public static Color Dark(this Color color, float percOfDarkDark)
    {
        return Color.FromArgb(
             (byte)(color.R * (1 - percOfDarkDark)),
             (byte)(color.G * (1 - percOfDarkDark)),
             (byte)(color.B * (1 - percOfDarkDark)));
    }

    public static Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float red = color.R;
        float green = color.G;
        float blue = color.B;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
    }
}
