using ReactiveUI;
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

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for PermittedWorkstationsWindow.xaml
	/// </summary>
	public partial class PermittedWorkstationsWindow : Window, IViewFor<PermittedWorkstationsWindowViewModel>
    {
        public PermittedWorkstationsWindow()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.WindowTitle, v => v.Title, x => x != null ? x : "");
            this.Bind(ViewModel, vm => vm.ComputerName, v => v.ComputerNameTextBox.Text);
            this.OneWayBind(ViewModel, vm => vm.ComputersView, v => v.ComputersListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.SelectedComputer, v => v.ComputersListView.SelectedItem);

            this.WhenActivated(d =>
            {
                ComputerNameTextBox.Focus();

                d(this.BindCommand(ViewModel, vm => vm.AddComputer, v => v.AddComputerButton));
                d(this.BindCommand(ViewModel, vm => vm.RemoveComputer, v => v.RemoveComputerButton));
                d(this.BindCommand(ViewModel, vm => vm.RemoveAllComputers, v => v.RemoveAllComputersButton));
                d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
                d(ComputersListView.Events()
                    .MouseDoubleClick
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.RemoveComputer));
            });
        }

        public PermittedWorkstationsWindowViewModel ViewModel
        {
            get { return (PermittedWorkstationsWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PermittedWorkstationsWindowViewModel), typeof(PermittedWorkstationsWindow), new PropertyMetadata(new PermittedWorkstationsWindowViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as PermittedWorkstationsWindowViewModel; }
        }
    }
}
