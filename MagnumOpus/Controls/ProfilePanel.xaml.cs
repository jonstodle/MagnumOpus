using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Reactive.Disposables;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for ProfilePanel.xaml
    /// </summary>
    public partial class ProfilePanel : UserControl, IViewFor<ProfilePanelViewModel>
    {
        public ProfilePanel()
        {
            InitializeComponent();

            ViewModel = new ProfilePanelViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.User, v => v.User).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingResetProfile, v => v.ResetProfileToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingResetProfile, v => v.ResetProfileGrid.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ComputerName, v => v.ResetProfileComputerNameTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsExecutingResetGlobalProfile, v => v.ResetGlobalProfileButton.Content, x => x ? "Resetting global profile..." : "Reset global profile").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsExecutingResetLocalProfile, v => v.ResetLocalProfileButton.Content, x => x ? "Resetting local profile..." : "Reset local profile").DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsShowingRestoreProfile, v => v.RestoreProfileToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingRestoreProfile, v => v.RestoreProfileStackPanel.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ComputerName, v => v.RestoreProfileComputerNameTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Profiles, v => v.RestoreProfileListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedProfileIndex, v => v.RestoreProfileListView.SelectedIndex).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShouldRestoreDesktopItems, v => v.DesktopItemsCheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShouldRestoreInternetExplorerFavorites, v => v.InternetExplorerFavoritesCheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShouldRestoreOutlookSignatures, v => v.OutlookSignaturesCheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShouldRestoreWindowsExplorerFavorites, v => v.WindowsExplorerFavoritesCheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShouldRestoreStickyNotes, v => v.StickyNotesCheckBox.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsExecutingRestoreProfile, v => v.RestoreProfileButton.Content, x=> x ? "Restoring profile..." : "Restore profile").DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsShowingGlobalProfile, v => v.GlobalProfileToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingGlobalProfile, v => v.GlobalProfileStackPanel.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.GlobalProfilePath, v => v.GlobalProfilePathTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HasGlobalProfilePathChanged, v => v.GlobalProfilePathButtonsStackPanel.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsShowingHomeFolder, v => v.HomeFolderToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingHomeFolder, v => v.HomeFolderStackPanel.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.HomeFolderPath, v => v.HomeFolderPathTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HasHomeFolderPathChanged, v => v.HomeFolderPathButtonsStackPanel.Visibility).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.ResetGlobalProfile, v => v.ResetGlobalProfileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ResetLocalProfile, v => v.ResetLocalProfileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SearchForProfiles, v => v.SearchButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RestoreProfile, v => v.RestoreProfileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ResetCitrixProfile, v => v.ResetCitrixProfileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenGlobalProfile, v => v.OpenGlobalProfileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveGlobalProfilePath, v => v.GlobalProfilePathSaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.CancelGlobalProfilePath, v => v.GlobalProfilePathCancelButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenHomeFolder, v => v.OpenHomeFolderButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveHomeFolderPath, v => v.HomeFolderPathSaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.CancelHomeFolderPath, v => v.HomeFolderPathCancelButton).DisposeWith(d);
                ResetProfileComputerNameTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .ToSignal()
                    .InvokeCommand(ViewModel.ResetLocalProfile).DisposeWith(d);
                RestoreProfileComputerNameTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .ToSignal()
                    .InvokeCommand(ViewModel.SearchForProfiles).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public UserObject User { get => (UserObject)GetValue(UserProperty); set => SetValue(UserProperty, value); }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(ProfilePanel), new PropertyMetadata(null));

        public ProfilePanelViewModel ViewModel { get => (ProfilePanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ProfilePanelViewModel), typeof(ProfilePanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as ProfilePanelViewModel; }
    }
}
