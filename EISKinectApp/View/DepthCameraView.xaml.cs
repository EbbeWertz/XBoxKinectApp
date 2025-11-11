using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public void UpdateDepth(int[] depthPixels, int maxValue)
        {
            if (depthPixels == null || depthPixels.Length == 0) return;

            for (int i = 0; i < depthPixels.Length; i++)
            {
                var normalizedDepth = (double)depthPixels[i] / maxValue;
                var red = (byte)(255 * normalizedDepth);
                const byte green = 0;
                var blue = (byte)(255 * (1 - normalizedDepth));

                var idx = i * 4;
                _colorPixels[idx + 0] = blue;
                _colorPixels[idx + 1] = green;
                _colorPixels[idx + 2] = red;
                _colorPixels[idx + 3] = 255;
            }

            _depthBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, 640, 480), _colorPixels, 640 * 4, 0);
        }
    }
}