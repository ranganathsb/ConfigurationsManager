using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace ConfigurationsManager
{
    public partial class MainWindow : MetroWindow
    {
        private DataModel _dataModel;
        private IEnumerable<Configuration> _configurations = new Configuration[] { };
        private IEnumerable<FeatureFlag> _featureFlags = new FeatureFlag[] { };

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var connections = new ConnectionsWindow();
            connections.ShowDialog();

            _dataModel = new DataModel(
                Properties.Settings.Default.Server,
                Properties.Settings.Default.Database,
                Properties.Settings.Default.Username,
                Properties.Settings.Default.Password);
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            _configurations = _dataModel.Configurations.ToList();
            _featureFlags = _dataModel.FeatureFlags.ToList();

            PopulateDataGrids();
        }

        private void SearchTextbox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            PopulateDataGrids();
        }

        private void ConfigDatagrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConfigDatagrid.SelectedItem is Configuration selection)
            {
                ConfigInstanceTextbox.Text = selection.InstanceName;
                ConfigKeyTextbox.Text = selection.ConfigurationKey;
                ConfigValueTextbox.Text = selection.ConfigurationValue;
            }
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private void FeaturesDatagrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeaturesDatagrid.SelectedItem is FeatureFlag selection)
            {
                FeatureInstanceTextbox.Text = selection.InstanceName;
                FeatureKeyTextbox.Text = selection.FlagName;
                FeatureValueToggle.IsChecked = selection.FlagValue;
            }
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TablesTab.SelectedIndex == 0)
            {
                // configuration
                if (ConfigDatagrid.SelectedItem is Configuration selection)
                {
                    var record = _dataModel.Configurations.FirstOrDefault(c =>
                        c.InstanceName == selection.InstanceName
                        && c.ConfigurationKey == selection.ConfigurationKey
                        && c.ConfigurationValue == selection.ConfigurationValue);

                    if (record != null)
                    {
                        _dataModel.Configurations.Remove(record);
                        _dataModel.SaveChanges();
                    }
                }
            }
            else
            {
                // feature flag
                if (FeaturesDatagrid.SelectedItem is FeatureFlag selection)
                {
                    var record = _dataModel.FeatureFlags.FirstOrDefault(c =>
                        c.InstanceName == selection.InstanceName
                        && c.FlagName == selection.FlagName
                        && c.FlagValue == selection.FlagValue);

                    if (record != null)
                    {
                        _dataModel.FeatureFlags.Remove(record);
                        _dataModel.SaveChanges();
                    }
                }
            }

            PopulateDataGrids();
        }

        private void UpsertButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TablesTab.SelectedIndex == 0)
            {
                // configuration
                var config = new Configuration
                {
                    InstanceName = ConfigInstanceTextbox.Text,
                    ConfigurationKey = ConfigKeyTextbox.Text,
                    ConfigurationValue = ConfigValueTextbox.Text
                };

                if (ConfigDatagrid.SelectedItem is Configuration selection)
                {
                    // update
                    var record = _dataModel.Configurations.FirstOrDefault(c =>
                        c.InstanceName == selection.InstanceName
                        && c.ConfigurationKey == selection.ConfigurationKey
                        && c.ConfigurationValue == selection.ConfigurationValue);

                    if (record != null)
                    {
                        record.InstanceName = config.InstanceName;
                        record.ConfigurationKey = config.ConfigurationKey;
                        record.ConfigurationValue = config.ConfigurationValue;

                        _dataModel.SaveChanges();
                    }
                }
                else
                {
                    // insert
                    _dataModel.Configurations.Add(config);
                    _dataModel.SaveChanges();
                }
            }
            else
            {
                // feature flag
                var feature = new FeatureFlag
                {
                    InstanceName = FeatureInstanceTextbox.Text,
                    FlagName = FeatureKeyTextbox.Text,
                    FlagValue = FeatureValueToggle.IsChecked.GetValueOrDefault()
                };

                if (FeaturesDatagrid.SelectedItem is FeatureFlag selection)
                {
                    // update
                    var record = _dataModel.FeatureFlags.FirstOrDefault(c =>
                        c.InstanceName == selection.InstanceName
                        && c.FlagName == selection.FlagName
                        && c.FlagValue == selection.FlagValue);

                    if (record != null)
                    {
                        record.InstanceName = feature.InstanceName;
                        record.FlagName = feature.FlagName;
                        record.FlagValue = feature.FlagValue;

                        _dataModel.SaveChanges();
                    }
                }
                else
                {
                    // insert
                    _dataModel.FeatureFlags.Add(feature);
                    _dataModel.SaveChanges();
                }
            }
            
            PopulateDataGrids();
        }

        private void PopulateDataGrids()
        {
            ClearControls();

            ConfigDatagrid.ItemsSource = _configurations
                .Where(c =>
                    (c.InstanceName != null && c.InstanceName.ToLower().Contains(SearchTextbox.Text.ToLower().Trim()))
                    || (c.ConfigurationKey != null && c.ConfigurationKey.ToLower().Contains(SearchTextbox.Text.ToLower().Trim()))
                    || (c.ConfigurationValue != null && c.ConfigurationValue.ToLower().Contains(SearchTextbox.Text.ToLower().Trim())));

            FeaturesDatagrid.ItemsSource = _featureFlags
                .Where(f =>
                    (f.InstanceName != null && f.InstanceName.ToLower().Contains(SearchTextbox.Text.ToLower().Trim()))
                    || (f.FlagName != null && f.FlagName.ToLower().Contains(SearchTextbox.Text.ToLower().Trim())));
        }

        private void ClearControls()
        {
            ConfigInstanceTextbox.Text = string.Empty;
            ConfigKeyTextbox.Text = string.Empty;
            ConfigValueTextbox.Text = string.Empty;

            FeatureInstanceTextbox.Text = string.Empty;
            FeatureKeyTextbox.Text = string.Empty;
            FeatureValueToggle.IsChecked = null;

            ConfigDatagrid.SelectedIndex = -1;
            FeaturesDatagrid.SelectedIndex = -1;
        }
    }
}