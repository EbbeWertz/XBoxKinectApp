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

        public KinectSensor Sensor { get; }

        private KinectCalibrator _calibrator;

        // events:
        public event Action<KinectSkeleton> SkeletonReady;
        public event Action<DepthImagePixel[]> DepthFrameReady;

        // buffer voor depth camera
        private readonly DepthImagePixel[] _depthBuffer;

        private KinectManager() {
            Sensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected)
                      ?? throw new InvalidOperationException("No Kinect connected!");

            Sensor.SkeletonStream.Enable();
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _depthBuffer = new DepthImagePixel[Sensor.DepthStream.FramePixelDataLength];

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
                SkeletonReady?.Invoke(wrapped);
            }
        }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;
                frame.CopyDepthImagePixelDataTo(_depthBuffer);
                DepthFrameReady?.Invoke(_depthBuffer);
            }
        }
    }
}