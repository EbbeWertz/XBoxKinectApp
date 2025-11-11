using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using EISKinectApp.model;
using EISKinectApp.service;

namespace EISKinectApp.view
{
    public partial class CalibrationWindow
    {
        private CallibrationWindowFloor _floorWindow;
        private readonly KinectService _kinectService;
        private readonly GestureDetector _gestureDetector;
        private readonly CalibrationService _calibrationService;
        private readonly CalibrationData _calibrationData;
        private TrackedSkeleton _currentSkeleton;

        private int _currentCorner;

        private Ellipse[] _cornerEllipses;

        public CalibrationWindow()
        {
            InitializeComponent();

            _floorWindow = new CallibrationWindowFloor();
            _floorWindow.Show();

            _kinectService = new KinectService();
            _gestureDetector = new GestureDetector();
            _calibrationData = new CalibrationData();
            _calibrationService = new CalibrationService(_kinectService.Sensor, _calibrationData);

            _cornerEllipses = new[] { Corner1, Corner2, Corner3, Corner4 };

            _kinectService.DepthFrameReady += OnDepthFrame;
            _kinectService.SkeletonUpdated += OnSkeletonUpdated;
            _kinectService.Start();

            StatusText.Text = "Stand in corner 1 and raise your hands to capture.";
            UpdateCornerMarkers();
        }


        private void OnDepthFrame(DepthImagePixel[] depthPixels)
        {
            DepthView.UpdateDepth(depthPixels);
        }

        private void OnSkeletonUpdated(TrackedSkeleton tracked)
        {
            _currentSkeleton = tracked;

            if (tracked.IsTracked)
                SkeletonOverlay.UpdateSkeleton(tracked.Skeleton, _kinectService.Sensor.CoordinateMapper);

            if (_gestureDetector.CheckCaptureGesture(tracked.Skeleton))
                CaptureCorner();
        }

        private void CaptureCorner()
        {
            if (_currentSkeleton == null || !_currentSkeleton.IsTracked) return;

            SkeletonPoint hip = _currentSkeleton.GetJointPosition(JointType.HipCenter);
            Point[] corners = { new Point(0,0), new Point(640,0), new Point(640,480), new Point(0,480) };

            _calibrationService.AddCorner(hip, corners[_currentCorner]);
            _currentCorner++;

            if (_currentCorner < 4)
            {
                StatusText.Text = $"Captured corner {_currentCorner}. Now stand in corner {_currentCorner + 1}.";
            }
            else
            {
                _calibrationService.Calibrate();
                var gameWindow = new GameWindow();
                gameWindow.Show();
                _floorWindow.Hide();
                Hide();
            }

            UpdateCornerMarkers();
        }

        private void UpdateCornerMarkers()
        {
            for (int i = 0; i < _cornerEllipses.Length; i++)
            {
                _cornerEllipses[i].Stroke = i == _currentCorner ? Brushes.Yellow : Brushes.Gray;
                _cornerEllipses[i].Fill = i < _calibrationData.SkeletonPoints.Count ? Brushes.Green : Brushes.Transparent;
            }

            _floorWindow.HighlightCorner(_currentCorner);
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            CaptureCorner();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _kinectService.Stop();
            _floorWindow.Close();
        }
    }
}
