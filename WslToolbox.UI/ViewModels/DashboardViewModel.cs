﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using WslToolbox.Core.Commands.Distribution;
using WslToolbox.UI.Core.Commands;
using WslToolbox.UI.Core.Models;
using WslToolbox.UI.Core.Services;
using WslToolbox.UI.Extensions;
using WslToolbox.UI.Views.Modals;

namespace WslToolbox.UI.ViewModels;

public partial class DashboardViewModel : ObservableRecipient
{
    private readonly DistributionService _distributionService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private bool _isRefreshing = true;

    public DashboardViewModel(DistributionService distributionService,
        ILogger<DashboardViewModel> logger)
    {
        _distributionService = distributionService;
        _logger = logger;

        EventHandlers();
    }

    public bool CanRenameDistribution => false;
    public bool CanMoveDistribution => false;

    public OpenUrlCommand OpenUrlCommand { get; } = new();

    public ObservableCollection<Distribution> Distributions { get; set; } = new();

    private void EventHandlers()
    {
        WslToolbox.Core.Commands.Distribution.StartDistributionCommand.DistributionStartFinished += OnReloadExecution;
        TerminateDistributionCommand.DistributionTerminateFinished += OnReloadExecution;
        UnregisterDistributionCommand.DistributionUnregisterFinished += OnReloadExecution;
    }

    private void OnReloadExecution(object? sender, EventArgs e)
    {
        RefreshDistributionsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task StartAllDistribution()
    {
        foreach (var distribution in Distributions)
        {
            await _distributionService.StartDistribution(distribution);
        }
    }

    [RelayCommand]
    private async Task StopAllDistribution()
    {
        foreach (var distribution in Distributions)
        {
            await _distributionService.StopDistribution(distribution);
        }
    }

    [RelayCommand]
    private async Task RestartAllDistribution()
    {
        foreach (var distribution in Distributions)
        {
            await _distributionService.RestartDistribution(distribution);
        }
    }

    [RelayCommand]
    private async Task AddDistribution(Page page)
    {
        var available = await _distributionService.ListInstallableDistributions();
        var distributions = available.Where(x => !x.IsInstalled).ToList();
        try
        {
            var installDistribution = await page.ShowModal<AddDistribution>(
                distributions, "Add distribution", "Add");

            if (installDistribution.Modal == null || installDistribution.ContentDialogResult != ContentDialogResult.Primary)
            {
                return;
            }

            _distributionService.InstallDistribution(installDistribution.Modal.GetSelectedItem<Distribution>());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not start {Message}", e.Message);
        }
    }

    [RelayCommand]
    private async Task RefreshDistributions()
    {
        _logger.LogInformation("Refreshing list of distributions");
        try
        {
            IsRefreshing = true;
            Distributions.Clear();
            (await _distributionService.ListDistributions()).ToList()
                .ForEach(distribution =>
                {
                    Distributions.Add(distribution);
                });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteDistribution(Distribution? distribution)
    {
        await _distributionService.DeleteDistribution(distribution);
    }

    [RelayCommand]
    private async Task RestartDistribution(Distribution? distribution)
    {
        await _distributionService.StopDistribution(distribution);
        await _distributionService.StartDistribution(distribution);
    }

    [RelayCommand]
    private async Task StopDistribution(Distribution? distribution)
    {
        await _distributionService.StopDistribution(distribution);
    }

    [RelayCommand]
    private async Task StartDistribution(Distribution? distribution)
    {
        await _distributionService.StartDistribution(distribution);
    }

    [RelayCommand(CanExecute = nameof(CanRenameDistribution))]
    private async Task RenameDistribution(Distribution? distribution)
    {
    }

    [RelayCommand(CanExecute = nameof(CanMoveDistribution))]
    private async Task MoveDistribution(Distribution? distribution)
    {
    }
}