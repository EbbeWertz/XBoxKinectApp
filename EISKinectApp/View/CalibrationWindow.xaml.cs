using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using EISKinectApp.model.KinectWrapper;
using EISKinectApp.Model.KinectWrapper;

namespace EISKinectApp.view {
    public partial class CalibrationWindow {
        private readonly CallibrationWindowFloor _floorWindow;
        private readonly KinectManager _kinect;
        private readonly Ellipse[] _cornerEllipses;
        private bool _handsWereDown = true;

        public CalibrationWindow() {
            InitializeComponent();

            _floorWindow = new CallibrationWindowFloor();
            _floorWindow.Show();

            _kinect = KinectManager.Instance;
            _kinect.Start();
            _kinect.SkeletonUpdated += OnSkeletonUpdated;
            _kinect.DepthFrameUpdated += OnDepthFrameUpdated;
            
            _cornerEllipses = new[] { Corner1, Corner2, Corner3, Corner4 };

            StatusText.Text = "Stand in corner 1 and raise your hands to capture.";
            UpdateCornerMarkers();
        }


        private void OnDepthFrameUpdated(int[] depthPixels, int maxValue) {
            DepthView.UpdateDepth(depthPixels, maxValue);
        }

        private void OnSkeletonUpdated(KinectSkeleton skeleton) {
            SkeletonOverlay.UpdateSkeleton(skeleton);

            switch (_handsWereDown) {
                case true when KinectGestureDetector.HandsRaisedAboveHead(skeleton):
                    CaptureCorner();
                    _handsWereDown = false;
                    SkeletonOverlay.Color = Brushes.Yellow;
                    break;
                case false when KinectGestureDetector.HandsLoweredBelowHead(skeleton):
                    _handsWereDown = true;
                    SkeletonOverlay.Color = Brushes.LimeGreen;
                    break;
            }
        }

        private void CaptureCorner() {
            var success = _kinect.RegisterCalibrationCorner();
            if (!success) return;
            if (_kinect.IsCalibrated) {
                var gameWindow = new GameWindow();
                gameWindow.Show();
                _floorWindow.Hide();
                Hide();
            } else {
                StatusText.Text = $"Captured corner {_kinect.NextCornerToCalibrate}. Now stand in corner {_kinect.NextCornerToCalibrate + 1}.";
            }
            UpdateCornerMarkers();
        }

        private void UpdateCornerMarkers() {
            for (var i = 0; i < _cornerEllipses.Length; i++)
            {
                _cornerEllipses[i].Stroke = i == _kinect.NextCornerToCalibrate ? Brushes.Yellow : Brushes.Gray;
                _cornerEllipses[i].Fill = i < _kinect.NextCornerToCalibrate  ? Brushes.Green : Brushes.Transparent;
            }
            
            _floorWindow.HighlightCorner(_kinect.NextCornerToCalibrate);
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e) {
            CaptureCorner();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            _kinect.Stop();
            _kinect.DepthFrameUpdated -= OnDepthFrameUpdated;
            _kinect.SkeletonUpdated -= OnSkeletonUpdated;
            _floorWindow.Close();
        }
    }
}