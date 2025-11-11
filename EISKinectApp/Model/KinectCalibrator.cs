using System.Collections.Generic;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace EISKinectApp.Model
{
    public class KinectCalibrator
    {
        private readonly PartialCalibrationClass _calibrator;

        public KinectCalibrator(KinectSensor sensor)
        {
            _calibrator = new PartialCalibrationClass(sensor);
        }

        public void Calibrate(List<SkeletonPoint> skeletonPoints, List<Point> screenPoints)
        {
            _calibrator.m_skeletonCalibPoints = skeletonPoints;
            _calibrator.m_calibPoints = screenPoints;
            _calibrator.Calibrate();
        }

        public Point MapToFloor2D(SkeletonPoint point)
        {
            return _calibrator.kinectToProjectionPoint(point);
        }
    }
}