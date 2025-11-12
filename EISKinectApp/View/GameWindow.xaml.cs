using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EISKinectApp.Model.Game;
using EISKinectApp.model.KinectWrapper;
using EISKinectApp.Model.KinectWrapper;
using EISKinectApp.view;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace EISKinectApp.View {
    public partial class GameWindow : Window {
        private readonly DispatcherTimer _gameTimer;
        private readonly Random _rand = new Random();
        private double _checkLineY;
        private readonly GameWindowFloor _floorWindow;
        private static readonly int ArmImageSize = 200;
        private double _pxPerSecondFallSpeed = 5;

        // Kinect
        private readonly KinectManager _kinect;
        private KinectSkeleton latestSkeleton;
        
        public GameWindow() {
            InitializeComponent();
            GestureImageFactory.Initialize();
            _floorWindow = new GameWindowFloor();
            _floorWindow.Show();

            // Kinect initialisatie
            _kinect = KinectManager.Instance;
            _kinect.Start();
            _kinect.SkeletonUpdated += OnSkeletonUpdated;

            // Timer setup
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(30);
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
            SizeChanged += OnSizeChanged;
        }

        protected override void OnClosed(System.EventArgs e) {
            base.OnClosed(e);
            _floorWindow.Close();
            _kinect.Stop();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateCheckLineSize();
        }

        private void UpdateCheckLineSize() {
            _checkLineY = GameCanvas.ActualHeight * 0.9;
            CheckLine.X1 = 0;
            CheckLine.X2 = GameCanvas.ActualWidth;
            CheckLine.Y1 = _checkLineY;
            CheckLine.Y2 = _checkLineY;
        }

        private void OnSkeletonUpdated(KinectSkeleton skeleton)
        {
            latestSkeleton = skeleton;
        }

        private void SpawnGesture() {
            var side = (ArmSide) _rand.Next(2);
            var move = (ArmGesture) _rand.Next(3);

            var leftOrRightName = side == ArmSide.Left ? "right" : "left"; // i know de file namen zijn omgedraaid ja
            var moveName = move == ArmGesture.ArmDown ? "armdown" : move == ArmGesture.ArmUp ? "armup" : "armside";

            var allColors = new []{
                Brushes.Red, Brushes.Blue, Brushes.Yellow,
                Brushes.Purple, Brushes.Green, Brushes.Orange,
            };
            var color = allColors[_rand.Next(6)];
            
            var img = new Image {
                Width = ArmImageSize,
                Height = ArmImageSize,
                Source = GestureImageFactory.Get(leftOrRightName, moveName, color),
                // Source = new BitmapImage(new Uri($"pack://application:,,,/resources/gestures/{leftOrRightName}{moveName}.png")),
                Tag = new GestureImageData { Y = 0, Gesture = move, Side = side }
            };

            var xPos = side == ArmSide.Left ? 50 : (int)GameCanvas.ActualWidth - 50 - ArmImageSize;
            Canvas.SetLeft(img, xPos);
            Canvas.SetTop(img, 0);
            GameCanvas.Children.Add(img);
        }

        private void GameLoop(object sender, EventArgs e) {
            for (var i = GameCanvas.Children.Count - 1; i >= 0; i--) {
                if (!(GameCanvas.Children[i] is Image img) || !(img.Tag is GestureImageData data)) continue;
                data.Y += _pxPerSecondFallSpeed; // snelheid
                Canvas.SetTop(img, data.Y);
                if (!(data.Y + img.Height >= _checkLineY))
                {
                    KinectGestureDetector.CheckArmDanceMove(data.Gesture, data.Side, latestSkeleton);
                }
                GameCanvas.Children.RemoveAt(i);
            }

            if (_rand.NextDouble() < 0.02)
                SpawnGesture();
        }

        private class GestureImageData {
            public double Y { get; set; }
            public ArmGesture Gesture { get; set; }
            public ArmSide Side { get; set; }
        }
    }
}