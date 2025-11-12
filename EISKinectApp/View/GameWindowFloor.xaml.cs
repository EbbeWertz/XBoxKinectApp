using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EISKinectApp.Model.Game;
using EISKinectApp.model.KinectWrapper;
using Microsoft.Kinect;

namespace EISKinectApp.View {
    public partial class GameWindowFloor : Window {
        private readonly GameFloor _gameFloor;

        public GameWindowFloor() {
            InitializeComponent();
            _gameFloor = new GameFloor();
            _circleViews = new[] { CircleRed, CircleBlue, CircleYellow };
            InitCircles();
            _gameFloor.ColorStepUpdated += UpdateHighlights;
            _gameFloor.FeetUpdated += UpdateFeet;
        }

        private readonly Ellipse[] _circleViews;

        private readonly SolidColorBrush[] _darkColors = {
            new SolidColorBrush(Color.FromRgb(128, 0, 0)), // dark red
            new SolidColorBrush(Color.FromRgb(0, 0, 128)), // dark blue
            new SolidColorBrush(Color.FromRgb(128, 128, 0)) // dark yellow
        };

        private readonly SolidColorBrush[] _lightColors = {
            new SolidColorBrush(Color.FromRgb(255, 128, 128)), // dark red
            new SolidColorBrush(Color.FromRgb(128, 128, 255)), // dark blue
            new SolidColorBrush(Color.FromRgb(255, 255, 128)) // dark yellow
        };

        private void InitCircles() {
            var r = GameFloor.ColorCircleRadius;

            for (var i = 0; i < 3; i++) {
                var center = _gameFloor.CircleCenters[i];
                _circleViews[i].Width = r * 2;
                _circleViews[i].Height = r * 2;
                Canvas.SetLeft(_circleViews[i], center.X - r);
                Canvas.SetTop(_circleViews[i], center.Y - r);
                _circleViews[i].Fill = _darkColors[i];
                _circleViews[i].Stroke = Brushes.Gray;
            }
        }

        private void UpdateHighlights(int leftCircle, int rightCircle, SolidColorBrush totalColor) {
            for (var i = 0; i < _circleViews.Length; i++) {
                var active = (i == leftCircle || i == rightCircle);
                _circleViews[i].Fill = active ? _lightColors[i] : _darkColors[i];
                _circleViews[i].Stroke = active ? Brushes.White : Brushes.Gray;
            }
        }

        private void UpdateFeet(Point left, Point right) {
            var radius = FootLeft.Width / 2;

            Canvas.SetLeft(FootLeft, left.X - radius);
            Canvas.SetTop(FootLeft, left.Y - radius);
            FootLeft.Visibility = Visibility.Visible;

            Canvas.SetLeft(FootRight, right.X - radius);
            Canvas.SetTop(FootRight, right.Y - radius);
            FootRight.Visibility = Visibility.Visible;
        }
    }
}