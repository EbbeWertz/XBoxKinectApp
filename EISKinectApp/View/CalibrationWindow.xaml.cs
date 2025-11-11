using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using EISKinectApp.Model;

namespace EISKinectApp.view
{
    public partial class CalibrationWindow
    {
        private readonly CallibrationWindowFloor _floorWindow;
        private readonly KinectManager _kinect;
        private readonly KinectCalibrator _kinectCalibrator;
        private bool _handsWereUp;

        private KinectSkeleton _currentSkeleton;

        private readonly Ellipse[] _cornerEllipses;

        public CalibrationWindow()
        {
            InitializeComponent();

            _floorWindow = new CallibrationWindowFloor();
            _floorWindow.Show();

            _kinect = KinectManager.Instance;
            _kinectCalibrator = new KinectCalibrator(_kinect.Sensor);

            _cornerEllipses = new[] { Corner1, Corner2, Corner3, Corner4 };

            _kinect.DepthFrameReady += OnDepthFrame;
            _kinect.SkeletonReady += OnSkeletonUpdated;
            _kinect.Start();

            StatusText.Text = "Stand in corner 1 and raise your hands to capture.";
            UpdateCornerMarkers();
        }


        private void OnDepthFrame(DepthImagePixel[] depthPixels)
        {
            DepthView.UpdateDepth(depthPixels);
        }

        private void OnSkeletonUpdated(KinectSkeleton skeleton)
        {
            _currentSkeleton = skeleton;

            if (skeleton.IsTracked)
                SkeletonOverlay.UpdateSkeleton(skeleton);

            if (KinectGestureDetector.HandsRaisedAboveHead(skeleton)) {
                _handsWereUp = true;
            } else if (KinectGestureDetector.HandsLoweredBelowHead(skeleton) && _handsWereUp) {
                CaptureCorner();
            }
        }

        private void CaptureCorner()
        {
            if (_currentSkeleton == null || !_currentSkeleton.IsTracked) return;
            
            var hip = _currentSkeleton.GetRaw(JointType.HipCenter);
            _kinectCalibrator.AddCorner(hip);

            if (_kinectCalibrator.CornersPending())
            {
                StatusText.Text = $"Captured corner {_kinectCalibrator.CurrentCorner}. Now stand in corner {_kinectCalibrator.CurrentCorner + 1}.";
            }
            else
            {
                _kinectCalibrator.Calibrate();
                // var trackingWindow = new TrackingTestWindow(_currentSkeleton, _calibrationService);
                // trackingWindow.Show();
                _floorWindow.Hide();
                Hide();
            }

            UpdateCornerMarkers();
        }

        private void UpdateCornerMarkers()
        {
            for (int i = 0; i < _cornerEllipses.Length; i++)
            {
                _cornerEllipses[i].Stroke = i == _kinectCalibrator.CurrentCorner ? Brushes.Yellow : Brushes.Gray;
                _cornerEllipses[i].Fill = i < _kinectCalibrator.CurrentCorner ? Brushes.Green : Brushes.Transparent;
            }

            _floorWindow.HighlightCorner(_kinectCalibrator.CurrentCorner);
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            CaptureCorner();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _kinect.Stop();
        }
    }
}
