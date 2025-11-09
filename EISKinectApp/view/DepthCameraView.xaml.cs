using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace EISKinectApp.view
{
    public partial class DepthCameraView
    {
        private readonly WriteableBitmap _depthBitmap;
        private readonly byte[] _colorPixels;

        public DepthCameraView()
        {
            InitializeComponent();

            _depthBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgra32, null);
            DepthImage.Source = _depthBitmap;
            _colorPixels = new byte[640 * 480 * 4];
        }

        public void UpdateDepth(DepthImagePixel[] depthPixels)
        {
            if (depthPixels == null || depthPixels.Length == 0) return;

            for (int i = 0; i < depthPixels.Length; i++)
            {
                int depth = depthPixels[i].Depth;
                double normalized = System.Math.Min(1.0, System.Math.Max(0, (depth - 500) / 3500.0));
                byte red = (byte)(255 * normalized);
                byte green = 0;
                byte blue = (byte)(255 * (1 - normalized));

                int idx = i * 4;
                _colorPixels[idx + 0] = blue;
                _colorPixels[idx + 1] = green;
                _colorPixels[idx + 2] = red;
                _colorPixels[idx + 3] = 255;
            }

            _depthBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, 640, 480), _colorPixels, 640 * 4, 0);
        }
    }
}