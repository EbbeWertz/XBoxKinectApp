using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.Model.KinectWrapper
{
    public static class KinectGestureDetector
    {
        
        public static bool HandsRaisedAboveHead(KinectSkeleton skeleton)
        {
            var head = skeleton.GetFrontViewInMeters(JointType.Head);
            var leftHand = skeleton.GetFrontViewInMeters(JointType.HandLeft);
            var rightHand = skeleton.GetFrontViewInMeters(JointType.HandRight);
            return leftHand.Y >= head.Y + 0.10f && rightHand.Y >= head.Y + 0.10f;
        }
        public static bool HandsLoweredBelowHead(KinectSkeleton skeleton)
        {
            var head = skeleton.GetFrontViewInMeters(JointType.Head);
            var leftHand = skeleton.GetFrontViewInMeters(JointType.HandLeft);
            var rightHand = skeleton.GetFrontViewInMeters(JointType.HandRight);
            return leftHand.Y <= head.Y - 0.15f && rightHand.Y <= head.Y + 0.15f;
        }
    }
}