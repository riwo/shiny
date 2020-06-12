using System.Reactive.Concurrency;
using System.Threading;
using Xamarin.Forms;
using L2CapTester.Services;
using L2CapTester.Views;

namespace L2CapTester
{
    public partial class App : Application
    {
        public static readonly SynchronizationContext MainSynchronizationContext;
        public static readonly IScheduler MainThreadScheduler;

        static App()
        {
            var mainSynchronizationContext = SynchronizationContext.Current;
            MainSynchronizationContext = mainSynchronizationContext;
            if (mainSynchronizationContext != null)
                MainThreadScheduler = new SynchronizationContextScheduler(mainSynchronizationContext);
        }
        
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
