using Microsoft.Kinect;

namespace EISKinectApp.Model
{
    public static class KinectGestureDetector
    {
        
        public static bool HandsRaisedAboveHead(KinectSkeleton skeleton)
        {
            var head = skeleton.Get3D(JointType.Head);
            var leftHand = skeleton.Get3D(JointType.HandLeft);
            var rightHand = skeleton.Get3D(JointType.HandRight);
            return leftHand.Y >= head.Y + 0.10f && rightHand.Y >= head.Y + 0.10f;
        }
        public static bool HandsLoweredBelowHead(KinectSkeleton skeleton)
        {
            var head = skeleton.Get3D(JointType.Head);
            var leftHand = skeleton.Get3D(JointType.HandLeft);
            var rightHand = skeleton.Get3D(JointType.HandRight);
            return leftHand.Y <= head.Y - 0.15f && rightHand.Y <= head.Y + 0.15f;
        }
    }
}
