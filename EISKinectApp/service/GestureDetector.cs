using Microsoft.Kinect;

namespace EISKinectApp.service {
    public class GestureDetector {
        private bool _gestureActive = false;
        private int _holdFrames = 0;
        private readonly int _requiredHoldFrames;

        public GestureDetector(int requiredHoldFrames = 5) {
            _requiredHoldFrames = requiredHoldFrames;
        }

        public bool CheckCaptureGesture(Skeleton skeleton) {
            if (skeleton == null) return false;

            Joint head = skeleton.Joints[JointType.Head];
            Joint handRight = skeleton.Joints[JointType.HandRight];
            Joint handLeft = skeleton.Joints[JointType.HandLeft];

            float triggerOffset = 0.10f;
            float resetOffset = 0.15f;

            float headY = head.Position.Y;
            float triggerY = headY + triggerOffset;
            float resetY = headY - resetOffset;

            bool bothHandsAbove = handRight.Position.Y > triggerY && handLeft.Position.Y > triggerY;
            bool bothHandsBelow = handRight.Position.Y < resetY && handLeft.Position.Y < resetY;

            if (bothHandsAbove && !_gestureActive) {
                _holdFrames++;
                if (_holdFrames > _requiredHoldFrames) {
                    _gestureActive = true;
                    _holdFrames = 0;
                    return true;
                }
            }
            else if (bothHandsBelow) {
                _gestureActive = false;
                _holdFrames = 0;
            }
            else {
                _holdFrames = 0;
            }

            return false;
        }
    }
}