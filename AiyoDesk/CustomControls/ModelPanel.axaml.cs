using AiyoDesk.AIModels;
using AiyoDesk.CommanandTools;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia.Controls;
using System;

namespace AiyoDesk.CustomControls;

public partial class ModelPanel : UserControl
{
    public RecommandModelItem SourceModel { get; set; } = default!;

    public ModelPanel()
    {
        InitializeComponent();
        ServiceCenter.modelManager.InstalledStateChanged += toggleInstallButton;
    }

    private void toggleInstallButton(object? s, EventArgs e)
    {
        toggleInstallButton();
    }

    private void toggleInstallButton()
    {
        ModelInstall.IsVisible = !SourceModel.IsModelInstalled();
        ModelUninstall.IsVisible = SourceModel.IsModelInstalled();
    }

    private async void ModelInstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SourceModel.IsModelInstalled())
        {
            _ = await MessageDialogHandler.ShowMessageAsync($"{Name} �w�g�w��");
        }
        else
        {
            var confirm = await MessageDialogHandler.ShowConfirmAsync($"�Y�N�}�l�w�� {SourceModel.Name}�A�T�w�����?", "�U���ҫ��T�{");
            if (confirm == null || !confirm.Equals(true)) return;
            var result = await MessageDialogHandler.ShowLicenseAsync(SourceModel);
            if (result == null || !result.Equals(true)) return;
            SourceModel.ModelInstall();
            ServiceCenter.modelManager.LoadInstalledModels();
        }
    }

    private async void ModelUninstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!SourceModel.IsModelInstalled())
        {
            _ = await MessageDialogHandler.ShowMessageAsync($"{Name} �|���w��");
        }
        else
        {
            var confirm = await MessageDialogHandler.ShowConfirmAsync($"�Y�N�}�l���� {SourceModel.Name}�A�T�w�����?", "�U���ҫ��T�{");
            if (confirm == null || !confirm.Equals(true)) return;
            SourceModel.ModelUninstall();
            ServiceCenter.modelManager.LoadInstalledModels();
        }
    }

    private void ModelSource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SourceModel.OfficialUrl)) return;
        CommandLineExecutor.StartProcess(SourceModel.OfficialUrl);
    }
}