using ReactiveUI;
using MagnumOpus.Controls;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Reactive.Disposables;

namespace MagnumOpus.Views
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : DetailsWindow<UserWindowViewModel>
    {
        public UserWindow()
        {
            InitializeComponent();

            ViewModel = new UserWindowViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.User.Name, v => v.Title, name => name ?? "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetails.User).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User, v => v.UserAccountPanel.User).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User, v => v.UserProfilePanel.User).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User, v => v.UserGroups.User).DisposeWith(d);

                MessageBus.Current.Listen<string>(ApplicationActionRequest.Refresh)
                    .ObserveOnDispatcher()
                    .Where(userCn => userCn == ViewModel.User?.CN)
                    .InvokeCommand(ViewModel.SetUser).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SetUser, v => v.RefreshHyperLink, ViewModel.WhenAnyValue(vm => vm.User.CN)).DisposeWith(d);
                this.Events().KeyDown
                    .Where(args => args.Key == System.Windows.Input.Key.F5)
                    .Select(_ => ViewModel.User.CN)
                    .InvokeCommand(ViewModel.SetUser).DisposeWith(d);
                new List<Interaction<MessageInfo, int>>
                {
                    UserDetails.Messages,
                    UserAccountPanel.Messages,
                    UserProfilePanel.Messages,
                    UserGroups.Messages
                }.RegisterMessageHandler(ContainerGrid).DisposeWith(d);
                new List<Interaction<DialogInfo, Unit>>
                {
                    UserAccountPanel.DialogRequests,
                    UserGroups.DialogRequests
                }.RegisterDialogHandler(ContainerGrid).DisposeWith(d);
            });
        }
    }
}
