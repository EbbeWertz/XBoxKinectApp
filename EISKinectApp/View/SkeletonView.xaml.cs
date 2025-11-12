using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.view {
    public partial class SkeletonView {
        private readonly Ellipse[] _jointDots;
        private readonly Line[] _boneLinesOuter;
        private readonly Line[] _boneLinesInner;

        public bool Thick { get; set; } = false;
        public SolidColorBrush Color { get; set; } = Brushes.LimeGreen;

        public SkeletonView() {
            InitializeComponent();

            // All bones from KinectSkeleton definition
            var allBones = KinectSkeleton.AllBones;
            // Double the number of line objects (outer + inner)
            _boneLinesOuter = new Line[allBones.Length];
            _boneLinesInner = new Line[allBones.Length];

            for (int i = 0; i < allBones.Length; i++) {
                // Outer thick outline
                _boneLinesOuter[i] = new Line {
                    Stroke = Brushes.White, // Outline color
                    StrokeThickness = 60, // Very thick
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Visibility = Visibility.Hidden
                };
                SkeletonCanvas.Children.Add(_boneLinesOuter[i]);
            }

            for (int i = 0; i < allBones.Length; i++) {
                // Inner thinner bright line
                _boneLinesInner[i] = new Line {
                    Stroke = Color,
                    StrokeThickness = Thick ? 50 : 5, // Slightly smaller
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Visibility = Visibility.Hidden
                };
                SkeletonCanvas.Children.Add(_boneLinesInner[i]);
            }

            // All joints from Kinect enum
            var allJoints = (JointType[])System.Enum.GetValues(typeof(JointType));
            _jointDots = new Ellipse[allJoints.Length];
            for (int i = 0; i < _jointDots.Length; i++) {
                _jointDots[i] = new Ellipse {
                    Width = 10,
                    Height = 10,
                    Fill = allJoints[i] == JointType.HipCenter ? Brushes.Red : Color,
                    Visibility = Visibility.Hidden
                };
                SkeletonCanvas.Children.Add(_jointDots[i]);
            }
        }

        public void UpdateSkeleton(KinectSkeleton skeleton) {
            if (skeleton == null)
                return;

            // Get all bones with 2D pixel positions (depth view projection)
            var boneDict = skeleton.GetFullSkeletonBonesFrontViewInPixels();

            // --- Draw bones ---
            int i = 0;
            foreach (var pair in boneDict) {
                var (ptA, ptB) = pair.Value;

                if (Thick) {
                    // Outer layer
                    _boneLinesOuter[i].X1 = ptA.X;
                    _boneLinesOuter[i].Y1 = ptA.Y;
                    _boneLinesOuter[i].X2 = ptB.X;
                    _boneLinesOuter[i].Y2 = ptB.Y;
                    _boneLinesOuter[i].Visibility = Visibility.Visible;
                }

                // Inner layer
                _boneLinesInner[i].X1 = ptA.X;
                _boneLinesInner[i].Y1 = ptA.Y;
                _boneLinesInner[i].X2 = ptB.X;
                _boneLinesInner[i].Y2 = ptB.Y;
                _boneLinesInner[i].Visibility = Visibility.Visible;
                _boneLinesInner[i].StrokeThickness =  Thick ? 50 : 5;
                _boneLinesInner[i].Stroke = Color;

                i++;
            }


            // Hide any remaining lines (if fewer than expected)
            for (int j = i; j < _boneLinesInner.Length; j++)
                _boneLinesInner[j].Visibility = Visibility.Hidden;

            for (int j = i; j < _boneLinesOuter.Length; j++)
                _boneLinesOuter[j].Visibility = Visibility.Hidden;

            // --- Draw joints ---
            if (Thick) return;
            var jointDict = skeleton.GetFullSkeletonJointsrontViewInPixels();
            foreach (var pair in jointDict) {
                var joint = pair.Key;
                var pt = pair.Value;

                _jointDots[(int)joint].Visibility = Visibility.Visible;
                Canvas.SetLeft(_jointDots[(int)joint], pt.X - 5);
                Canvas.SetTop(_jointDots[(int)joint], pt.Y - 5);
            }
        }
    }
}