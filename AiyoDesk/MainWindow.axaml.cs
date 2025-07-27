using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using AiyoDesk.Pages;
using Avalonia.Controls;
using Avalonia.Threading;
using DialogHostAvalonia;
using Material.Styles.Controls;
using Material.Styles.Models;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Linq;

namespace AiyoDesk;

public partial class MainWindow : Window
{
    public static MainWindow mainWindow { get; private set; } = default!;
    public ServiceCenter serviceCenter { get; internal set; } = default!;
    public PageMain pageMain { get; internal set; } = new();
    public PagePackages pagePackages { get; internal set; } = new();
    public PageModelsManage pageModels { get; internal set; } = new();
    public PageMustInstall pageMustInstall { get; internal set; } = new();
    public PageSettings pageSettings { get; internal set; } = new();
    public PageModules pageModules { get; internal set; } = new();

    private bool _forceClosing = false;

    public MainWindow()
    {
        InitializeComponent();
        mainWindow = this;
        pagePackages.mainWindow = this;
        pageMustInstall.mainWindow = this;
        serviceCenter = new ServiceCenter(() =>
        {
            pagePackages.InitializePackages();
            pageMustInstall.CheckInstalledPackage();
            if (ServiceCenter.databaseManager.GetSystemSetting().PassPackageCheck || 
                (ServiceCenter.condaService.PackageInstalled && ServiceCenter.llamaCppService.PackageInstalled))
            {
                SwitchPage(pageMain);
                _ = pageMain.SystemStartupProcess();
            }
            else
            {
                SwitchPage(pageMustInstall);
            }
        });
        this.Closing += MainWindow_Closing;
        setTrayIcon();
        //contentContrainer.Content = pageMain;
        //showPageName(pageMain);
    }

    void setTrayIcon()
    {
        if (ServiceCenter.databaseManager.GetSystemSetting().MinToSystemTray == false) return;

        var trayIcons = TrayIcon.GetIcons(App.Current!);
        if (trayIcons == null) return;
        var trayIcon = trayIcons.FirstOrDefault();
        if (trayIcon == null || trayIcon.Menu == null || trayIcon.Menu.Count() <= 0) return;
        trayIcon.IsVisible = true;
        foreach (var menuItem in trayIcon.Menu)
        {
            if (menuItem == null || !(menuItem is NativeMenuItem item)) continue;
            if (item.Header == "�}��")
            {
                item.Click += (s, e) => {
                    Dispatcher.UIThread.Invoke(() => { 
                        if (this.WindowState == WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        this.Show(); 
                    });
                };
            }
            else if (item.Header == "�h�X")
            {
                item.Click += (s, e) => {
                    Dispatcher.UIThread.Invoke(() => {
                        if (this.WindowState == WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        this.Close(); 
                    });
                };
            }
        }
    }

    public void SwitchPage(object TargetPage)
    {
        Dispatcher.UIThread.Invoke(() => { 
            contentContrainer.Content = TargetPage;
            showPageName(TargetPage);
        });
    }

    public void showSnackBarMessage(string msg)
    {
        Dispatcher.UIThread.Invoke(() => { 
            SnackbarHost.Post(
                new SnackbarModel(msg, TimeSpan.FromSeconds(8)),
                SnackbarHoster.HostName,
                DispatcherPriority.Normal);
        });
    }

    private void showPageName(object TargetPage)
    {
        if (TargetPage.Equals(pagePackages))
        {
            AppBarTitle.Text = "��X�M��";
            pagePackages.manageButtonState();
        }
        else if (TargetPage.Equals(pageModels))
        {
            AppBarTitle.Text = "�ҫ��޲z";
        }
        else if (TargetPage.Equals(pageMustInstall))
        {
            AppBarTitle.Text = "�w�˥����M��";
        }
        else if (TargetPage.Equals(pageSettings))
        {
            AppBarTitle.Text = "�t�γ]�w";
        }
        else if (TargetPage.Equals(pageMain))
        {
            AppBarTitle.Text = "��T����";
            pageMain.RefreshSystemInfo();
        }
        else if (TargetPage.Equals(pageModules))
        {
            AppBarTitle.Text = "�\��Ҳ�";
            pageMain.RefreshSystemInfo();
        }
    }

    private void NavDrawerSwitch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LeftDrawer.LeftDrawerOpened = !(LeftDrawer.LeftDrawerOpened);
    }

    private void btnPackages_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pagePackages);
    }

    private void btnModels_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageModels);
    }

    private void btnSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageSettings);
    }

    private void btnMain_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageMain);
    }

    private void btnModules_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageModules);
    }

    private async void MainWindow_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        if (_forceClosing) return;
        e.Cancel = true;

        if (ServiceCenter.hostedHttpService.PackageRunning || 
            ServiceCenter.llamaCppService.PackageRunning || 
            ServiceCenter.openWebUIService.PackageRunning || 
            ServiceCenter.comfyUIService.PackageRunning)
        {
            var result = await MessageDialogHandler.ShowConfirmAsync("�����I���A�ȥ��b�B�椤�A�������n��N�P�������o�ǪA�ȡC\n�����A�ȥi��ݭn�@�Ǯɶ��A�Ф��n�j������n��H�K�귽�L�k��������C\n\n�T�w�n�ߧY�������n���?");
            if (result == null || !result.Equals(true))
            {
                e.Cancel = true;
                return;
            }
            this.IsEnabled = false;
            await serviceCenter.Disposing();
        }

        _forceClosing = true;
        this.Close();
    }
}

