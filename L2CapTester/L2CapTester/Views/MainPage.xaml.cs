using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace L2CapTester.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            var discovery = ShinyHost.Container.GetRequiredService<IL2CapDiscovery>();
            discovery.StartScanningAsync();
        }
    }
}