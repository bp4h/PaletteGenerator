using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PaletteGenerator.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GeneratePalette(string color, string paletteType)
        {
            if (!string.IsNullOrEmpty(color))
            {
                List<string> palette = new List<string>();

                if (paletteType == "analogous")
                {
                    palette = GenerateAnalogousPalette(color);
                }

                ViewData["Palette"] = palette;
            }
            else
            {
                ViewData["Palette"] = null;
            }

            ViewData["PaletteType"] = paletteType;

            return View("Index");
        }
        
        private List<string> GenerateAnalogousPalette(string baseColor)
        {
            List<string> palette = new List<string>();

            Color color = ColorTranslator.FromHtml(baseColor);

            ColorToHsl(color, out double baseHue, out double baseSaturation, out double baseLightness);

            // Generate analogous colors
            double hueStep = 30;

            double shiftedHue1 = (baseHue + hueStep) % 360;
            Color shiftedColor1 = HslToRgb(shiftedHue1, baseSaturation, baseLightness);
            string hex1 = ColorToHex(shiftedColor1);

            double shiftedHue2 = (baseHue - hueStep + 360) % 360;
            Color shiftedColor2 = HslToRgb(shiftedHue2, baseSaturation, baseLightness);
            string hex2 = ColorToHex(shiftedColor2);

            palette.Add(baseColor);
            palette.Add(hex1);
            palette.Add(hex2);

            List<Color> analogousColors1 = GetAnalogousColors(hex1);
            palette.AddRange(analogousColors1.Select(ColorToHex));

            List<Color> analogousColors2 = GetAnalogousColors(hex2);
            palette.AddRange(analogousColors2.Select(ColorToHex));            

            return palette;

        }
        private List<Color> GetAnalogousColors(string baseColor)
        {
            Color color = ColorTranslator.FromHtml(baseColor);

            ColorToHsl(color, out double baseHue, out double baseSaturation, out double baseLightness);

            double hueStep = 30;

            double shiftedHue1 = (baseHue + hueStep) % 360;
            Color shiftedColor1 = HslToRgb(shiftedHue1, baseSaturation, baseLightness);

            double shiftedHue2 = (baseHue - hueStep + 360) % 360;
            Color shiftedColor2 = HslToRgb(shiftedHue2, baseSaturation, baseLightness);

            return new List<Color> { shiftedColor1, shiftedColor2 };
        }        
        private void ColorToHsl(Color color, out double hue, out double saturation, out double lightness)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            // Calculate the hue
            if (max == min)
            {
                hue = 0; 
            }
            else if (max == r)
            {
                hue = (((g - b) / (max - min)) % 6 + 6) % 6 * 60;
            }
            else if (max == g)
            {
                hue = (((b - r) / (max - min)) + 2) * 60;
            }
            else // max == b
            {
                hue = (((r - g) / (max - min)) + 4) * 60;
            }

            // Calculate the lightness
            lightness = (max + min) / 2;

            // Calculate the saturation
            if (max == min)
            {
                saturation = 0;
            }
            else if (lightness <= 0.5)
            {
                saturation = (max - min) / (2 * lightness);
            }
            else
            {
                saturation = (max - min) / (2 - (2 * lightness));
            }
        }

        
        private Color HslToRgb(double hue, double saturation, double lightness)
        {
            double c = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
            double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            double m = lightness - (c / 2);

            double r, g, b;

            //if (hue >= 0 && hue < 60)
            if (hue >= 0 && hue <= 60)

            {
                r = c;
                g = x;
                b = 0;
            }
            else if (hue >= 60 && hue < 120)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (hue >= 120 && hue < 180)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (hue >= 180 && hue < 240)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (hue >= 240 && hue < 300)
            {
                r = x;
                g = 0;
                b = c;
            }
            else // hue >= 300 && hue < 360
            {
                r = c;
                g = 0;
                b = x;
            }

            int red = (int)Math.Round((r + m) * 255);
            int green = (int)Math.Round((g + m) * 255);
            int blue = (int)Math.Round((b + m) * 255);

            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);

            return Color.FromArgb(red, green, blue);
        }
        private string ColorToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }        
    }
}