using ReactiveUI;
using SupportTool.Services.NavigationServices;
using System.Windows.Controls;

namespace SupportTool.Controls
{
	/// <summary>
	/// Interaction logic for ModalControl.xaml
	/// </summary>
	public partial class ModalControl : UserControl
	{
		public ModalControl(Grid parent, object content, object parameter = null)
		{
			InitializeComponent();

			_parent = parent;

			SetValue(Grid.RowSpanProperty, _parent.RowDefinitions.Count > 0 ? _parent.RowDefinitions.Count : 1);
			SetValue(Grid.ColumnSpanProperty, _parent.ColumnDefinitions.Count > 0 ? _parent.ColumnDefinitions.Count : 1);

			var view = content as IViewFor;
			var vm = view?.ViewModel as IDialog;
			if (vm != null) vm.Opening(Close, parameter);

			ContentPresenter.Content = content;

			_parent.Children.Add(this);
		}

		private void Close()
		{
			_parent.Children.Remove(this);
		}



		private Grid _parent;
	}
}
