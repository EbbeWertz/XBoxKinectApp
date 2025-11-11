using System;
using System.Linq;
using Microsoft.Kinect;

namespace EISKinectApp.Model {
    public sealed class KinectManager {
        // singleton:
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<KinectManager> _instance = new Lazy<KinectManager>(() => new KinectManager());
        public static KinectManager Instance => _instance.Value;

        // resources:
        private readonly KinectSensor _sensor;
        private KinectCalibrator _calibrator;

        // events:
        public event Action<KinectSkeleton> SkeletonReady;
        public event Action<byte[]> DepthFrameReady;

        // buffer voor depth camera
        private readonly byte[] _depthPixels;
        private readonly DepthImagePixel[] _depthBuffer;

        private KinectManager() {
            _sensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected)
                      ?? throw new InvalidOperationException("No Kinect connected!");

            _sensor.SkeletonStream.Enable();
            _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _depthBuffer = new DepthImagePixel[_sensor.DepthStream.FramePixelDataLength];
            _depthPixels = new byte[_sensor.DepthStream.FramePixelDataLength * 4];

            _sensor.SkeletonFrameReady += OnSkeletonFrameReady;
            _sensor.DepthFrameReady += OnDepthFrameReady;
        }

        public void Start() {
            if (!_sensor.IsRunning)
                _sensor.Start();
        }

        public void Stop() {
            if (_sensor.IsRunning)
                _sensor.Stop();
        }

        public void SetCalibration(KinectCalibrator calibrator) => _calibrator = calibrator;

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                var tracked = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                if (tracked == null)
                    return;

                var wrapped = new KinectSkeleton(tracked, _calibrator);
                if (SkeletonReady != null)
                    SkeletonReady(wrapped);
            }
        }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                frame.CopyDepthImagePixelDataTo(_depthBuffer);

                // Convert depth pixels to grayscale byte array (for visualization)
                int colorIndex = 0;
                foreach (var depth in _depthBuffer)
                {
                    byte intensity = (byte)(255 - (depth.Depth / 8));
                    _depthPixels[colorIndex++] = intensity;
                    _depthPixels[colorIndex++] = intensity;
                    _depthPixels[colorIndex++] = intensity;
                    _depthPixels[colorIndex++] = 255;
                }

                if (DepthFrameReady != null)
                    DepthFrameReady(_depthPixels);
            }
        }
    }
}