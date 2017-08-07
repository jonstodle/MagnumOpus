using ReactiveUI;
using MagnumOpus.Services.ActiveDirectoryServices;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MagnumOpus.Services.SettingsServices;

namespace MagnumOpus.Services.NavigationServices
{
    public class NavigationService
    {
        public static NavigationService Current { get; private set; }



        private NavigationService() { }
        /// <summary>
        /// Needs to be called before use of the NavigationService. Failing to do so will lead to never instantiating the Current property
        /// </summary>
        /// <param name="mainWindow">The MainWindow of the application</param>
        public static void Init(Window mainWindow)
        {
            Current = new NavigationService();
            Current._navigationStack.Add(mainWindow);
        }



        public static void ShowMainWindow() => Current._navigationStack.First().Activate();

        /// <summary>
        /// Displays a windows as a dialog (preventing interaction with other windows while it's open) and passes the parameter object to the view model.
        /// </summary>
        /// <typeparam name="TWindow">The type of window to display</typeparam>
        /// <param name="parameter">An object to be passed to the Opening method in the view model</param>
        /// <returns></returns>
        public async static Task ShowDialog<TWindow>(object parameter = null) where TWindow : Window, IViewFor, new()
        {
            var dialogWindow = new TWindow();

            await (dialogWindow.ViewModel as IDialog)?.Opening(() => dialogWindow.Close(), parameter);

            dialogWindow.ShowDialog();
        }


        /// <summary>
        /// Displays a new window and passes the parameter object to the view model. If a window with the same parameter already exists, that window is activated.
        /// </summary>
        /// <typeparam name="TWindow">The type of window to display</typeparam>
        /// <param name="parameter">An object to be passed to the OnNavigatedTo method in the view model</param>
        /// <returns></returns>
        public async static Task ShowWindow<TWindow>(object parameter = null) where TWindow : Window, IViewFor, new()
        {
            var existingWindow = !SettingsService.Current.OpenDuplicateWindows ? App.Current.Windows.ToGeneric<Window>().Where(window => window is IViewFor).FirstOrDefault(window => window.Tag?.Equals(parameter) ?? false) : null;

            if (existingWindow != null) existingWindow.Activate();
            else
            {
                var newWindow = new TWindow
                {
                    Tag = parameter
                };

                await (newWindow.ViewModel as INavigable)?.OnNavigatedTo(parameter);

                newWindow.Show();
            }
        }

        /// <summary>
        /// Calls ShowWindow with the correct generic parameter and passes in the Name property as the parameter
        /// </summary>
        /// <param name="principal">The principal to open</param>
        /// <returns></returns>
        public static Task ShowPrincipalWindow(Principal principal)
        {
            switch (ActiveDirectoryService.Current.DeterminePrincipalType(principal))
            {
                case PrincipalType.User: return ShowWindow<Views.UserWindow>(principal.Name);
                case PrincipalType.Computer: return ShowWindow<Views.ComputerWindow>(principal.Name);
                case PrincipalType.Group: return ShowWindow<Views.GroupWindow>(principal.Name);
                case PrincipalType.Generic:
                default: return Task.FromResult<object>(null);
            }
        }



        private List<Window> _navigationStack = new List<Window>();
        public IReadOnlyList<Window> NavigationStack => _navigationStack;

        private IViewFor _currentWindow => _navigationStack.LastOrDefault() as IViewFor;
        private IViewFor _previousWindow => _navigationStack.Reverse<Window>().Skip(1).FirstOrDefault() as IViewFor;

        public async Task NavigateTo<TWindow>(object parameter = null) where TWindow : Window, IViewFor, new()
        {
            await (_currentWindow.ViewModel as INavigable)?.OnNavigatingFrom();

            var newWindow = new TWindow();
            await (newWindow.ViewModel as INavigable)?.OnNavigatedTo(parameter);

            _navigationStack.Add(newWindow as Window);
            newWindow.ShowDialog();
        }

        public async Task GoBack(object parameter = null)
        {
            await (_currentWindow.ViewModel as INavigable)?.OnNavigatingFrom();

            await (_previousWindow?.ViewModel as INavigable)?.OnNavigatedTo(parameter);

            (_currentWindow as Window).Close();

            _navigationStack.Remove(_navigationStack.Last());
        }
    }
}
