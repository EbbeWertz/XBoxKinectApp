using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace EISKinectApp.Model
{
    public class KinectSkeleton
    {
        private readonly Skeleton _skeleton;
        private readonly KinectCalibrator _kinectCalibrator;

        public bool IsTracked => _skeleton?.TrackingState == SkeletonTrackingState.Tracked;

        public KinectSkeleton(Skeleton skeleton, KinectCalibrator kinectCalibrator = null)
        {
            _skeleton = skeleton;
            _kinectCalibrator = kinectCalibrator;
        }

        public Point3D Get3D(JointType joint)
        {
            var j = _skeleton.Joints[joint];
            return new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
        }

        public Point GetFloor2D(JointType joint)
        {
            if (_kinectCalibrator == null)
                return new Point(double.NaN, double.NaN);

            return _kinectCalibrator.MapToFloor2D(_skeleton.Joints[joint].Position);
        }

        public Dictionary<JointType, Point3D> GetFullSkeleton3D(IEnumerable<JointType> joints)
        {
            return joints.ToDictionary(j => j, Get3D);
        }
    }
}