using System.Windows;
using EISKinectApp.model;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace EISKinectApp.service
{
    public class CalibrationService
    {
        private readonly PartialCalibrationClass _calibrator;
        private readonly CalibrationData _data;

        public CalibrationService(KinectSensor sensor, CalibrationData data)
        {
            _calibrator = new PartialCalibrationClass(sensor);
            _data = data;
        }

        public void AddCorner(SkeletonPoint skeletonPoint, Point screenCorner)
        {
            _data.SkeletonPoints.Add(skeletonPoint);
            _data.ScreenPoints.Add(screenCorner);
        }

        public bool IsReadyToCalibrate => _data.IsComplete;

        public void Calibrate()
        {
            _calibrator.m_skeletonCalibPoints = _data.SkeletonPoints;
            _calibrator.m_calibPoints = _data.ScreenPoints;
            _calibrator.Calibrate();
        }

        public Point MapToProjection(SkeletonPoint point)
        {
            return _calibrator.kinectToProjectionPoint(point);
        }
    }
}