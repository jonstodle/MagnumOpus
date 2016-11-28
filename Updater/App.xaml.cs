using System;
using System.Windows;
using Updater.Services;

namespace Updater
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			this.Events()
				.Exit
				.Subscribe(async _ =>
				{
					await StateService.Current.Shutdown();
				});
		}
	}
}
