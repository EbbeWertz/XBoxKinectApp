using System;
using EISKinectApp.Model.Game;
using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.Model.KinectWrapper {
    public static class KinectGestureDetector {
        public static bool HandsRaisedAboveHead(KinectSkeleton skeleton) {
            var head = skeleton.GetFrontViewInMeters(JointType.Head);
            var leftHand = skeleton.GetFrontViewInMeters(JointType.HandLeft);
            var rightHand = skeleton.GetFrontViewInMeters(JointType.HandRight);
            return leftHand.Y >= head.Y + 0.10f && rightHand.Y >= head.Y + 0.10f;
        }

        public static bool HandsLoweredBelowHead(KinectSkeleton skeleton) {
            var head = skeleton.GetFrontViewInMeters(JointType.Head);
            var leftHand = skeleton.GetFrontViewInMeters(JointType.HandLeft);
            var rightHand = skeleton.GetFrontViewInMeters(JointType.HandRight);
            return leftHand.Y <= head.Y - 0.15f && rightHand.Y <= head.Y + 0.15f;
        }

        public static bool CheckArmDanceMove(ArmGesture move, ArmSide side, KinectSkeleton skeleton) {
            if (side == ArmSide.Left) {
                switch (move) {
                    case ArmGesture.ArmUp:
                        return LeftArmUp(skeleton);
                    case ArmGesture.ArmDown:
                        return LeftArmDown(skeleton);
                    case ArmGesture.ArmMiddle:
                        return LeftArmSide(skeleton);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(move), move, null);
                }
            }
            switch (move) {
                case ArmGesture.ArmUp:
                    return RightArmUp(skeleton);
                case ArmGesture.ArmDown:
                    return RightArmDown(skeleton);
                case ArmGesture.ArmMiddle:
                    return RightArmSide(skeleton);
                default:
                    throw new ArgumentOutOfRangeException(nameof(move), move, null);
            }
        }

        // -----------------------------
        // Linkerarm omhoog \_
        // -----------------------------
        public static bool LeftArmUp(KinectSkeleton skeleton) {
            var shoulder = skeleton.GetFrontViewInMeters(JointType.ShoulderLeft);
            var elbow = skeleton.GetFrontViewInMeters(JointType.ElbowLeft);
            var hand = skeleton.GetFrontViewInMeters(JointType.HandLeft);

            return hand.Y > elbow.Y + 0.2 && elbow.Y < shoulder.Y + 0.1 && elbow.Y > shoulder.Y - 0.1;
        }

        // -----------------------------
        // Linkerarm opzij __
        // -----------------------------
        public static bool LeftArmSide(KinectSkeleton skeleton) {
            var shoulder = skeleton.GetFrontViewInMeters(JointType.ShoulderLeft);
            var hand = skeleton.GetFrontViewInMeters(JointType.HandLeft);

            return hand.Y < shoulder.Y + 0.1 && hand.Y > shoulder.Y - 0.1;
        }

        // -----------------------------
        // Linkerarm omlaag /¨¨
        // -----------------------------
        public static bool LeftArmDown(KinectSkeleton skeleton) {
            var shoulder = skeleton.GetFrontViewInMeters(JointType.ShoulderLeft);
            var hand = skeleton.GetFrontViewInMeters(JointType.HandLeft);
            var elbow = skeleton.GetFrontViewInMeters(JointType.ElbowLeft);

            return hand.Y < elbow.Y - 0.2 && elbow.Y < shoulder.Y + 0.1 && elbow.Y > shoulder.Y - 0.1;
        }

        // -----------------------------
        // Rechterarm omhoog _/
        // -----------------------------
        public static bool RightArmUp(KinectSkeleton skeleton) {
            var shoulder = skeleton.GetFrontViewInMeters(JointType.ShoulderRight);
            var elbow = skeleton.GetFrontViewInMeters(JointType.ElbowRight);
            var hand = skeleton.GetFrontViewInMeters(JointType.HandRight);

            return hand.Y > elbow.Y + 0.2 && elbow.Y < shoulder.Y + 0.1 && elbow.Y > shoulder.Y - 0.1;
        }

        // -----------------------------
        // Rechterarm opzij __
        // -----------------------------
        public static bool RightArmSide(KinectSkeleton skeleton) {
            var shoulder = skeleton.GetFrontViewInMeters(JointType.ShoulderRight);
            var hand = skeleton.GetFrontViewInMeters(JointType.HandRight);

            return hand.Y < shoulder.Y + 0.1 && hand.Y > shoulder.Y - 0.1;
        }

        // -----------------------------
        // Rechterarm omlaag ¨¨\
        // -----------------------------
        public static bool RightArmDown(KinectSkeleton skeleton) {
            var shoulder = skeleton.GetFrontViewInMeters(JointType.ShoulderRight);
            var hand = skeleton.GetFrontViewInMeters(JointType.HandRight);
            var elbow = skeleton.GetFrontViewInMeters(JointType.ElbowRight);

            return hand.Y < elbow.Y - 0.2 && elbow.Y < shoulder.Y + 0.1 && elbow.Y > shoulder.Y - 0.1;
        }
    }
}