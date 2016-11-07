using SupportTool.Services.LogServices;
using SupportTool.Services.SettingsServices;
using System.Windows;
using System;
using Akavache;

namespace SupportTool
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
		public App()
		{
			LogService.Info("Application start");
			SettingsService.Init();

			this.Events()
				.Exit
				.Subscribe(_ => BlobCache.Shutdown().Wait());
		}
    }
}
