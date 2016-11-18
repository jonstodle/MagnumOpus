using ReactiveUI;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
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
