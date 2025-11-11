using System;
using System.Linq;
using System.Windows;
using Microsoft.Kinect;

namespace EISKinectApp.model.KinectWrapper {
    public class KinectManager {
        // singleton:
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<KinectManager> _instance = new Lazy<KinectManager>(() => new KinectManager());
        public static KinectManager Instance => _instance.Value;

        // calibration
        private static int[] _floorDimensions = { 480, 640 };
        private bool _isCalibrated;
        private KinectSensor Sensor { get; }
        private readonly KinectCalibratedProjector _calibratedProjector;

        // events:
        public event Action<KinectSkeleton> SkeletonUpdated;
        public event Action<int[], int> DepthFrameUpdated;

        // sensor buffers
        private readonly DepthImagePixel[] _rawDepthBuffer;
        private readonly int[] _depthBuffer;
        private Skeleton _currentRawSkeleton;


        private KinectManager() {
            _isCalibrated = false;
            Sensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected)
                     ?? throw new InvalidOperationException("No Kinect connected!");

            Sensor.SkeletonStream.Enable();
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            
            _calibratedProjector = new KinectCalibratedProjector(Sensor);
            
            _depthBuffer = new int[Sensor.DepthStream.FramePixelDataLength];
            _rawDepthBuffer = new DepthImagePixel[Sensor.DepthStream.FramePixelDataLength];

            Sensor.SkeletonFrameReady += OnSkeletonFrameReady;
            Sensor.DepthFrameReady += OnDepthFrameReady;
        }

        public void Start() {
            if (!Sensor.IsRunning)
                Sensor.Start();
        }

        public void Stop() {
            if (Sensor.IsRunning)
                Sensor.Stop();
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            using (var frame = e.OpenSkeletonFrame()) {
                if (frame == null) return;
                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var tracked = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                if (tracked == null) return;
                _currentRawSkeleton = tracked;
                SkeletonUpdated?.Invoke(new KinectSkeleton(tracked, _calibratedProjector, Sensor.CoordinateMapper));
            }
        }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e) {
            using (var frame = e.OpenDepthImageFrame()) {
                if (frame == null)
                    return;

                frame.CopyDepthImagePixelDataTo(_rawDepthBuffer);
                for (var i = 0; i < _rawDepthBuffer.Length; i++) {
                    _depthBuffer[i] = _rawDepthBuffer[i].Depth;
                }

                DepthFrameUpdated?.Invoke(_depthBuffer, 2047);
            }
        }

        public void RegisterCalibrationCorner() {
            Point[] screenCorners = {
                new Point(0,0),
                new Point(_floorDimensions[0], 0),
                new Point(_floorDimensions[0], _floorDimensions[1]),
                new Point(0,  _floorDimensions[1])
            };
             
            _calibratedProjector.RegisterCorner(_currentRawSkeleton.Joints[JointType.HipCenter].Position, screenCorners[_calibratedProjector.CurrentPoint]);

            if (_calibratedProjector.CurrentPoint < 4) return;
            _calibratedProjector.Calibrate();
            _isCalibrated = true;
        }
    }
}