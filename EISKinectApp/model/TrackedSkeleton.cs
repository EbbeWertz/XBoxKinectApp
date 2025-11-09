using Microsoft.Kinect;

namespace EISKinectApp.model
{
    public class TrackedSkeleton
    {
        public Skeleton Skeleton { get; private set; }
        public bool IsTracked => Skeleton?.TrackingState == SkeletonTrackingState.Tracked;

        public TrackedSkeleton(Skeleton skeleton)
        {
            Skeleton = skeleton;
        }

        public SkeletonPoint GetJointPosition(JointType jointType)
        {
            return Skeleton?.Joints[jointType].Position ?? new SkeletonPoint();
        }
    }
}