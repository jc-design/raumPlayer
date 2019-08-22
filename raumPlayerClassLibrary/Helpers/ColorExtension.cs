using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.UI;

namespace raumPlayer.Helpers
{
    public static class ColorExtension
    {
        public static Color StringToColor(string colorstring)
        {
            byte alpha;
            byte pos = 0;
            Color color;
            try
            {
                colorstring = colorstring.Replace("#", "");

                if (colorstring.Length == 8)
                {
                    alpha = System.Convert.ToByte(colorstring.Substring(pos, 2), 16);
                    pos = 2;
                }
                else
                {
                    alpha = System.Convert.ToByte("ff", 16);
                }

                byte red = System.Convert.ToByte(colorstring.Substring(pos, 2), 16);

                pos += 2;
                byte green = System.Convert.ToByte(colorstring.Substring(pos, 2), 16);

                pos += 2;
                byte blue = System.Convert.ToByte(colorstring.Substring(pos, 2), 16);

                color = Color.FromArgb(alpha, red, green, blue);
            }
            catch (Exception)
            {
                color = Colors.Transparent;
            }

            return color;
        }
        public static Color UIntToColor(uint uintCol)
        {
            byte A = (byte)((uintCol & 0xFF000000) >> 24);
            byte R = (byte)((uintCol & 0x00FF0000) >> 16);
            byte G = (byte)((uintCol & 0x0000FF00) >> 8);
            byte B = (byte)((uintCol & 0x000000FF) >> 0);
            return Color.FromArgb(A, R, G, B);
        }
    }
}
