using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using EISKinectApp.model;
using EISKinectApp.service;
using Microsoft.Kinect;

namespace EISKinectApp.view
{
    public partial class TrackingTestWindow
    {
        private DispatcherTimer _trackingTimer;
        private TrackedSkeleton _currentSkeleton;
        private CalibrationService _calibrationService;

        public TrackingTestWindow(TrackedSkeleton currentSkeleton, CalibrationService calibrationService)
        {
            InitializeComponent();

            _currentSkeleton = currentSkeleton;
            _calibrationService = calibrationService;

            _trackingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33)
            };
            _trackingTimer.Tick += TrackingTimer_Tick;
            _trackingTimer.Start();
        }

        private void TrackingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentSkeleton == null) return;

            Point p2D = _calibrationService.MapToProjection(
                _currentSkeleton.GetJointPosition(JointType.HipCenter));

            if (TrackingCanvas.Children.Count == 0)
            {
                Ellipse playerDot = new Ellipse { Width = 20, Height = 20, Fill = System.Windows.Media.Brushes.Red };
                TrackingCanvas.Children.Add(playerDot);
            }

            Ellipse dot = TrackingCanvas.Children[0] as Ellipse;
            Canvas.SetLeft(dot, p2D.X - 10);
            Canvas.SetTop(dot, p2D.Y - 10);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _trackingTimer.Stop();
        }
    }
}