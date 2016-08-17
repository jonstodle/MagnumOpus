﻿using ReactiveUI;
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



        private List<Window> navigationStack = new List<Window>();
        public IReadOnlyList<Window> NavigationStack => navigationStack;

        private IViewFor currentWindow => navigationStack.LastOrDefault() as IViewFor;
        private IViewFor previousWindow => navigationStack.Reverse<Window>().Skip(1).FirstOrDefault() as IViewFor;

        public async Task NavigateTo<TWindow>(object parameter = null) where TWindow : Window, IViewFor, new()
        {
            await (currentWindow.ViewModel as INavigable)?.OnNavigatingFrom();

            var newWindow = new TWindow();
            await (newWindow.ViewModel as INavigable)?.OnNavigatedTo(parameter);

            newWindow.ShowDialog();
            navigationStack.Add(newWindow as Window);
        }

        public async Task GoBack(object parameter = null)
        {
            await (currentWindow.ViewModel as INavigable)?.OnNavigatingFrom();

            await (previousWindow?.ViewModel as INavigable)?.OnNavigatedTo(parameter);

            (currentWindow as Window).Close();

            navigationStack.Remove(navigationStack.Last());
        }
    }
}
