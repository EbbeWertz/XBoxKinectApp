using System;
using System.Windows;
using System.Windows.Media;
using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.Model.Game {
    public class GameFloor {
        private readonly KinectManager _kinect;
        public static readonly int ColorCircleRadius = 120;
        public static readonly int Padding = 10;
        public static readonly SolidColorBrush[] CircleColors = { Brushes.Red, Brushes.Blue, Brushes.Yellow };

        public Point[] CircleCenters { get; }

        private SolidColorBrush _lastColor;
        public event Action<int, int, SolidColorBrush> ColorStepUpdated;
        public event Action<Point, Point> FeetUpdated;

        public GameFloor() {
            _kinect = KinectManager.Instance;
            _kinect.SkeletonUpdated += OnSkeletonUpdated;
            _lastColor = null;
            var w = KinectManager.FloorDimensions[0];
            var h = KinectManager.FloorDimensions[1];
            var paddedRadius = ColorCircleRadius + Padding;
            CircleCenters = new[] {
                new Point(w / 2, paddedRadius),
                new Point(paddedRadius, h - paddedRadius),
                new Point(w - paddedRadius, h - paddedRadius),
            };
        }
        
        private void OnSkeletonUpdated(KinectSkeleton skeleton) {
            var leftFoot = skeleton.GetFloorInPixels(JointType.FootLeft);
            var rightFoot = skeleton.GetFloorInPixels(JointType.FootRight);
            var leftCircle = -1;
            var rightCircle = -1;

            for (var i = 0; i < 3; i++) {
                if (IsInCircle(leftFoot, CircleCenters[i]))
                    leftCircle = i;
                if (IsInCircle(rightFoot, CircleCenters[i]))
                    rightCircle = i;
            }
            
            FeetUpdated?.Invoke(leftFoot, rightFoot);

            var color = MixColors(leftCircle >= 0 ? CircleColors[leftCircle] : null, rightCircle >= 0 ? CircleColors[rightCircle] : null);
            if ((color == null || (_lastColor != null && color.Equals(_lastColor))) &&
                (color != null || _lastColor == null)) return;
            ColorStepUpdated?.Invoke(leftCircle, rightCircle, color);
            _lastColor = color;
        }

        private static bool IsInCircle(Point floorPoint, Point center) {
            var xDist = (int)center.X - (int)floorPoint.X;
            var yDist = (int)center.Y - (int)floorPoint.Y;
            return xDist*xDist + yDist*yDist < ColorCircleRadius*ColorCircleRadius;
        }

        private static bool PairEquals(SolidColorBrush color1, SolidColorBrush color2, SolidColorBrush equalColor1,
            SolidColorBrush equalColor2) {
            return (color1.Equals(equalColor1) && color2.Equals(equalColor2)) ||
                   (color1.Equals(equalColor2) && color2.Equals(equalColor1));
        }

        private SolidColorBrush MixColors(SolidColorBrush color1, SolidColorBrush color2) {
            if (color1 == null) return color2;
            if (color2 == null) return color1;
            if (PairEquals(color1, color2, Brushes.Blue, Brushes.Red))
                return Brushes.Purple;
            if (PairEquals(color1, color2, Brushes.Blue, Brushes.Yellow))
                return Brushes.Green;
            if (PairEquals(color1, color2, Brushes.Yellow, Brushes.Red))
                return Brushes.Orange;
            return null;
        }
    }
}