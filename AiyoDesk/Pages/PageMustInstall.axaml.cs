using AiyoDesk.CommanandTools;
using AiyoDesk.CustomControls;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DialogHostAvalonia;
using System.Threading.Tasks;
using static AiyoDesk.AppPackages.LlamaCppService;

namespace AiyoDesk.Pages;

public partial class PageMustInstall : UserControl
{
    public MainWindow mainWindow = default!;
    private BackendType hardwareChoose { get; set; } = BackendType.cpu;

    public PageMustInstall()
    {
        InitializeComponent();
    }

    public void ClearAllCheck()
    {
        Dispatcher.UIThread.Invoke(() => {
            chkInstallLlamaCpp.IsChecked = false;
            chkInstallConda.IsChecked = false;
            chkInstallGemma.IsChecked = false;
            chkInstallOpenWebUI.IsChecked = false;
        });
    }

    private void OfficialLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(btnLlamaCpp))
        {
            CommandLineExecutor.StartProcess(@"https://github.com/ggml-org/llama.cpp");
        }
        else if (sender.Equals(btnConda))
        {
            CommandLineExecutor.StartProcess(@"https://conda-forge.org");
        }
        else if (sender.Equals(btnGemma))
        {
            CommandLineExecutor.StartProcess(@"https://deepmind.google/models/gemma");
        }
        else if (sender.Equals(btnOpenWebUI))
        {
            CommandLineExecutor.StartProcess(@"https://docs.openwebui.com/");
        }
    }

    private void rdoHardware_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(rdoHardwareCPU))
        {
            hardwareChoose = BackendType.cpu;
        }
        else if (sender.Equals(rdoHardwareCuda))
        {
            hardwareChoose = BackendType.cuda;
        }
        else if (sender.Equals(rdoHardwareHip))
        {
            hardwareChoose = BackendType.hip;
        }
        else if (sender.Equals(rdoHardwareIntel))
        {
            hardwareChoose = BackendType.sycl;
        }
    }

    private async void btnStart_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        bool insConda = (chkInstallConda.IsChecked!.Value);
        bool insLlama = (chkInstallLlamaCpp.IsChecked!.Value);
        bool insGemma = (chkInstallGemma.IsChecked!.Value);
        bool insOpenWebUI = (chkInstallOpenWebUI.IsChecked!.Value);

        string confirmMsg = string.Empty;
        if (insConda) confirmMsg += $"�w�� {ServiceCenter.condaService.PackageName}\n";
        if (insLlama) confirmMsg += $"�w�� {ServiceCenter.llamaCppService.PackageName}\n";
        if (insOpenWebUI) confirmMsg += $"�w�� {ServiceCenter.openWebUIService.PackageName}\n";

        if (string.IsNullOrWhiteSpace(confirmMsg))
        {
            await MessageDialogHandler.ShowMessageAsync("�S���ݭn�w�˪��M��");
            return;
        }
        else
        {
            confirmMsg = $"�Y�N�i��H�U�@�~\n\n" + confirmMsg + "\n�T�w�����?";
        }
        var ret = await MessageDialogHandler.ShowConfirmAsync(confirmMsg, "�w�˴���");
        if (ret == null || !ret.Equals(true)) return;

        if (insConda)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(ServiceCenter.condaService);
            if (!result!.Equals(true)) return;
        }
        if (insLlama)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(ServiceCenter.llamaCppService);
            if (!result!.Equals(true)) return;
        }
        if (insOpenWebUI)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(ServiceCenter.openWebUIService);
            if (!result!.Equals(true)) return;
        }

        string resultMsg = string.Empty;
        if (insConda)
        {
            try
            {
                await ServiceCenter.condaService.PackageInstall();
            }
            catch
            {
                resultMsg += $"�w�� {ServiceCenter.condaService.PackageName} �o�Ϳ��~\n";
            }
        }
        if (insLlama)
        {
            try
            {
                ServiceCenter.llamaCppService.UsingBackend = hardwareChoose;
                await ServiceCenter.llamaCppService.PackageInstall();
            }
            catch
            {
                resultMsg += $"�w�� {ServiceCenter.llamaCppService.PackageName} �o�Ϳ��~\n";
            }
        }
        if (insOpenWebUI)
        {
            try
            {
                _ = ServiceCenter.openWebUIService.PackageInstall();
            }
            catch
            {
                resultMsg += $"�w�� {ServiceCenter.openWebUIService.PackageName} �o�Ϳ��~\n";
            }
        }

        if (string.IsNullOrWhiteSpace(resultMsg))
        {
            await MessageDialogHandler.ShowMessageAsync("�n�D���@�~�w�g���槹��");
        }
        else
        {
            await MessageDialogHandler.ShowMessageAsync($"�@�~�w�g�������i�঳���~�o��\n\n{resultMsg}");
        }

    }

    private async void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var ret = await MessageDialogHandler.ShowMessageAsync("���հT�����e", "���հT�����D");
        await Task.Delay(1);
    }
}