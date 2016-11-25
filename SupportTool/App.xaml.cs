using Splat;
using SupportTool.Services.LogServices;
using SupportTool.Services.SettingsServices;
using System.Windows;
using System;

namespace SupportTool
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

			this.Events()
				.Exit
				.Subscribe(_ => SettingsService.Shutdown());
		}
    }
}
