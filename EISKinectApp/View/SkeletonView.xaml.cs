using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Kinect;
using EISKinectApp.Model;

namespace EISKinectApp.View
{
    public partial class SkeletonView
    {
        private readonly Ellipse[] _jointDots;
        private readonly Line[] _boneLines;

        // Define the skeleton structure
        private readonly JointType[][] _bones = {
            new[]{ JointType.Head, JointType.ShoulderCenter },
            new[]{ JointType.ShoulderCenter, JointType.ShoulderLeft },
            new[]{ JointType.ShoulderCenter, JointType.ShoulderRight },
            new[]{ JointType.ShoulderCenter, JointType.Spine },
            new []{ JointType.Spine, JointType.HipCenter },
            new []{ JointType.HipCenter, JointType.HipLeft },
            new []{ JointType.HipCenter, JointType.HipRight },
            new []{ JointType.ShoulderLeft, JointType.ElbowLeft },
            new []{ JointType.ElbowLeft, JointType.WristLeft },
            new []{ JointType.WristLeft, JointType.HandLeft },
            new []{ JointType.ShoulderRight, JointType.ElbowRight },
            new []{ JointType.ElbowRight, JointType.WristRight },
            new []{ JointType.WristRight, JointType.HandRight },
            new []{ JointType.HipLeft, JointType.KneeLeft },
            new []{ JointType.KneeLeft, JointType.AnkleLeft },
            new []{ JointType.AnkleLeft, JointType.FootLeft },
            new []{ JointType.HipRight, JointType.KneeRight },
            new []{ JointType.KneeRight, JointType.AnkleRight },
            new []{ JointType.AnkleRight, JointType.FootRight },
        };

        public SkeletonView()
        {
            InitializeComponent();

            _jointDots = new Ellipse[20];
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

            _boneLines = new Line[_bones.Length];
            for (int i = 0; i < _bones.Length; i++)
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

        /// <summary>
        /// Draws a skeleton from a KinectSkeleton object (which includes calibration)
        /// </summary>
        public void UpdateSkeleton(KinectSkeleton skeleton)
        {
            if (skeleton == null || !skeleton.IsTracked)
                return;

            Dictionary<JointType, Point3D> jointPoints = new Dictionary<JointType, Point3D>();

            // For every joint that’s tracked, get the 2D projection point
            foreach (JointType jointType in (JointType[])System.Enum.GetValues(typeof(JointType)))
            {
                Point3D pt = skeleton.Get3D(jointType);
                if (pt.X >= 0 && pt.Y >= 0)
                {
                    jointPoints[jointType] = pt;

                    _jointDots[(int)jointType].Visibility = Visibility.Visible;
                    Canvas.SetLeft(_jointDots[(int)jointType], pt.X - 5);
                    Canvas.SetTop(_jointDots[(int)jointType], pt.Y - 5);
                }
                else
                {
                    _jointDots[(int)jointType].Visibility = Visibility.Hidden;
                }
            }

            // Draw bones
            for (int i = 0; i < _bones.Length; i++)
            {
                var bone = _bones[i];
                if (jointPoints.ContainsKey(bone[0]) && jointPoints.ContainsKey(bone[1]))
                {
                    _boneLines[i].Visibility = Visibility.Visible;
                    _boneLines[i].X1 = jointPoints[bone[0]].X;
                    _boneLines[i].Y1 = jointPoints[bone[0]].Y;
                    _boneLines[i].X2 = jointPoints[bone[1]].X;
                    _boneLines[i].Y2 = jointPoints[bone[1]].Y;
                }
                else
                {
                    _boneLines[i].Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
