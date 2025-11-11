using System.Windows;

namespace EISKinectApp.view {
    public partial class GameWindow : Window {

        private readonly GameWindowFloor _floorWindow;
        
        public GameWindow() {
            InitializeComponent();
            _floorWindow = new GameWindowFloor();
            _floorWindow.Show();
        }
        
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _floorWindow.Close();
        }
    }
}