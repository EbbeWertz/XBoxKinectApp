using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EISKinectApp.View
{
    public partial class CallibrationWindowFloor : Window
    {
        private Ellipse[] _corners;

        public CallibrationWindowFloor()
        {
            InitializeComponent();
            _corners = new[] { Corner1, Corner2, Corner3, Corner4 };

            Loaded += (s, e) => UpdateCornerPositions();
            ProjectionBox.SizeChanged += (s, e) => UpdateCornerPositions();
            
            var screen = Screen.AllScreens.Length > 1 ? Screen.AllScreens[1] : Screen.PrimaryScreen;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = screen.WorkingArea.Left;
            Top = screen.WorkingArea.Top;
            WindowState = WindowState.Maximized;
        }

        private void UpdateCornerPositions()
        {
            double w = ProjectionCanvas.Width;
            double h = ProjectionCanvas.Height;

            Canvas.SetLeft(Corner1, 0);
            Canvas.SetTop(Corner1, 0);

            Canvas.SetLeft(Corner2, w - Corner2.Width);
            Canvas.SetTop(Corner2, 0);

            Canvas.SetLeft(Corner3, w - Corner3.Width);
            Canvas.SetTop(Corner3, h - Corner3.Height);

            Canvas.SetLeft(Corner4, 0);
            Canvas.SetTop(Corner4, h - Corner4.Height);
        }

        /// <summary>
        /// Highlights the active corner (0..3)
        /// </summary>
        public void HighlightCorner(int index)
        {
            for (int i = 0; i < _corners.Length; i++)
            {
                _corners[i].Stroke = i == index ? Brushes.Yellow : Brushes.Gray;
                _corners[i].Fill = i == index ? Brushes.Yellow : Brushes.Transparent;
            }
        }
    }
}