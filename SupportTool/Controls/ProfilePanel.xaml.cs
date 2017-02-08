using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
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

namespace SupportTool.Controls
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
                d(this.Bind(ViewModel, vm => vm.User, v => v.User));

                d(this.Bind(ViewModel, vm => vm.IsShowingResetProfile, v => v.ResetProfileToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingResetProfile, v => v.ResetProfileGrid.Visibility));
                d(this.Bind(ViewModel, vm => vm.ComputerName, v => v.ResetProfileComputerNameTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.IsExecutingResetGlobalProfile, v => v.ResetGlobalProfileButton.Content, x => x ? "Resetting global profile..." : "Reset global profile"));
                d(this.OneWayBind(ViewModel, vm => vm.IsExecutingResetLocalProfile, v => v.ResetLocalProfileButton.Content, x => x ? "Resetting local profile..." : "Reset local profile"));
                d(this.Bind(ViewModel, vm => vm.IsShowingRestoreProfile, v => v.RestoreProfileToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingRestoreProfile, v => v.RestoreProfileStackPanel.Visibility));
                d(this.Bind(ViewModel, vm => vm.ComputerName, v => v.RestoreProfileComputerNameTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Profiles, v => v.RestoreProfileListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedProfileIndex, v => v.RestoreProfileListView.SelectedIndex));
                d(this.Bind(ViewModel, vm => vm.ShouldRestoreDesktopItems, v => v.DesktopItemsCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.ShouldRestoreInternetExplorerFavorites, v => v.InternetExplorerFavoritesCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.ShouldRestoreOutlookSignatures, v => v.OutlookSignaturesCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.ShouldRestoreWindowsExplorerFavorites, v => v.WindowsExplorerFavoritesCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.ShouldRestoreStickyNotes, v => v.StickyNotesCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.IsShowingGlobalProfile, v => v.GlobalProfileToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingGlobalProfile, v => v.GlobalProfileStackPanel.Visibility));
                d(this.Bind(ViewModel, vm => vm.GlobalProfilePath, v => v.GlobalProfilePathTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.HasGlobalProfilePathChanged, v => v.GlobalProfilePathButtonsStackPanel.Visibility));
                d(this.Bind(ViewModel, vm => vm.IsShowingHomeFolder, v => v.HomeFolderToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingHomeFolder, v => v.HomeFolderStackPanel.Visibility));
                d(this.Bind(ViewModel, vm => vm.HomeFolderPath, v => v.HomeFolderPathTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.HasHomeFolderPathChanged, v => v.HomeFolderPathButtonsStackPanel.Visibility));

                d(this.BindCommand(ViewModel, vm => vm.ResetGlobalProfile, v => v.ResetGlobalProfileButton));
                d(this.BindCommand(ViewModel, vm => vm.ResetLocalProfile, v => v.ResetLocalProfileButton));
                d(this.BindCommand(ViewModel, vm => vm.SearchForProfiles, v => v.SearchButton));
                d(this.BindCommand(ViewModel, vm => vm.RestoreProfile, v => v.RestoreProfileButton));
				d(this.BindCommand(ViewModel, vm => vm.ResetCitrixProfile, v => v.ResetCitrixProfileButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenGlobalProfile, v => v.OpenGlobalProfileButton));
                d(this.BindCommand(ViewModel, vm => vm.SaveGlobalProfilePath, v => v.GlobalProfilePathSaveButton));
                d(this.BindCommand(ViewModel, vm => vm.CancelGlobalProfilePath, v => v.GlobalProfilePathCancelButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenHomeFolder, v => v.OpenHomeFolderButton));
                d(this.BindCommand(ViewModel, vm => vm.SaveHomeFolderPath, v => v.HomeFolderPathSaveButton));
                d(this.BindCommand(ViewModel, vm => vm.CancelHomeFolderPath, v => v.HomeFolderPathCancelButton));
                d(ResetProfileComputerNameTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.ResetLocalProfile));
                d(RestoreProfileComputerNameTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.SearchForProfiles));
            });
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public UserObject User
        {
            get { return (UserObject)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(ProfilePanel), new PropertyMetadata(null));

        public ProfilePanelViewModel ViewModel
        {
            get { return (ProfilePanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ProfilePanelViewModel), typeof(ProfilePanel), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ProfilePanelViewModel; }
        }
    }
}
