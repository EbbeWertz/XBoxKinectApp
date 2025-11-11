using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.view
{
    public partial class SkeletonView
    {
        private readonly Ellipse[] _jointDots;
        private readonly Line[] _boneLines;

        public SkeletonView()
        {
            InitializeComponent();

            // All joints from Kinect enum
            var allJoints = (JointType[])System.Enum.GetValues(typeof(JointType));
            _jointDots = new Ellipse[allJoints.Length];

            for (int i = 0; i < _jointDots.Length; i++)
            {
                _jointDots[i] = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.LimeGreen,
                    Visibility = Visibility.Hidden
                };
                SkeletonCanvas.Children.Add(_jointDots[i]);
            }

            // All bones from KinectSkeleton definition
            var allBones = KinectSkeleton.AllBones;
            _boneLines = new Line[allBones.Length];

            for (int i = 0; i < _boneLines.Length; i++)
            {
                _boneLines[i] = new Line
                {
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 4,
                    Opacity = 0.7,
                    Visibility = Visibility.Hidden
                };
                SkeletonCanvas.Children.Add(_boneLines[i]);
            }
        }

        public void UpdateSkeleton(KinectSkeleton skeleton)
        {
            if (skeleton == null)
                return;

            // Get all bones with 2D pixel positions (depth view projection)
            var boneDict = skeleton.GetFullSkeletonBonesFrontViewInPixels();

            // --- Draw bones ---
            int i = 0;
            foreach (var pair in boneDict)
            {
                var (ptA, ptB) = pair.Value;

                _boneLines[i].X1 = ptA.X;
                _boneLines[i].Y1 = ptA.Y;
                _boneLines[i].X2 = ptB.X;
                _boneLines[i].Y2 = ptB.Y;
                _boneLines[i].Visibility = Visibility.Visible;
                i++;
            }

            // Hide any remaining lines (if fewer than expected)
            for (; i < _boneLines.Length; i++)
                _boneLines[i].Visibility = Visibility.Hidden;

            // --- Draw joints ---
            var jointDict = skeleton.GetFullSkeletonJointsrontViewInPixels();
            foreach (var pair in jointDict)
            {
                var joint = pair.Key;
                var pt = pair.Value;

                _jointDots[(int)joint].Visibility = Visibility.Visible;
                Canvas.SetLeft(_jointDots[(int)joint], pt.X - 5);
                Canvas.SetTop(_jointDots[(int)joint], pt.Y - 5);
            }
        }
    }
}
