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
			SettingsService.Init();
			Locator.CurrentMutable.RegisterConstant(new FileLoggerService(), typeof(ILogger));
			this.Log().Info("Application start");

			this.Events()
				.Exit
				.Subscribe(_ => SettingsService.Shutdown());
		}
    }
}
