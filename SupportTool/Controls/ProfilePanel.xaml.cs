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

            this.Bind(ViewModel, vm => vm.IsShowingResetProfile, v => v.ResetProfileToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingResetProfile, v => v.ResetProfileGrid.Visibility);
            this.Bind(ViewModel, vm => vm.ComputerName, v => v.ResetProfileComputerNameTextBox.Text);
            this.OneWayBind(ViewModel, vm => vm.ResetMessages, v => v.ResetProfileListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.IsShowingRestoreProfile, v => v.RestoreProfileToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingRestoreProfile, v => v.RestoreProfileStackPanel.Visibility);
            this.Bind(ViewModel, vm => vm.ComputerName, v => v.RestoreProfileComputerNameTextBox.Text);
            this.OneWayBind(ViewModel, vm => vm.Profiles, v => v.RestoreProfileListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.SelectedProfileIndex, v => v.RestoreProfileListView.SelectedIndex);
            this.Bind(ViewModel, vm => vm.ShouldRestoreDesktopItems, v => v.DesktopItemsCheckBox.IsChecked);
            this.Bind(ViewModel, vm => vm.ShouldRestoreInternetExplorerFavorites, v => v.InternetExplorerFavoritesCheckBox.IsChecked);
            this.Bind(ViewModel, vm => vm.ShouldRestoreOutlookSignatures, v => v.OutlookSignaturesCheckBox.IsChecked);
            this.Bind(ViewModel, vm => vm.ShouldRestoreWindowsExplorerFavorites, v => v.WindowsExplorerFavoritesCheckBox.IsChecked);
			this.Bind(ViewModel, vm => vm.ShouldRestoreStickyNotes, v => v.StickyNotesCheckBox.IsChecked);

			this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.ResetGlobalProfile, v => v.ResetGlobalProfileButton));
                d(this.BindCommand(ViewModel, vm => vm.ResetLocalProfile, v => v.ResetLocalProfileButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenGlobalProfileDirectory, v => v.OpenGlobalProfileDirectoryButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenLocalProfileDirectory, v => v.OpenLocalProfileDirectoryButton));
                d(this.BindCommand(ViewModel, vm => vm.SearchForProfiles, v => v.SearchButton));
                d(this.BindCommand(ViewModel, vm => vm.RestoreProfile, v => v.RestoreProfileButton));
				d(this.BindCommand(ViewModel, vm => vm.ResetCitrixProfile, v => v.ResetCitrixProfileButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenGlobalProfile, v => v.OpenGlobalProfileButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenHomeFolder, v => v.OpenHomeFolderButton));
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

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(ProfilePanel), new PropertyMetadata(null, (d,e) => (d as ProfilePanel).ViewModel.User = e.NewValue as UserObject));

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
