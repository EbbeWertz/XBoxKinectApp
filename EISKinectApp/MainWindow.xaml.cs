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

                byte[] pixels = new byte[_depthPixels.Length];

                // Convert depth to grayscale (simple linear mapping)
                for (int i = 0; i < _depthPixels.Length; i++)
                {
                    int depth = _depthPixels[i].Depth;
                    byte intensity = (byte)(255 - (depth / 8) % 255);
                    pixels[i] = intensity;
                }

                _depthBitmap.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height),
                    pixels,
                    frame.Width,
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

                // Draw hip center larger (red)
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

            // Draw calibration points if captured
            for (int i = 0; i < _calibration.m_skeletonCalibPoints.Count; i++)
            {
                SkeletonPoint sp = _calibration.m_skeletonCalibPoints[i];
                DepthImagePoint pt = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                    sp, DepthImageFormat.Resolution640x480Fps30);

                Ellipse cornerMarker = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Stroke = Brushes.Yellow,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(cornerMarker, pt.X - 6);
                Canvas.SetTop(cornerMarker, pt.Y - 6);
                KinectCanvas.Children.Add(cornerMarker);
            }
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
            StatusText.Text = "Calibration complete!";
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_sensor != null && _sensor.IsRunning)
                _sensor.Stop();
        }
    }
}
