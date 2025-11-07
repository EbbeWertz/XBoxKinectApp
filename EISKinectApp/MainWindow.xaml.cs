using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace EISKinectApp
{
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor;
        private PartialCalibrationClass _calibration;
        private int _currentCorner = 0;

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

            _sensor.SkeletonStream.Enable();
            _sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
            _sensor.Start();

            _calibration = new PartialCalibrationClass(_sensor);
        }

        private Skeleton _lastSkeleton;

        private void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null) return;

                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                _lastSkeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
            }
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastSkeleton == null)
            {
                StatusText.Text = "No skeleton detected — please step in front of the Kinect.";
                return;
            }

            // Get skeleton point and a screen corner (for now, simple 640x480 rectangle)
            SkeletonPoint sp = _lastSkeleton.Joints[JointType.HipCenter].Position;

            _calibration.m_skeletonCalibPoints.Add(sp);

            // Simple example screen positions (adjust as needed)
            Point[] screenCorners =
            {
                new Point(0, 0),         // top-left
                new Point(640, 0),       // top-right
                new Point(640, 480),     // bottom-right
                new Point(0, 480)        // bottom-left
            };

            _calibration.m_calibPoints.Add(screenCorners[_currentCorner]);

            _currentCorner++;
            if (_currentCorner < 4)
            {
                StatusText.Text = $"Captured corner {_currentCorner}. Now stand in corner {_currentCorner + 1} and press Capture.";
            }
            else
            {
                StatusText.Text = "All corners captured! Click Calibrate.";
                CaptureButton.IsEnabled = false;
                CalibrateButton.IsEnabled = true;
            }
        }

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
