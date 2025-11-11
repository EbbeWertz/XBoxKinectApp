using System.Collections.Generic;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace EISKinectApp.model.KinectWrapper {
    public class KinectCalibratedProjector {

        private readonly PartialCalibrationClass _calibration;
        private readonly List<SkeletonPoint> _skeletonPoints;
        private readonly List<Point> _screenPoints;

        public int CurrentPoint { get; set; }

        public KinectCalibratedProjector(KinectSensor kinectSensor) {
            _calibration = new PartialCalibrationClass(kinectSensor);
            _skeletonPoints = new  List<SkeletonPoint>(4);
            _screenPoints = new  List<Point>(4);
            CurrentPoint = 0;
        }

        public void RegisterCorner(SkeletonPoint skeletonPoint, Point screenPoint) {
            if (ReadyToCalibrate()) {
                return;
            }
            _skeletonPoints.Add(skeletonPoint);
            _screenPoints.Add(screenPoint);
            CurrentPoint++;
        }

        private bool ReadyToCalibrate() {
            return CurrentPoint == 4;
        }

        public void Calibrate() {
            _calibration.m_calibPoints = _screenPoints;
            _calibration.m_skeletonCalibPoints = _skeletonPoints;
            _calibration.Calibrate();
        }

        public Point ProjectToFloorCoordinate(SkeletonPoint skeletonPoint) {
            return _calibration.kinectToProjectionPoint(skeletonPoint);
        }
    }
}