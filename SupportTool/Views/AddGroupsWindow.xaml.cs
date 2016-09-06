using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
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
using System.Windows.Shapes;

namespace SupportTool.Views
{
    /// <summary>
    /// Interaction logic for AddGroupsWindow.xaml
    /// </summary>
    public partial class AddGroupsWindow : Window, IViewFor<AddGroupsWindowViewModel>
    {
        public AddGroupsWindow()
        {
            InitializeComponent();

            this.Events()
                .Activated
                .Subscribe(_ => SearchTextBox.Focus());

            this.OneWayBind(ViewModel, vm => vm.WindowTitle, v => v.Title, x => x ?? "");
            this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchTextBox.Text);
            this.OneWayBind(ViewModel, vm => vm.IsSearchingForGroups, v => v.SearchingProgressBar.Visibility);
            this.OneWayBind(ViewModel, vm => vm.IsSearchingForGroups, v => v.SearchingProgressBar.IsIndeterminate);
            this.OneWayBind(ViewModel, vm => vm.SearchResultsView, v => v.SearchResultsListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem);
            this.OneWayBind(ViewModel, vm => vm.GroupsToAddView, v => v.GroupsToAddListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.SelectedGroupToAdd, v => v.GroupsToAddListView.SelectedItem);

            this.WhenActivated(d =>
            {
                d(ViewModel
                    .WhenAnyValue(x => x.SearchQuery)
                    .Throttle(TimeSpan.FromMilliseconds(500))
                    .Where(x => x.HasValue())
                    .Select(x => Unit.Default)
					.ObserveOnDispatcher()
                    .InvokeCommand(ViewModel, x => x.SearchForGroups));
                d(SearchResultsListView.Events()
                    .MouseDoubleClick
                    .Select(x => Unit.Default)
                    .ObserveOnDispatcher()
                    .InvokeCommand(ViewModel, x => x.AddToGroupsToAdd));
                d(this.BindCommand(ViewModel, vm => vm.AddToGroupsToAdd, v => v.AddToGroupsToAddButton));
                d(GroupsToAddListView.Events()
                    .MouseDoubleClick
                    .Select(x => Unit.Default)
                    .ObserveOnDispatcher()
                    .InvokeCommand(ViewModel, x => x.RemoveFromGroupsToAdd));
                d(this.BindCommand(ViewModel, vm => vm.RemoveFromGroupsToAdd, v => v.RemoveFromGroupsToAddButton));
                d(this.BindCommand(ViewModel, vm => vm.AddPrincipalToGroups, v => v.AddPrincipalToGroupsButton));
            });
        }

        public AddGroupsWindowViewModel ViewModel
        {
            get { return (AddGroupsWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(AddGroupsWindowViewModel), typeof(AddGroupsWindow), new PropertyMetadata(new AddGroupsWindowViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as AddGroupsWindowViewModel; }
        }
    }
}
