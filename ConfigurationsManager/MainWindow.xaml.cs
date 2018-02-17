using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;

namespace ConfigurationsManager
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private static void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var connections = new ConnectionsWindow();
            connections.ShowDialog();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}