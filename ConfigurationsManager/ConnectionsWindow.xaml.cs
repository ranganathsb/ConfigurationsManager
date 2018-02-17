using System.Windows;
using System.Windows.Controls;
using System.Linq;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ConfigurationsManager
{
    public partial class ConnectionsWindow : MetroWindow
    {
        public ConnectionsWindow()
        {
            InitializeComponent();

            Loaded += ConnectionsWindow_Loaded;

            Closing += (sender, args) =>
            {
                var isvalid = VerifyConnection();
                if (!isvalid)
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void ConnectionsWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // load data from config file
            ServerTextbox.Text = Properties.Settings.Default.Server;
            DatabaseTextbox.Text = Properties.Settings.Default.Database;
            UsernameTextbox.Text = Properties.Settings.Default.Username;
            PasswordTextbox.Password = Properties.Settings.Default.Password;
        }

        private async void SaveConnectionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var isvalid = VerifyConnection();

            if (isvalid)
            {
                // save connection to config & close window
                Properties.Settings.Default.Server = ServerTextbox.Text;
                Properties.Settings.Default.Database = DatabaseTextbox.Text;
                Properties.Settings.Default.Username = UsernameTextbox.Text;
                Properties.Settings.Default.Password = PasswordTextbox.Password;

                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                Close();
            }
            else
            {
                await this.ShowMessageAsync("Incomplete", "Please enter all fields");
            }
        }

        private bool VerifyConnection()
        {
            var isvalid = this.FindChildren<TextBox>()
                .All(tb => !string.IsNullOrWhiteSpace(tb.Text));

            isvalid = isvalid && this.FindChildren<PasswordBox>()
                          .All(pw => !string.IsNullOrWhiteSpace(pw.Password));

            return isvalid;
        }
    }
}