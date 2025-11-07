using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace EISKinectApp
{
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor;
        private PartialCalibrationClass _calibration;
        private Skeleton _lastSkeleton;
        private int _currentCorner = 0;
        private WriteableBitmap _depthBitmap;
        private DepthImagePixel[] _depthPixels;
        private byte[] _colorPixels;

        // Skeleton drawing
        private Ellipse[] _jointDots;
        private Ellipse _hipDot;

        // Calibration corners
        private Ellipse[] _cornerMarkers;
        private TextBlock[] _cornerLabels;

        // Tracking view
        private System.Windows.Threading.DispatcherTimer _trackingTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize Kinect
            _sensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
            if (_sensor == null)
            {
                MessageBox.Show("No Kinect detected!");
                return;
            }

            // Enable streams
            _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _sensor.SkeletonStream.Enable();

            // Event handlers
            _sensor.DepthFrameReady += Sensor_DepthFrameReady;
            _sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;

            // Initialize depth image buffer
            _depthPixels = new DepthImagePixel[_sensor.DepthStream.FramePixelDataLength];
            _colorPixels = new byte[_depthPixels.Length * 4]; // BGRA
            _depthBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgra32, null);
            DepthImage.Source = _depthBitmap;

            // Initialize skeleton and calibration shapes
            InitSkeletonShapes();
            InitCalibrationShapes();

            _sensor.Start();
            _calibration = new PartialCalibrationClass(_sensor);
        }

        private void InitSkeletonShapes()
        {
            _jointDots = new Ellipse[20];
            for (int i = 0; i < _jointDots.Length; i++)
            {
                _jointDots[i] = new Ellipse { Width = 8, Height = 8, Fill = Brushes.LimeGreen };
                KinectCanvas.Children.Add(_jointDots[i]);
            }

            _hipDot = new Ellipse { Width = 15, Height = 15, Fill = Brushes.Red };
            KinectCanvas.Children.Add(_hipDot);
        }

        private void InitCalibrationShapes()
        {
            Point[] screenCorners =
            {
                new Point(50,50), new Point(590,50), new Point(590,430), new Point(50,430)
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
                    Text = $"Corner {i+1}",
                    Foreground = Brushes.White
                };
                Canvas.SetLeft(_cornerLabels[i], screenCorners[i].X - 15);
                Canvas.SetTop(_cornerLabels[i], screenCorners[i].Y - 30);
                KinectCanvas.Children.Add(_cornerLabels[i]);
            }
        }

        // DEPTH FRAME
        private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;

                frame.CopyDepthImagePixelDataTo(_depthPixels);

                for (int i = 0; i < _depthPixels.Length; i++)
                {
                    int depth = _depthPixels[i].Depth;
                    double normalized = Math.Min(1.0, Math.Max(0, (depth - 500) / 3500.0));

                    byte blue = (byte)(255 * (1 - normalized));
                    byte green = (byte)(blue / 4);

                    int idx = i * 4;
                    _colorPixels[idx + 0] = blue;
                    _colorPixels[idx + 1] = green;
                    _colorPixels[idx + 2] = 0;
                    _colorPixels[idx + 3] = 255;
                }

                _depthBitmap.WritePixels(
                    new Int32Rect(0, 0, 640, 480),
                    _colorPixels,
                    640 * 4,
                    0
                );
            }
        }

        // SKELETON FRAME
        private void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null) return;

                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                _lastSkeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                UpdateDebugOverlays();
            }
        }

        private void UpdateDebugOverlays()
        {
            if (_lastSkeleton != null)
            {
                int idx = 0;
                foreach (Joint joint in _lastSkeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.NotTracked && idx < _jointDots.Length)
                    {
                        DepthImagePoint pt = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                            joint.Position, DepthImageFormat.Resolution640x480Fps30);
                        Canvas.SetLeft(_jointDots[idx], pt.X - 4);
                        Canvas.SetTop(_jointDots[idx], pt.Y - 4);
                        idx++;
                    }
                }

                DepthImagePoint hipPt = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                    _lastSkeleton.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                Canvas.SetLeft(_hipDot, hipPt.X - 7.5);
                Canvas.SetTop(_hipDot, hipPt.Y - 7.5);
            }

            // Update calibration corner colors
            for (int i = 0; i < _cornerMarkers.Length; i++)
            {
                _cornerMarkers[i].Stroke = i == _currentCorner ? Brushes.Yellow : Brushes.Gray;
                _cornerMarkers[i].Fill = i < _calibration.m_skeletonCalibPoints.Count ? Brushes.Green : Brushes.Transparent;
                _cornerLabels[i].FontWeight = i == _currentCorner ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        private void StartTrackingView()
        {
            CalibrationGrid.Visibility = Visibility.Collapsed;
            TrackingCanvas.Visibility = Visibility.Visible;

            _trackingTimer = new System.Windows.Threading.DispatcherTimer();
            _trackingTimer.Interval = TimeSpan.FromMilliseconds(33); // ~30 FPS
            _trackingTimer.Tick += TrackingTimer_Tick;
            _trackingTimer.Start();
        }

        private void TrackingTimer_Tick(object sender, EventArgs e)
        {
            if (_lastSkeleton == null) return;

            Point p2D = _calibration.kinectToProjectionPoint(_lastSkeleton.Joints[JointType.HipCenter].Position);

            // Move/redraw player dot efficiently
            if (TrackingCanvas.Children.Count == 0)
            {
                Ellipse playerDot = new Ellipse { Width = 20, Height = 20, Fill = Brushes.Red };
                TrackingCanvas.Children.Add(playerDot);
            }

            Ellipse dot = TrackingCanvas.Children[0] as Ellipse;
            Canvas.SetLeft(dot, p2D.X - 10);
            Canvas.SetTop(dot, p2D.Y - 10);
        }

        // CAPTURE
        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastSkeleton == null)
            {
                StatusText.Text = "No skeleton detected — please step in view.";
                return;
            }

            SkeletonPoint sp = _lastSkeleton.Joints[JointType.HipCenter].Position;
            _calibration.m_skeletonCalibPoints.Add(sp);

            Point[] screenCorners = { new Point(0, 0), new Point(640, 0), new Point(640, 480), new Point(0, 480) };
            _calibration.m_calibPoints.Add(screenCorners[_currentCorner]);

            _currentCorner++;
            if (_currentCorner < 4)
            {
                StatusText.Text = $"Captured corner {_currentCorner}. Now stand in corner {_currentCorner + 1} and press Capture.";
            }
            else
            {
                StatusText.Text = "All corners captured — click Calibrate.";
                CaptureButton.IsEnabled = false;
                CalibrateButton.IsEnabled = true;
            }
        }

        // CALIBRATE
        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            _calibration.Calibrate();
            StatusText.Text = "Calibration complete! Showing 2D player view...";
            StartTrackingView();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_sensor != null && _sensor.IsRunning)
                _sensor.Stop();
        }
    }
}
