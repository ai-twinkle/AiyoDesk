using AiyoDesk.AIModels;
using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Linq;
using static AiyoDesk.AppPackages.LlamaCppService;

namespace AiyoDesk.Pages;

public partial class PageMustInstall : UserControl
{
    public MainWindow mainWindow = default!;
    private BackendType hardwareChoose { get; set; } = BackendType.cpu;
    private RecommandModelItem defaultModel { get; set; } = null!;

    public PageMustInstall()
    {
        InitializeComponent();
        defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-4b-it");
    }

    public void CheckInstalledPackage()
    {
        Dispatcher.UIThread.Invoke(() => {
            if (ServiceCenter.llamaCppService.PackageInstalled) chkInstallLlamaCpp.IsChecked = false;
            if (ServiceCenter.condaService.PackageInstalled) chkInstallConda.IsChecked = false;
        });
    }

    public void ClearAllCheck()
    {
        Dispatcher.UIThread.Invoke(() => {
            chkInstallLlamaCpp.IsChecked = false;
            chkInstallConda.IsChecked = false;
        });
    }

    private void OfficialLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(btnLlamaCpp))
        {
            CommandLineExecutor.StartProcess(ServiceCenter.llamaCppService.PackageOfficialUrl);
        }
        else if (sender.Equals(btnConda))
        {
            CommandLineExecutor.StartProcess(ServiceCenter.condaService.PackageOfficialUrl);
        }
    }

    private void rdoModel_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(rdoModelGemma4b))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-4b-it");
        }
        else if (sender.Equals(rdoModelTwinkle3bF1))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "twinkle-ai.Llama-3.2-3B-F1-Instruct");
        }
        else if (sender.Equals(rdoModelGemma12b))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-12b-it");
        }
        else if (sender.Equals(rdoModelGemma1b))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-1b-it");
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
        bool insConda = (chkInstallConda.IsChecked!.Value && !ServiceCenter.condaService.PackageInstalled);
        bool insLlama = (chkInstallLlamaCpp.IsChecked!.Value && (!ServiceCenter.llamaCppService.PackageInstalled || !ServiceCenter.condaService.PackageInstalled));
        bool insModel = (chkInstallAiModel.IsChecked!.Value && !defaultModel.IsModelInstalled());
        bool insOpenWebUI = (chkInstallOpenWebUI.IsChecked!.Value && (!ServiceCenter.openWebUIService.PackageInstalled || !ServiceCenter.condaService.PackageInstalled));

        string confirmMsg = string.Empty;
        if (insConda) confirmMsg += $"�w�� {ServiceCenter.condaService.PackageName}\n";
        if (insLlama) confirmMsg += $"�w�� {ServiceCenter.llamaCppService.PackageName}\n";
        if (insModel) confirmMsg += $"�w�˼ҫ� {defaultModel.Name}\n";
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

        if (insModel || insOpenWebUI)
            await MessageDialogHandler.ShowMessageAsync("�w�˴������z���������p�i��O�ɸ��[�A\n�Ф��n�������n��A�åB�קK�q���i�J��v���A�A\n�H�K�w�˥��ѡC");

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
        if (insModel)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(defaultModel);
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
                if (hardwareChoose == BackendType.cpu)
                {
                    await ServiceCenter.databaseManager.SaveBackendUseGPU(false);
                }
                else
                {
                    await ServiceCenter.databaseManager.SaveBackendUseGPU(true);
                }
                await ServiceCenter.llamaCppService.PackageInstall();
            }
            catch
            {
                resultMsg += $"�w�� {ServiceCenter.llamaCppService.PackageName} �o�Ϳ��~\n";
            }
        }
        if (insModel)
        {
            try
            {
                defaultModel.ModelInstall();
                ServiceCenter.modelManager.LoadInstalledModels();
                var insedModel = ServiceCenter.modelManager.ChatModels.FirstOrDefault(x => x.ModelName == defaultModel.Name);
                if (insedModel != null)
                {
                    ServiceCenter.modelManager.UsingLlmModel = insedModel;
                    SystemSetting systemSetting = ServiceCenter.databaseManager.GetSystemSetting();
                    systemSetting.DefaultModelName = insedModel.ModelName;
                    systemSetting.DefaultModelSubDir = insedModel.SubDir;
                    await ServiceCenter.databaseManager.SaveSystemSetting(systemSetting);
                }
            }
            catch
            {
                resultMsg += $"�w�� {defaultModel.Name} �o�Ϳ��~\n";
            }
        }
        if (insOpenWebUI)
        {
            try
            {
                await ServiceCenter.openWebUIService.PackageInstall();
            }
            catch
            {
                resultMsg += $"�w�� {ServiceCenter.condaService.PackageName} �o�Ϳ��~\n";
            }
        }

        if (string.IsNullOrWhiteSpace(resultMsg))
        {
            await MessageDialogHandler.ShowMessageAsync("�n�D���@�~�w�g���槹���A\n��ĳ�z���������n��᭫�s����C");
            mainWindow.SwitchPage(mainWindow.pagePackages);
        }
        else
        {
            await MessageDialogHandler.ShowMessageAsync($"�@�~�w�g�������i�঳���~�o��\n\n{resultMsg}");
        }

    }

    private async void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var ret = await MessageDialogHandler.ShowConfirmAsync("�U���n��ҰʮɱN���A��ܳo�ӭ���\n\n�T�w�����?");
        if (ret == null || !ret.Equals(true)) return;
        await ServiceCenter.databaseManager.SavePassPackageCheck();
        mainWindow.SwitchPage(mainWindow.pageMain);
    }
}