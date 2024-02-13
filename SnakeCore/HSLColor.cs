using System.Drawing;


namespace SnakeCore
{
    /// <summary>
    /// https://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/
    /// </summary>
    public record struct HSLColor
    {
        public double Hue { get; set; }

        public double Saturation { get; set; }

        public double Luminosity { get; set; }

        public HSLColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }

        public HSLColor(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }

        public HSLColor(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }

        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }

        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor.Luminosity < 0.5)  //<=??
                temp2 = hslColor.Luminosity * (1.0 + hslColor.Saturation);
            else
                temp2 = hslColor.Luminosity + hslColor.Saturation - (hslColor.Luminosity * hslColor.Saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            HSLColor hslColor = new HSLColor();
            hslColor.Hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.Luminosity = color.GetBrightness();
            hslColor.Saturation = color.GetSaturation();
            return hslColor;
        }

        public void SetRGB(int red, int green, int blue)
        {
            HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
            Hue = hslColor.Hue;
            Saturation = hslColor.Saturation;
            Luminosity = hslColor.Luminosity;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            Color color = (Color)this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor.Luminosity != 0)
            {
                if (hslColor.Saturation == 0)
                    r = g = b = hslColor.Luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.Luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor.Hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.Hue);
                    b = GetColorComponent(temp1, temp2, hslColor.Hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }
    }
}
