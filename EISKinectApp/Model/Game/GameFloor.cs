using System;
using System.Windows;
using System.Windows.Media;
using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.Model.Game {
    public class GameFloor {
        private readonly KinectManager _kinect;
        public static readonly int ColorCircleRadius = 100;
        public static readonly int Padding = 10;
        public static readonly SolidColorBrush[] CircleColors = { Brushes.Red, Brushes.Blue, Brushes.Yellow };
        private readonly Point[] _circleCenters;
        private readonly SolidColorBrush _lastColor;
        public event Action<int, int, SolidColorBrush> ColorStepUpdated;

        public GameFloor() {
            _kinect = KinectManager.Instance;
            _kinect.SkeletonUpdated += OnSkeletonUpdated;
            _lastColor = null;
            var w = KinectManager.FloorDimensions[0];
            var h = KinectManager.FloorDimensions[1];
            var paddedRadius = ColorCircleRadius + Padding;
            _circleCenters = new[] {
                new Point(w / 2, paddedRadius),
                new Point(paddedRadius, h - paddedRadius),
                new Point(w - paddedRadius, h - paddedRadius),
            };
        }

        private void OnSkeletonUpdated(KinectSkeleton skeleton) {
            var leftFoot = skeleton.GetFloorInPixels(JointType.AnkleLeft);
            var rightFoot = skeleton.GetFloorInPixels(JointType.AnkleRight);
            var leftCircle = -1;
            var rightCircle = -1;

            for (var i = 0; i < 3; i++) {
                if (IsInCircle(leftFoot, _circleCenters[i]))
                    leftCircle = i;
                if (IsInCircle(rightFoot, _circleCenters[i]))
                    rightCircle = i;
            }

            var color = MixColors(leftCircle >= 0 ? CircleColors[leftCircle] : null, rightCircle >= 0 ? CircleColors[rightCircle] : null);
            if (color != null && (_lastColor == null || !color.Equals(_lastColor)))
                ColorStepUpdated?.Invoke(leftCircle, rightCircle, color);
        }

        private static bool IsInCircle(Point floorPoint, Point center) {
            var xDist = (int)center.X - (int)floorPoint.X;
            var yDist = (int)center.Y - (int)floorPoint.Y;
            return xDist << 1 + yDist << 1 < ColorCircleRadius << 1;
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