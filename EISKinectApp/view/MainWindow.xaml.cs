using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;
using EISKinectApp.model;
using EISKinectApp.service;

namespace EISKinectApp.view
{
    public partial class MainWindow : Window
    {
        private KinectService _kinectService;
        private GestureDetector _gestureDetector;
        private CalibrationService _calibrationService;
        private CalibrationData _calibrationData;
        private TrackedSkeleton _currentSkeleton;

        private WriteableBitmap _depthBitmap;
        private byte[] _colorPixels;

        // Skeleton drawing
        private Ellipse[] _jointDots;
        private Ellipse _hipDot;

        // Calibration markers
        private Ellipse[] _cornerMarkers;
        private TextBlock[] _cornerLabels;
        private int _currentCorner = 0;

        // Tracking view
        private DispatcherTimer _trackingTimer;

        public MainWindow()
        {
            InitializeComponent();

            _kinectService = new KinectService();
            _gestureDetector = new GestureDetector();
            _calibrationData = new CalibrationData();
            _calibrationService = new CalibrationService(_kinectService.Sensor, _calibrationData);

            // Depth visualization
            _depthBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgra32, null);
            DepthImage.Source = _depthBitmap;
            _colorPixels = new byte[640 * 480 * 4];

            InitSkeletonShapes();
            InitCalibrationShapes();

            _kinectService.DepthFrameReady += OnDepthFrame;
            _kinectService.SkeletonUpdated += OnSkeletonUpdated;
            _kinectService.Start();

            StatusText.Text = "Stand in corner 1 and raise your hands to capture.";
        }

        // === DEPTH FRAME ===
        private void OnDepthFrame(DepthImagePixel[] depthPixels)
        {
            if (depthPixels == null || depthPixels.Length == 0)
                return;

            for (int i = 0; i < depthPixels.Length; i++)
            {
                int depth = depthPixels[i].Depth;

                double normalized = Math.Min(1.0, Math.Max(0, (depth - 500) / 3500.0));
                byte red = (byte)(255 * normalized);
                byte green = 0;
                byte blue = (byte)(255 * (1 - normalized));

                int idx = i * 4;
                _colorPixels[idx + 0] = blue;
                _colorPixels[idx + 1] = green;
                _colorPixels[idx + 2] = red;
                _colorPixels[idx + 3] = 255;
            }

            _depthBitmap.WritePixels(new Int32Rect(0, 0, 640, 480), _colorPixels, 640 * 4, 0);
        }

        // === SKELETON UPDATE ===
        private void OnSkeletonUpdated(TrackedSkeleton tracked)
        {
            _currentSkeleton = tracked;

            if (tracked.IsTracked)
                UpdateSkeletonOverlay(tracked.Skeleton);

            if (_gestureDetector.CheckCaptureGesture(tracked.Skeleton))
                CaptureCorner();
        }

        // === DRAW SKELETON DOTS ===
        private void InitSkeletonShapes()
        {
            _jointDots = new Ellipse[20];
            for (int i = 0; i < _jointDots.Length; i++)
            {
                _jointDots[i] = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.LimeGreen,
                    Visibility = Visibility.Hidden
                };
                KinectCanvas.Children.Add(_jointDots[i]);
            }

            _hipDot = new Ellipse { Width = 15, Height = 15, Fill = Brushes.Red, Visibility = Visibility.Hidden };
            KinectCanvas.Children.Add(_hipDot);
        }

        private void UpdateSkeletonOverlay(Skeleton skeleton)
        {
            var mapper = _kinectService.Sensor.CoordinateMapper;

            int idx = 0;
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState == JointTrackingState.NotTracked)
                {
                    _jointDots[idx].Visibility = Visibility.Hidden;
                }
                else
                {
                    DepthImagePoint pt = mapper.MapSkeletonPointToDepthPoint(joint.Position,
                        DepthImageFormat.Resolution640x480Fps30);

                    Canvas.SetLeft(_jointDots[idx], pt.X - 4);
                    Canvas.SetTop(_jointDots[idx], pt.Y - 4);
                    _jointDots[idx].Visibility = Visibility.Visible;
                }
                idx++;
            }

            DepthImagePoint hipPt = mapper.MapSkeletonPointToDepthPoint(
                skeleton.Joints[JointType.HipCenter].Position,
                DepthImageFormat.Resolution640x480Fps30);

            Canvas.SetLeft(_hipDot, hipPt.X - 7.5);
            Canvas.SetTop(_hipDot, hipPt.Y - 7.5);
            _hipDot.Visibility = Visibility.Visible;

            UpdateCalibrationMarkers();
        }

        // === CALIBRATION HINTS ===
        private void InitCalibrationShapes()
        {
            Point[] screenCorners =
            {
                new Point(50, 50),
                new Point(590, 50),
                new Point(590, 430),
                new Point(50, 430)
            };

            _cornerMarkers = new Ellipse[4];
            _cornerLabels = new TextBlock[4];

            for (int i = 0; i < 4; i++)
            {
                _cornerMarkers[i] = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    StrokeThickness = 3,
                    Stroke = Brushes.Gray,
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(_cornerMarkers[i], screenCorners[i].X - 10);
                Canvas.SetTop(_cornerMarkers[i], screenCorners[i].Y - 10);
                KinectCanvas.Children.Add(_cornerMarkers[i]);

                _cornerLabels[i] = new TextBlock
                {
                    Text = $"Corner {i + 1}",
                    Foreground = Brushes.White
                };
                Canvas.SetLeft(_cornerLabels[i], screenCorners[i].X - 15);
                Canvas.SetTop(_cornerLabels[i], screenCorners[i].Y - 30);
                KinectCanvas.Children.Add(_cornerLabels[i]);
            }
        }

        private void UpdateCalibrationMarkers()
        {
            for (int i = 0; i < _cornerMarkers.Length; i++)
            {
                _cornerMarkers[i].Stroke = i == _currentCorner ? Brushes.Yellow : Brushes.Gray;
                _cornerMarkers[i].Fill =
                    i < _calibrationData.SkeletonPoints.Count ? Brushes.Green : Brushes.Transparent;
                _cornerLabels[i].FontWeight = i == _currentCorner ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        // === CAPTURE CORNER ===
        private void CaptureCorner()
        {
            if (_currentSkeleton == null || !_currentSkeleton.IsTracked)
                return;

            SkeletonPoint hip = _currentSkeleton.GetJointPosition(JointType.HipCenter);
            Point[] corners = { new Point(0, 0), new Point(640, 0), new Point(640, 480), new Point(0, 480) };

            _calibrationService.AddCorner(hip, corners[_currentCorner]);
            _currentCorner++;

            if (_currentCorner < 4)
            {
                StatusText.Text = $"Captured corner {_currentCorner}. Now stand in corner {_currentCorner + 1}.";
            }
            else
            {
                StatusText.Text = "All corners captured — calibrating...";
                _calibrationService.Calibrate();
                StatusText.Text = "Calibration complete! Showing 2D player view...";
                StartTrackingView();
            }

            UpdateCalibrationMarkers();
        }

        // === TRACKING VIEW ===
        private void StartTrackingView()
        {
            CalibrationGrid.Visibility = Visibility.Collapsed;
            TrackingCanvas.Visibility = Visibility.Visible;

            _trackingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33)
            };
            _trackingTimer.Tick += TrackingTimer_Tick;
            _trackingTimer.Start();
        }

        private void TrackingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentSkeleton == null) return;

            Point p2D = _calibrationService.MapToProjection(
                _currentSkeleton.GetJointPosition(JointType.HipCenter));

            if (TrackingCanvas.Children.Count == 0)
            {
                Ellipse playerDot = new Ellipse { Width = 20, Height = 20, Fill = Brushes.Red };
                TrackingCanvas.Children.Add(playerDot);
            }

            Ellipse dot = TrackingCanvas.Children[0] as Ellipse;
            Canvas.SetLeft(dot, p2D.X - 10);
            Canvas.SetTop(dot, p2D.Y - 10);
        }

        // === BUTTON ===
        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            CaptureCorner();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _kinectService.Stop();
        }
    }
}
