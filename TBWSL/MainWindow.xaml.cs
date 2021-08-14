﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace WslToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ToolboxLogWindow LogWindow = new();
        private DistributionClass SelectedDistro { get; set; }
        private bool HideDockerDistributions { get; set; } = false;

        private void AddOutput(string output)
        {
            LogWindow.LogBox.Text = LogWindow.LogBox.Text + output + Environment.NewLine;
        }

        private void SetStatus(string status) => StatusBlock.Text = status;

        public MainWindow()
        {
            InitializeComponent();
            PopulateWsl();
            PopulateSelectedDistro();
            SetStatus(String.Empty);
        }

        private async void StatusWsl_Click(object sender, RoutedEventArgs e)
        {
            CommandClass command = await ToolboxClass.StatusWsl();

            PopulateWsl();
        }

        private void ToolboxLog_Click(object sender, RoutedEventArgs e)
        {
            LogWindow.Show();
        }

        private void PopulateWsl()
        {
            SetStatus("Populating...");
            List<DistributionClass> DistroList = ToolboxClass.ListDistributions();

            if (HideDockerDistributions)
            {
                DistroList.RemoveAll(distro => distro.Name == "docker-desktop");
                DistroList.RemoveAll(distro => distro.Name == "docker-desktop-data");
            }

            DistroDetails.ItemsSource = DistroList.FindAll(x => x.isInstalled);
            DefaultDistribution.Content = ToolboxClass.DefaultDistribution().Name;
            SetStatus(String.Empty);
        }

        private void DistroDetails_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedDistro = DistroDetails.SelectedItem is DistributionClass ? (DistributionClass)DistroDetails.SelectedItem : null;
            PopulateSelectedDistro();
        }

        private async void DistroSetDefault_Click(object sender, RoutedEventArgs e)
        {
            CommandClass command = await ToolboxClass.SetDefaultDistribution(SelectedDistro);

            PopulateWsl();
        }

        private async void DistroStop_Click(object sender, RoutedEventArgs e)
        {
            CommandClass command = await ToolboxClass.TerminateDistribution(SelectedDistro);

            PopulateWsl();
        }

        private async void DistroRestart_Click(object sender, RoutedEventArgs e)
        {
            CommandClass commandTerminate = await ToolboxClass.TerminateDistribution(SelectedDistro);
            CommandClass commandStart = await ToolboxClass.StartDistribution(SelectedDistro);

            PopulateWsl();
        }

        private async void DistroStart_Click(object sender, RoutedEventArgs e)
        {
            CommandClass command = await ToolboxClass.StartDistribution(SelectedDistro);

            PopulateWsl();
        }

        private void RefreshWsl_Click(object sender, RoutedEventArgs e)
        {
            PopulateWsl();
        }

        private async void DistroUninstall_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult uninstallMessagebox = MessageBox.Show(
                $"Are you sure you want to uninstall {SelectedDistro.Name}? This will also destroy all data within the distribution.", "Uninstall?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (uninstallMessagebox == MessageBoxResult.Yes)
            {
                CommandClass command = await ToolboxClass.UnregisterDistribution(SelectedDistro);

                PopulateWsl();
            }
        }

        private void PopulateSelectedDistro()
        {
            if (SelectedDistro == null)
            {
                DistroSetDefault.IsEnabled = false;
                DistroStart.IsEnabled = false;
                DistroStop.IsEnabled = false;
                DistroConvert.IsEnabled = false;
                DistroRestart.IsEnabled = false;
                DistroUninstall.IsEnabled = false;
                DistroShell.IsEnabled = false;

                return;
            }

            DistroStart.IsEnabled = SelectedDistro.State != DistributionClass.StateRunning;
            DistroStop.IsEnabled = SelectedDistro.State == DistributionClass.StateRunning;
            DistroRestart.IsEnabled = true;
            DistroConvert.IsEnabled = SelectedDistro.Version != 2;
            DistroUninstall.IsEnabled = true;
            DistroSetDefault.IsEnabled = !SelectedDistro.IsDefault;
            DistroShell.IsEnabled = SelectedDistro.State == DistributionClass.StateRunning;
        }

        private void HideDockerDistro_Checked(object sender, RoutedEventArgs e)
        {
            HideDockerDistributions = HideDockerDistro.IsChecked ?? false;
            PopulateWsl();
        }

        private void HideDockerDistro_Unchecked(object sender, RoutedEventArgs e)
        {
            HideDockerDistributions = HideDockerDistro.IsChecked ?? false;
            PopulateWsl();
        }

        private async void UpdateWsl_Click(object sender, RoutedEventArgs e)
        {
            SetStatus("Checking for WSL Updates...");
            CommandClass command = await ToolboxClass.UpdateWsl();
            string output = Regex.Replace(command.Output, "\t", " ");
            MessageBox.Show(output, "WSL Updater", MessageBoxButton.OK, MessageBoxImage.Information);
            SetStatus(String.Empty);
        }

        private void DistroShell_Click(object sender, RoutedEventArgs e)
        {
            ToolboxClass.ShellDistribution(SelectedDistro);
        }

        private async void StopWsl_Click(object sender, RoutedEventArgs e)
        {
            await ToolboxClass.StopWsl();
        }

        private async void StartWsl_Click(object sender, RoutedEventArgs e)
        {
            await ToolboxClass.StartWsl();
        }

        private void DistroInstall_Click(object sender, RoutedEventArgs e)
        {
            SelectDistroWindow selectDistroWindow = new();

            bool? distroSelected = selectDistroWindow.ShowDialog();

            if ((bool)distroSelected)
            {
                DistributionClass selectedDistro = (DistributionClass)selectDistroWindow.AvailableDistros.SelectedItem;
                ToolboxClass.ShellDistribution(selectedDistro);
            }
        }

        private async void RestartWsl_Click(object sender, RoutedEventArgs e)
        {
            await ToolboxClass.StopWsl();
            await ToolboxClass.StartWsl();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        private async void DistroConvert_Click(object sender, RoutedEventArgs e)
        {
            SetStatus($"Converting {SelectedDistro.Name} to WSL2...");
            CommandClass command = await ToolboxClass.ConvertDistribution(SelectedDistro);
            string output = Regex.Replace(command.Output, "\t", " ");
            MessageBox.Show(output, "Convert", MessageBoxButton.OK, MessageBoxImage.Information);
            SetStatus(String.Empty);
        }
    }
}