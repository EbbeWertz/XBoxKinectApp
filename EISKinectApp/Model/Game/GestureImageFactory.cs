using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Windows.Media.PixelFormats;

namespace EISKinectApp.Model.Game {
    public static class GestureImageFactory {
        private static readonly Dictionary<string, BitmapSource> _cache = new Dictionary<string, BitmapSource>();

        private static readonly Dictionary<SolidColorBrush, (Color, string)> _colors =
            new Dictionary<SolidColorBrush, (Color, string)> {
                { Brushes.Red, (Colors.Red, "red") },
                { Brushes.Blue, (Colors.Blue, "blue") },
                { Brushes.Yellow, (Colors.Yellow, "yellow") },
                { Brushes.Purple, (Colors.Purple, "purple") },
                { Brushes.Green, (Colors.Green, "green") },
                { Brushes.Orange, (Colors.Orange, "orange") }
            };

        private static readonly string[] _sides = { "left", "right" };
        private static readonly string[] _moves = { "armup", "armside", "armdown" };

        /// <summary>
        /// Prepares all colorized gesture images and caches them.
        /// </summary>
        public static void Initialize() {
            foreach (var side in _sides) {
                foreach (var move in _moves) {
                    string baseKey = side + move;
                    string basePath = "pack://application:,,,/resources/gestures/" + side + move + ".png";

                    var baseImg = new BitmapImage(new Uri(basePath, UriKind.Absolute));

                    foreach (var color in _colors) {
                        string key = baseKey + color.Value.Item2;
                        _cache[key] = Colorize(baseImg, color.Value.Item1);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a pre-colored gesture image by side, move, and color name.
        /// </summary>
        public static BitmapSource Get(string side, string move, SolidColorBrush color) {
            string key = side + move + _colors[color].Item2;
            BitmapSource img;
            if (_cache.TryGetValue(key, out img))
                return img;

            throw new KeyNotFoundException("Gesture image not found: " + key);
        }

        /// <summary>
        /// Creates a colorized copy of a black+transparent PNG using a tint color.
        /// </summary>
        private static BitmapSource Colorize(BitmapSource source, Color color) {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4) {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                // Skip transparent pixels
                if (a == 0)
                    continue;

                // Compute brightness (black = low)
                int brightness = (r + g + b) / 3;

                // If dark (part of shape), tint with given color
                if (brightness < 128) {
                    pixels[i] = color.B;
                    pixels[i + 1] = color.G;
                    pixels[i + 2] = color.R;
                    pixels[i + 3] = a;
                }
            }

            return BitmapSource.Create(
                width, height,
                source.DpiX, source.DpiY,
                PixelFormat.Bgra32,
                null, pixels, stride
            );
        }
    }
}