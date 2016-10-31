using SupportTool.Services.LogServices;
using SupportTool.Services.SettingsServices;
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
			SettingsService.Init();
		}
    }
}
