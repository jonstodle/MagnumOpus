using Akavache;
using SupportTool.Services.LogServices;
using System;
using System.Windows;

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
			BlobCache.ApplicationName = "Magnum Opus";

			this.Events()
				.Exit
				.Subscribe(_ => BlobCache.Shutdown().Wait());
		}
    }
}
