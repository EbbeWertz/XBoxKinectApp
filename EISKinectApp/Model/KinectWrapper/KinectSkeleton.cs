using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Kinect;
using Point = System.Windows.Point;

namespace EISKinectApp.model.KinectWrapper {
    public class KinectSkeleton {
        private readonly Skeleton _rawSkeleton;
        private readonly KinectCalibratedProjector _calibratedProjector;
        private readonly CoordinateMapper _mapper;

        public static readonly (JointType, JointType)[] AllBones = {
            (JointType.Head, JointType.ShoulderCenter),
            (JointType.ShoulderCenter, JointType.ShoulderLeft),
            (JointType.ShoulderCenter, JointType.ShoulderRight),
            (JointType.ShoulderCenter, JointType.Spine),
            (JointType.Spine, JointType.HipCenter),
            (JointType.HipCenter, JointType.HipLeft),
            (JointType.HipCenter, JointType.HipRight),
            (JointType.ShoulderRight, JointType.ElbowRight),
            (JointType.ElbowRight, JointType.WristRight),
            (JointType.WristRight, JointType.HandRight),
            (JointType.ShoulderLeft, JointType.ElbowLeft),
            (JointType.ElbowLeft, JointType.WristLeft),
            (JointType.WristLeft, JointType.HandLeft),
            (JointType.HipRight, JointType.KneeRight),
            (JointType.KneeRight, JointType.AnkleRight),
            (JointType.AnkleRight, JointType.FootRight),
            (JointType.HipLeft, JointType.KneeLeft),
            (JointType.KneeLeft, JointType.AnkleLeft),
            (JointType.AnkleLeft, JointType.FootLeft),
        };

        private readonly JointType[] _allJoints = (JointType[])Enum.GetValues(typeof(JointType));

        public KinectSkeleton(Skeleton rawSkeleton, KinectCalibratedProjector calibratedProjector, CoordinateMapper mapper) {
            _rawSkeleton = rawSkeleton;
            _calibratedProjector = calibratedProjector;
            _mapper = mapper;
        }

        public PointF GetFrontViewInMeters(JointType joint) {
            var j = _rawSkeleton.Joints[joint];
            return new PointF(j.Position.X, j.Position.Y);
        }

        public Point GetFrontViewInPixels(JointType joint) {
            var j = _rawSkeleton.Joints[joint];
            var pt = _mapper.MapSkeletonPointToDepthPoint(j.Position, DepthImageFormat.Resolution640x480Fps30);
            return new Point(pt.X, pt.Y);
        }

        public Point GetFloorInPixels(JointType joint) {
            return _calibratedProjector?.ProjectToFloorCoordinate(_rawSkeleton.Joints[joint].Position) ??
                   throw new Exception(
                       "Cannot project point to floor coordinate because calibrator is not initialised");
        }

        public Dictionary<JointType, Point> GetFullSkeletonJointsrontViewInPixels() {
            return _allJoints.ToDictionary(j => j, GetFrontViewInPixels);
        }

        public Dictionary<(JointType, JointType), (Point, Point)> GetFullSkeletonBonesFrontViewInPixels() {
            return AllBones.ToDictionary(
                bonesTuple => bonesTuple,
                bonesTuple => (GetFrontViewInPixels(bonesTuple.Item1), GetFrontViewInPixels(bonesTuple.Item2))
            );
        }
    }
}