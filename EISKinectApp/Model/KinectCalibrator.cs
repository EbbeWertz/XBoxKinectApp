using System.Collections.Generic;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace EISKinectApp.Model
{
    public class KinectCalibrator
    {
        
        public static Point[] CORNERS2D = { new Point(0,0), new Point(640,0), new Point(640,480), new Point(0,480) };
        private readonly PartialCalibrationClass _calibrator;
        private readonly List<SkeletonPoint> _skeletonPoints =  new List<SkeletonPoint>();
        private readonly List<Point> _screenPoints =  new List<Point>();

        public int CurrentCorner { get; private set; }

        public bool CornersPending() {
            return CurrentCorner <= 3;
        }

        public void AddCorner(SkeletonPoint rawSkeletonPoint) {
            _screenPoints.Add(CORNERS2D[CurrentCorner]);
            _skeletonPoints.Add(rawSkeletonPoint);
            CurrentCorner++;
            if (CurrentCorner == 4) {
                Calibrate();
            }
        }
        public KinectCalibrator(KinectSensor sensor)
        {
            _calibrator = new PartialCalibrationClass(sensor);
        }

        public void Calibrate()
        {
            _calibrator.m_skeletonCalibPoints = _skeletonPoints;
            _calibrator.m_calibPoints = _screenPoints;
            _calibrator.Calibrate();
        }

        public Point MapToFloor2D(SkeletonPoint point)
        {
            return _calibrator.kinectToProjectionPoint(point);
        }
    }
}