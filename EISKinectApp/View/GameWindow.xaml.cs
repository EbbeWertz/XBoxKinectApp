using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EISKinectApp.Model.Game;
using EISKinectApp.model.KinectWrapper;
using EISKinectApp.Model.KinectWrapper;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace EISKinectApp.View {
    public partial class GameWindow : Window {
        private readonly DispatcherTimer _gameTimer;
        private readonly DispatcherTimer _spawnTimer;
        private readonly Random _rand = new Random();
        private double _checkLineY;
        private readonly GameWindowFloor _floorWindow;
        private static readonly int ArmImageSize = 200;
        private double _pxPerSecondFallSpeed = 2;
        private readonly GameFloor _gameFloor;
        private SolidColorBrush _currentStandingColor;
        double spawnInterval = 2.5;

        // Kinect
        private readonly KinectManager _kinect;
        private KinectSkeleton latestSkeleton;

        public GameWindow() {
            InitializeComponent();

            _gameFloor = new GameFloor();
            _gameFloor.ColorStepUpdated += UpdateHighlights;
            _gameFloor.FeetUpdated += UpdateFeet;
            GestureImageFactory.Initialize();
            _floorWindow = new GameWindowFloor(_gameFloor);
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

            _spawnTimer = new DispatcherTimer();
            _spawnTimer.Interval = TimeSpan.FromSeconds(spawnInterval);
            _spawnTimer.Tick += SpawnGesture;
            _spawnTimer.Start();

            SizeChanged += OnSizeChanged;
        }

        private void UpdateFeet(Point left, Point right) {
            _floorWindow.UpdateFeet(left, right);
        }

        private void UpdateHighlights(int left, int right, SolidColorBrush color) {
            _floorWindow.UpdateHighlights(left, right);
            SkeletonOverlay.Color = color;
            _currentStandingColor = color;
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
            _checkLineY = GameCanvas.ActualHeight * 0.75;
            CheckLine.X1 = 0;
            CheckLine.X2 = GameCanvas.ActualWidth;
            CheckLine.Y1 = _checkLineY;
            CheckLine.Y2 = _checkLineY;
        }

        private void OnSkeletonUpdated(KinectSkeleton skeleton) {
            SkeletonOverlay.UpdateSkeleton(skeleton);
            latestSkeleton = skeleton;
        }

        private void SpawnGesture(object sender, EventArgs e) {
            var side = (ArmSide)_rand.Next(2);
            var move = (ArmGesture)_rand.Next(3);

            var leftOrRightName = side == ArmSide.Left ? "right" : "left"; // i know de file namen zijn omgedraaid ja
            var moveName = move == ArmGesture.ArmDown ? "armdown" : move == ArmGesture.ArmUp ? "armup" : "armside";

            var allColors = new[] {
                Brushes.Red, Brushes.Blue, Brushes.Yellow,
                Brushes.Purple, Brushes.Green, Brushes.Orange,
            };
            var color = allColors[_rand.Next(6)];

            var img = new Image {
                Width = ArmImageSize,
                Height = ArmImageSize,
                Source = GestureImageFactory.Get(leftOrRightName, moveName, color),
                // Source = new BitmapImage(new Uri($"pack://application:,,,/resources/gestures/{leftOrRightName}{moveName}.png")),
                Tag = new GestureImageData { Y = 0, Gesture = move, Side = side, Color = color }
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
                if (!(data.Y + img.Height / 2 >= _checkLineY)) {
                    var good = KinectGestureDetector.CheckArmDanceMove(data.Gesture, data.Side, latestSkeleton)
                               && _currentStandingColor == data.Color;
                    if (good) {
                        _gameFloor.updateScore(10);
                        StatusText.Text = "score: " + _gameFloor.getScore();
                        ShowFloatingText("+10!", Brushes.LimeGreen);
                        GameCanvas.Children.RemoveAt(i);
                        spawnInterval *= 0.97;
                        _spawnTimer.Interval = TimeSpan.FromSeconds(spawnInterval);
                        _pxPerSecondFallSpeed += 0.1;
                    }

                    continue;
                }

                ShowFloatingText("Miss!", Brushes.Red);
                GameCanvas.Children.RemoveAt(i);
            }
        }

        private void ShowFloatingText(string message, SolidColorBrush color) {
            var text = new TextBlock {
                Text = message,
                Foreground = color,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Opacity = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Center it on the game canvas
            Canvas.SetLeft(text, GameCanvas.ActualWidth / 2 - 30);
            Canvas.SetTop(text, _checkLineY - 50);
            GameCanvas.Children.Add(text);

            // Fade-in + float + fade-out animation
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150));
            var moveUp = new DoubleAnimation(Canvas.GetTop(text), Canvas.GetTop(text) - 40, TimeSpan.FromSeconds(0.8));
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.8)) {
                BeginTime = TimeSpan.FromSeconds(0.5)
            };

            // Apply animations
            text.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            text.BeginAnimation(Canvas.TopProperty, moveUp);
            text.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            // Cleanup after animation
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.2) };
            timer.Tick += (s, ev) => {
                GameCanvas.Children.Remove(text);
                timer.Stop();
            };
            timer.Start();
        }

        private class GestureImageData {
            public double Y { get; set; }
            public ArmGesture Gesture { get; set; }
            public ArmSide Side { get; set; }
            public SolidColorBrush Color { get; set; }
        }
    }
}