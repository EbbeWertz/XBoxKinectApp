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
            _depthBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Gray8, null);
            DepthImage.Source = _depthBitmap;

            _sensor.Start();
            _calibration = new PartialCalibrationClass(_sensor);
        }

        // DEPTH FRAME
        private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;

                frame.CopyDepthImagePixelDataTo(_depthPixels);

                byte[] colorPixels = new byte[_depthPixels.Length * 4]; // BGRA

                for (int i = 0; i < _depthPixels.Length; i++)
                {
                    int depth = _depthPixels[i].Depth;

                    // Clamp and normalize depth (0–4096 → 0–1)
                    double normalized = Math.Min(1.0, Math.Max(0, (depth - 500) / 3500.0));

                    // Map to blue intensity
                    byte blue = (byte)(255 * (1 - normalized)); // Near = bright blue, far = dark
                    byte red = 0;
                    byte green = (byte)(blue / 4); // subtle teal tint

                    int idx = i * 4;
                    colorPixels[idx + 0] = blue; // Blue
                    colorPixels[idx + 1] = green; // Green
                    colorPixels[idx + 2] = red; // Red
                    colorPixels[idx + 3] = 255; // Alpha
                }

                _depthBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgra32, null);
                DepthImage.Source = _depthBitmap;

                _depthBitmap.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height),
                    colorPixels,
                    frame.Width * 4,
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
                DrawDebugOverlays();
            }
        }

        private void DrawDebugOverlays()
        {
            KinectCanvas.Children.Clear();

            // 1️⃣ Draw skeleton if available
            if (_lastSkeleton != null)
            {
                foreach (Joint joint in _lastSkeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.NotTracked)
                    {
                        DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                            joint.Position, DepthImageFormat.Resolution640x480Fps30);

                        Ellipse jointDot = new Ellipse
                        {
                            Width = 8,
                            Height = 8,
                            Fill = Brushes.LimeGreen
                        };

                        Canvas.SetLeft(jointDot, depthPoint.X - 4);
                        Canvas.SetTop(jointDot, depthPoint.Y - 4);
                        KinectCanvas.Children.Add(jointDot);
                    }
                }

                // HipCenter
                Joint hip = _lastSkeleton.Joints[JointType.HipCenter];
                DepthImagePoint hipPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                    hip.Position, DepthImageFormat.Resolution640x480Fps30);

                Ellipse hipMarker = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(hipMarker, hipPoint.X - 7.5);
                Canvas.SetTop(hipMarker, hipPoint.Y - 7.5);
                KinectCanvas.Children.Add(hipMarker);
            }

            // 2️⃣ Draw calibration corners
            Point[] screenCorners =
            {
                new Point(50, 50), // top-left
                new Point(590, 50), // top-right
                new Point(590, 430), // bottom-right
                new Point(50, 430) // bottom-left
            };

            for (int i = 0; i < screenCorners.Length; i++)
            {
                Ellipse cornerMarker = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    StrokeThickness = 3,
                    Stroke = i == _currentCorner ? Brushes.Yellow : Brushes.Gray,
                    Fill = i < _calibration.m_skeletonCalibPoints.Count ? Brushes.Green : Brushes.Transparent
                };
                Canvas.SetLeft(cornerMarker, screenCorners[i].X - 10);
                Canvas.SetTop(cornerMarker, screenCorners[i].Y - 10);
                KinectCanvas.Children.Add(cornerMarker);

                // Optional label
                TextBlock label = new TextBlock
                {
                    Text = $"Corner {i + 1}",
                    Foreground = Brushes.White,
                    FontWeight = i == _currentCorner ? FontWeights.Bold : FontWeights.Normal
                };
                Canvas.SetLeft(label, screenCorners[i].X - 15);
                Canvas.SetTop(label, screenCorners[i].Y - 30);
                KinectCanvas.Children.Add(label);
            }
        }

        private void StartTrackingView()
        {
            // Hide calibration UI, show tracking canvas
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

            // Get 2D projected position
            Point p2D = _calibration.kinectToProjectionPoint(_lastSkeleton.Joints[JointType.HipCenter].Position);

            TrackingCanvas.Children.Clear();

            // Draw a dot for player
            Ellipse playerDot = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(playerDot, p2D.X - 10);
            Canvas.SetTop(playerDot, p2D.Y - 10);
            TrackingCanvas.Children.Add(playerDot);

            // Optional: draw borders
            Rectangle border = new Rectangle
            {
                Width = 640,
                Height = 480,
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            TrackingCanvas.Children.Add(border);
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

            Point[] screenCorners =
            {
                new Point(0, 0),
                new Point(640, 0),
                new Point(640, 480),
                new Point(0, 480)
            };
            _calibration.m_calibPoints.Add(screenCorners[_currentCorner]);

            _currentCorner++;
            if (_currentCorner < 4)
            {
                StatusText.Text =
                    $"Captured corner {_currentCorner}. Now stand in corner {_currentCorner + 1} and press Capture.";
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
            StatusText.Text = "Calibration comp" +
                              "lete! Showing 2D player view...";
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