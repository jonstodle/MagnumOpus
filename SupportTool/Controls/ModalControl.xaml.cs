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
		public ModalControl(object parameters = null)
		{
			InitializeComponent();

			_parameters = parameters;
		}



		public object DisplayContent
		{
			get { return (object)GetValue(DisplayContentProperty); }
			set { SetValue(DisplayContentProperty, value); }
		}

		public static readonly DependencyProperty DisplayContentProperty = DependencyProperty.Register(nameof(DisplayContent), typeof(object), typeof(ModalControl), new PropertyMetadata(null, (d, e) => (d as ModalControl).PassParameters()));



		private void PassParameters()
		{

		}



		object _parameters;
	}
}
