using System;
using System.Linq;
using Microsoft.Kinect;
using EISKinectApp.model;

namespace EISKinectApp.service
{
    public class KinectService
    {
        private readonly DepthImagePixel[] _depthPixels;

        public KinectSensor Sensor { get; }

        public event Action<DepthImagePixel[]> DepthFrameReady;
        public event Action<TrackedSkeleton> SkeletonUpdated;

        public KinectService()
        {
            Sensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
            if (Sensor == null)
                throw new InvalidOperationException("No Kinect detected!");

            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Sensor.SkeletonStream.Enable();

            _depthPixels = new DepthImagePixel[Sensor.DepthStream.FramePixelDataLength];
            Sensor.DepthFrameReady += OnDepthFrameReady;
            Sensor.SkeletonFrameReady += OnSkeletonFrameReady;
        }

        public void Start() => Sensor.Start();
        public void Stop() { if (Sensor.IsRunning) Sensor.Stop(); }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;
                frame.CopyDepthImagePixelDataTo(_depthPixels);
                DepthFrameReady?.Invoke(_depthPixels);
            }
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null) return;
                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var tracked = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                if (tracked != null)
                    SkeletonUpdated?.Invoke(new TrackedSkeleton(tracked));
            }
        }
    }
}
