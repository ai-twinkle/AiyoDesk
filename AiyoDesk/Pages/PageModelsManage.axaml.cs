using AiyoDesk.AIModels;
using AiyoDesk.CustomControls;
using AiyoDesk.LocalHost;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AiyoDesk.Pages;

public partial class PageModelsManage : UserControl
{
    public PageModelsManage()
    {
        InitializeComponent();
        loadRecommandModels();
    }

    private void loadRecommandModels()
    {
        foreach(RecommandModelItem rcModel in ServiceCenter.modelManager.RecommandModels)
        {
            ModelPanel modelPanel = new();
            modelPanel.ModelName.Text = rcModel.Name;
            modelPanel.ModelDescription.Text = rcModel.Description;
            modelPanel.lblHardwareNeeded.Text = "�w��ݨD: " + (rcModel.HardwareRequired == HardwareRequiredType.high ? "��" : (rcModel.HardwareRequired == HardwareRequiredType.medium ? "��" : "�C"));
            modelPanel.bdCanVision.IsVisible = rcModel.Vision;
            modelPanel.bdCanTools.IsVisible = rcModel.FunctionCall;
            RecommandContainer.Children.Add(modelPanel);
        }
    }
    

}