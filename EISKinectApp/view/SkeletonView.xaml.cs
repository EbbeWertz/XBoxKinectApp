using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Windows;

namespace EISKinectApp.view
{
    public partial class SkeletonView
    {
        private readonly Ellipse[] _jointDots;
        private readonly Line[] _boneLines;

        private readonly JointType[][] _bones = new JointType[][]
        {
            new JointType[]{ JointType.Head, JointType.ShoulderCenter },
            new JointType[]{ JointType.ShoulderCenter, JointType.ShoulderLeft },
            new JointType[]{ JointType.ShoulderCenter, JointType.ShoulderRight },
            new JointType[]{ JointType.ShoulderCenter, JointType.Spine },
            new JointType[]{ JointType.Spine, JointType.HipCenter },
            new JointType[]{ JointType.HipCenter, JointType.HipLeft },
            new JointType[]{ JointType.HipCenter, JointType.HipRight },

            new JointType[]{ JointType.ShoulderLeft, JointType.ElbowLeft },
            new JointType[]{ JointType.ElbowLeft, JointType.WristLeft },
            new JointType[]{ JointType.WristLeft, JointType.HandLeft },

            new JointType[]{ JointType.ShoulderRight, JointType.ElbowRight },
            new JointType[]{ JointType.ElbowRight, JointType.WristRight },
            new JointType[]{ JointType.WristRight, JointType.HandRight },

            new JointType[]{ JointType.HipLeft, JointType.KneeLeft },
            new JointType[]{ JointType.KneeLeft, JointType.AnkleLeft },
            new JointType[]{ JointType.AnkleLeft, JointType.FootLeft },

            new JointType[]{ JointType.HipRight, JointType.KneeRight },
            new JointType[]{ JointType.KneeRight, JointType.AnkleRight },
            new JointType[]{ JointType.AnkleRight, JointType.FootRight },
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

        public void UpdateSkeleton(Skeleton skeleton, CoordinateMapper mapper)
        {
            if (skeleton == null) return;

            // Map joints to depth points
            Dictionary<JointType, System.Windows.Point> points = new Dictionary<JointType, System.Windows.Point>();

            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.NotTracked)
                {
                    DepthImagePoint pt = mapper.MapSkeletonPointToDepthPoint(joint.Position, DepthImageFormat.Resolution640x480Fps30);
                    points[joint.JointType] = new System.Windows.Point(pt.X, pt.Y);

                    _jointDots[(int)joint.JointType].Visibility = Visibility.Visible;
                    Canvas.SetLeft(_jointDots[(int)joint.JointType], pt.X - 5);
                    Canvas.SetTop(_jointDots[(int)joint.JointType], pt.Y - 5);
                }
                else
                {
                    _jointDots[(int)joint.JointType].Visibility = Visibility.Hidden;
                }
            }

            // Draw bones
            for (int i = 0; i < _bones.Length; i++)
            {
                var bone = _bones[i];
                if (points.ContainsKey(bone[0]) && points.ContainsKey(bone[1]))
                {
                    _boneLines[i].Visibility = Visibility.Visible;
                    _boneLines[i].X1 = points[bone[0]].X;
                    _boneLines[i].Y1 = points[bone[0]].Y;
                    _boneLines[i].X2 = points[bone[1]].X;
                    _boneLines[i].Y2 = points[bone[1]].Y;
                }
                else
                {
                    _boneLines[i].Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
