using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using EISKinectApp.Model.KinectWrapper; // jouw bestaande namespaces
using EISKinectApp.model.KinectWrapper;
using EISKinectApp.view;

namespace EISKinectApp.View
{
    public partial class GameWindow : Window
    {
        private readonly DispatcherTimer _gameTimer;
        private readonly Random _rand = new Random();
        private double _checkLineY;
        private readonly GameWindowFloor _floorWindow;

        // Kinect
        private readonly KinectManager _kinect;
        private KinectSkeleton _latestSkeleton;

        public GameWindow()
        {
            InitializeComponent();
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

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PositionCheckLine();
            _gameTimer.Start();
            SpawnGesture();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _floorWindow.Close();
            _kinect.Stop();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionCheckLine();
        }

        private void PositionCheckLine()
        {
            _checkLineY = GameCanvas.ActualHeight * 0.9;
            CheckLine.X1 = 0;
            CheckLine.X2 = GameCanvas.ActualWidth;
            CheckLine.Y1 = _checkLineY;
            CheckLine.Y2 = _checkLineY;
        }

        private void OnSkeletonUpdated(KinectSkeleton skeleton)
        {
            _latestSkeleton = skeleton;
        }

        private void SpawnGesture()
        {
            // Alle gestures die je hebt gedefinieerd
            var gestureTypes = new[]
            {
                "leftarmup", "leftarmdown", "leftarmside",
                "rightarmup", "rightarmdown", "rightarmside"
            };

            var type = gestureTypes[_rand.Next(gestureTypes.Length)];

            var img = new Image
            {
                Width = 80,
                Height = 80,
                Source = new BitmapImage(new Uri($"pack://application:,,,/resources/gestures/{type}.png")),
                Tag = new GestureData { Y = 0, Type = type }
            };

            // Vaste x-positie per kant
            double xPos;
            if (type.StartsWith("left"))
            {
                xPos = 50; // linkerkant
            }
            else
            {
                xPos = GameCanvas.ActualWidth - 130; // rechterkant, 80 px breed + marge
            }

            Canvas.SetLeft(img, xPos);
            Canvas.SetTop(img, 0);
            GameCanvas.Children.Add(img);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            for (int i = GameCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (GameCanvas.Children[i] is Image img && img.Tag is GestureData data)
                {
                    data.Y += 5; // snelheid
                    Canvas.SetTop(img, data.Y);

                    if (data.Y + img.Height >= _checkLineY)
                    {
                        CheckGesture(data.Type);
                        GameCanvas.Children.RemoveAt(i);
                    }
                }
            }

            if (_rand.NextDouble() < 0.02)
                SpawnGesture();
        }

        private void CheckGesture(string gestureType)
        {
            if (_latestSkeleton == null) return;

            bool correct = false;

            switch (gestureType)
            {
                case "LeftArmUp":
                    correct = KinectGestureDetector.LeftArmUp(_latestSkeleton);
                    break;
                case "LeftArmDown":
                    correct = KinectGestureDetector.LeftArmDown(_latestSkeleton);
                    break;
                case "LeftArmSide":
                    correct = KinectGestureDetector.LeftArmSide(_latestSkeleton);
                    break;
                case "RightArmUp":
                    correct = KinectGestureDetector.RightArmUp(_latestSkeleton);
                    break;
                case "RightArmDown":
                    correct = KinectGestureDetector.RightArmDown(_latestSkeleton);
                    break;
                case "RightArmSide":
                    correct = KinectGestureDetector.RightArmSide(_latestSkeleton);
                    break;
            }

            StatusText.Text = correct ? $"✅ Correct: {gestureType}" : $"❌ Fout: {gestureType}";
        }

        private class GestureData
        {
            public double Y { get; set; }
            public string Type { get; set; }
        }
    }
}
