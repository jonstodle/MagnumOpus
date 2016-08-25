using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SupportTool.Controls
{
    /// <summary>
    /// Interaction logic for UserGroups.xaml
    /// </summary>
    public partial class UserGroups : UserControl, IViewFor<UserGroupsViewModel>
    {
        public UserGroups()
        {
            InitializeComponent();

            this.Bind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsGrid.Visibility);
            this.Bind(ViewModel, vm => vm.IsShowingAllGroups, v => v.AllGroupsToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingAllGroups, v => v.AllGroupsGrid.Visibility);
            this.OneWayBind(ViewModel, vm => vm.DirectGroupsCollectionView, v => v.DirectGroupsListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.GroupFilter, v => v.GroupFilterTextBox.Text);
            this.Bind(ViewModel, vm => vm.UseFuzzy, v => v.UseFuzzyToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.AllGroupsCollectionView, v => v.AllGroupsListView.ItemsSource);
            this.OneWayBind(ViewModel, vm => vm.IsLoadingGroups, v => v.IsLoadingGroupsProgressBar.Visibility);
            this.OneWayBind(ViewModel, vm => vm.IsLoadingGroups, v => v.IsLoadingGroupsProgressBar.IsIndeterminate);
            this.OneWayBind(ViewModel, vm => vm.AllGroupsCollectionView.Count, v => v.ShowingCountRun.Text);
            this.OneWayBind(ViewModel, vm => vm.AllGroups.Count, v => v.TotalCountRun.Text);

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OpenAddGroups, v => v.AddGroupsButton));
            });

            ViewModel
                .WhenAnyValue(x => x.IsShowingAllGroups)
                .Where(x => x)
                .Select(_ => Unit.Default)
                .InvokeCommand(ViewModel, x => x.GetAllGroups);
        }

        public IObservable<Message> Messages => ViewModel.Messages;

        public UserObject User
        {
            get { return (UserObject)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(UserGroups), new PropertyMetadata(null, (d,e)=> (d as UserGroups).ViewModel.User = e.NewValue as UserObject));

        public UserGroupsViewModel ViewModel
        {
            get { return (UserGroupsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserGroupsViewModel), typeof(UserGroups), new PropertyMetadata(new UserGroupsViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as UserGroupsViewModel; }
        }
    }
}
