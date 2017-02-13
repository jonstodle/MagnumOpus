using Splat;
using MagnumOpus.Services.LogServices;
using MagnumOpus.Services.SettingsServices;
using System.Windows;
using System;

namespace MagnumOpus
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, IEnableLogger
    {
		public App()
		{
			Locator.CurrentMutable.RegisterConstant(new FileLoggerService(), typeof(ILogger));
			this.Log().Info("Application start");
			SettingsService.Init();

			this.Events()
				.Exit
				.Subscribe(_ => SettingsService.Shutdown());
		}
    }
}
