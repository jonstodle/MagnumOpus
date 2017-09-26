using Splat;
using System.Windows;
using System;
using ReactiveUI;
using System.Reactive;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Logging;
using MagnumOpus.Settings;

namespace MagnumOpus
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, IEnableLogger
    {
		public App()
		{
			Locator.CurrentMutable.RegisterLazySingleton(() => new SettingsFacade(), typeof(SettingsFacade));
			Locator.CurrentMutable.RegisterLazySingleton(() => new ADFacade(), typeof(ADFacade));
			Locator.CurrentMutable.RegisterConstant(new FileLoggerService(), typeof(ILogger));
			
			this.Log().Info("Application start");

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(
                error =>
                {
                    this.Log().ErrorException("Application failure", error);
                    Shutdown();
                },
                error =>
                {
                    this.Log().ErrorException("Application failure failure", error);
                    Shutdown();
                },
                () =>
                {
                    this.Log().Info("Exception handler completed");
                    Shutdown();
                });

			this.Events()
				.Exit
				.Subscribe(_ => SettingsFacade.Shutdown());
		}
    }
}
