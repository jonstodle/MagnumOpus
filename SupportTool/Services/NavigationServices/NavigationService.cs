using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.Services.NavigationServices
{
	public class NavigationService
	{
		public static NavigationService Current { get; private set; }



		private NavigationService() { }
		public static void Init(Window mainWindow)
		{
			Current = new NavigationService();
			Current.navigationStack.Add(mainWindow);
		}



		public async static Task ShowDialog<TWindow>(object parameter = null) where TWindow : Window, IViewFor, new()
		{
			var dialogWindow = new TWindow();

			await (dialogWindow.ViewModel as IDialog)?.Opening(() => dialogWindow.Close(), parameter);

			dialogWindow.ShowDialog();
		}

		public async static Task<TResult> ShowDialog<TWindow, TResult>(object parameter = null) where TWindow : Window, IViewFor, new()
		{
			var dialogWindow = new TWindow();
			TResult resultValue = default(TResult);

			await (dialogWindow.ViewModel as IDialog<TResult>)?.Opening(
				(TResult result) =>
				{
					resultValue = result;
					dialogWindow.Close();
				}, parameter);

			dialogWindow.ShowDialog();

			return resultValue;
		}



		private List<Window> navigationStack = new List<Window>();
		public IReadOnlyList<Window> NavigationStack => navigationStack;

		private IViewFor currentWindow => navigationStack.LastOrDefault() as IViewFor;
		private IViewFor previousWindow => navigationStack.Reverse<Window>().Skip(1).FirstOrDefault() as IViewFor;

		public async Task NavigateTo<TWindow>(object parameter = null) where TWindow : Window, IViewFor, new()
		{
			await (currentWindow.ViewModel as IDialog)?.OnNavigatingFrom();

			var newWindow = new TWindow();
			await (newWindow.ViewModel as IDialog)?.OnNavigatedTo(parameter);

			navigationStack.Add(newWindow as Window);
			newWindow.ShowDialog();
		}

		public async Task GoBack(object parameter = null)
		{
			await (currentWindow.ViewModel as IDialog)?.OnNavigatingFrom();

			await (previousWindow?.ViewModel as IDialog)?.OnNavigatedTo(parameter);

			(currentWindow as Window).Close();

			navigationStack.Remove(navigationStack.Last());
		}
	}
}
