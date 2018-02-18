using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Table;

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

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            _configurations = _dataModel.Configurations.ToList();
            _featureFlags = _dataModel.FeatureFlags.ToList();

            PopulateDataGrids();

            var dialog = new SaveFileDialog
            {
                Filter = "Excel File (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var file = dialog.FileName;

                using (var package = new ExcelPackage())
                {
                    var configurations = package.Workbook.Worksheets.Add("Configurations");
                    configurations.View.ShowGridLines = false;
                    configurations.Tables.Add(
                        configurations.Cells[1, 1].LoadFromCollection(_configurations, true),
                        string.Empty).TableStyle = TableStyles.Medium10;
                    configurations.Cells.AutoFitColumns();

                    var features = package.Workbook.Worksheets.Add("Features");
                    features.View.ShowGridLines = false;
                    features.Tables.Add(
                        features.Cells[1, 1].LoadFromCollection(_featureFlags, true),
                        String.Empty).TableStyle = TableStyles.Medium10;
                    features.Cells.AutoFitColumns();

                    package.SaveAs(new FileInfo(file));
                }

                Process.Start(file);
            }
        }

        private async void ImportButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel File (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var file = dialog.FileName;

                using (var package = new ExcelPackage(new FileInfo(file)))
                {
                    var configWorksheet = package.Workbook.Worksheets["Configurations"];
                    var conflictConfigurations = new List<Configuration>();
                    if (configWorksheet.Dimension.End.Row > 1)
                    {
                        var configurations = Enumerable.Range(2, configWorksheet.Dimension.End.Row - 1)
                            .Select(i => new Configuration
                            {
                                InstanceName = configWorksheet.Cells[i, 1].Value?.ToString(),
                                ConfigurationKey = configWorksheet.Cells[i, 2].Value?.ToString(),
                                ConfigurationValue = configWorksheet.Cells[i, 3].Value?.ToString()
                            })
                            .ToList();

                        var existingItems =
                            (from dbConfig in _configurations
                                from config in configurations
                                where config.InstanceName == dbConfig.InstanceName
                                      && dbConfig.ConfigurationKey == config.ConfigurationKey
                                      && dbConfig.ConfigurationValue == config.ConfigurationValue
                                select config).ToList();

                        conflictConfigurations.AddRange(
                            from dbConfig in _configurations
                            from config in configurations
                            where config.InstanceName == dbConfig.InstanceName
                                  && dbConfig.ConfigurationKey == config.ConfigurationKey
                                  && dbConfig.ConfigurationValue != config.ConfigurationValue
                            select config);

                        var newItems =
                            configurations
                            .Where(c => !existingItems.Contains(c) && !conflictConfigurations.Contains(c))
                            .ToList();

                        _dataModel.Configurations.AddRange(newItems);
                        _dataModel.SaveChanges();
                    }

                    var featureWorksheet = package.Workbook.Worksheets["Features"];
                    var conflictFeatures = new List<FeatureFlag>();
                    if (featureWorksheet.Dimension.End.Row > 1)
                    {
                        var features = Enumerable.Range(2, featureWorksheet.Dimension.End.Row - 1)
                            .Select(i => new FeatureFlag
                            {
                                InstanceName = featureWorksheet.Cells[i, 1].Value?.ToString(),
                                FlagName = featureWorksheet.Cells[i, 2].Value?.ToString(),
                                FlagValue = bool.Parse(featureWorksheet.Cells[i, 3].Value?.ToString() ?? "false")
                            })
                            .ToList();

                        var existingItems =
                            (from dbFeature in _featureFlags
                                from feature in features
                                where feature.InstanceName == dbFeature.InstanceName
                                      && dbFeature.FlagName == feature.FlagName
                                      && dbFeature.FlagValue == feature.FlagValue
                                select feature).ToList();

                        conflictFeatures.AddRange(
                            from dbFeature in _featureFlags
                            from feature in features
                            where feature.InstanceName == dbFeature.InstanceName
                                  && dbFeature.FlagName == feature.FlagName
                                  && dbFeature.FlagValue != feature.FlagValue
                            select feature);

                        var newItems =
                            features
                            .Where(f => !existingItems.Contains(f) && !conflictFeatures.Contains(f))
                            .ToList();

                        _dataModel.FeatureFlags.AddRange(newItems);
                        _dataModel.SaveChanges();
                    }

                    PopulateDataGrids();

                    // generate excel with conflicting items
                    if (conflictConfigurations.Any() || conflictFeatures.Any())
                    {
                        await this.ShowMessageAsync("Conflicts", "Found conflicts. Save as excel for further analysis.");
                        GenerateConflictExcel(conflictConfigurations, conflictFeatures);

                        conflictConfigurations.ForEach(c =>
                        {
                            _dataModel.Configurations.First(db =>
                                    db.InstanceName == c.InstanceName && db.ConfigurationKey == c.ConfigurationKey)
                                .ConfigurationValue = c.ConfigurationValue;

                            _dataModel.SaveChanges();
                        });

                        conflictFeatures.ForEach(c =>
                        {
                            _dataModel.FeatureFlags.First(db =>
                                    db.InstanceName == c.InstanceName && db.FlagName == c.FlagName)
                                .FlagValue = c.FlagValue;

                            _dataModel.SaveChanges();
                        });
                    }
                }
            }
        }

        private void GenerateConflictExcel(
            IReadOnlyCollection<Configuration> configurations,
            IReadOnlyCollection<FeatureFlag> features)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel File (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var file = dialog.FileName;

                using (var package = new ExcelPackage())
                {
                    if (configurations.Any())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Configurations");

                        var conflicts =
                            from config in configurations
                            from dbConfig in _configurations
                            where config.InstanceName == dbConfig.InstanceName
                                  && config.ConfigurationKey == dbConfig.ConfigurationKey
                            select new
                            {
                                config.InstanceName,
                                config.ConfigurationKey,
                                config.ConfigurationValue,
                                OldValue = dbConfig.ConfigurationValue
                            };

                        worksheet.View.ShowGridLines = false;
                        worksheet.Tables.Add(
                            worksheet.Cells[1, 1].LoadFromCollection(conflicts, true),
                            string.Empty).TableStyle = TableStyles.Medium10;
                        worksheet.Cells.AutoFitColumns();
                    }

                    if (features.Any())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Features");

                        var conflicts =
                            from feature in features
                            from dbFeatute in _featureFlags
                            where feature.InstanceName == dbFeatute.InstanceName
                                  && feature.FlagName == dbFeatute.FlagName
                            select new
                            {
                                feature.InstanceName,
                                feature.FlagName,
                                feature.FlagValue,
                                OldValue = dbFeatute.FlagValue
                            };

                        worksheet.View.ShowGridLines = false;
                        worksheet.Tables.Add(
                            worksheet.Cells[1, 1].LoadFromCollection(conflicts, true),
                            string.Empty).TableStyle = TableStyles.Medium10;
                        worksheet.Cells.AutoFitColumns();
                    }

                    package.SaveAs(new FileInfo(file));
                }
            }
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