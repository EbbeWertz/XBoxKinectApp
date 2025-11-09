using System.Collections.Generic;
using System.Windows;
using Microsoft.Kinect;

namespace EISKinectApp.model
{
    public class CalibrationData
    {
        public List<SkeletonPoint> SkeletonPoints { get; } = new List<SkeletonPoint>();
        public List<Point> ScreenPoints { get; } = new List<Point>();

        public bool IsComplete => SkeletonPoints.Count >= 4;
    }
}