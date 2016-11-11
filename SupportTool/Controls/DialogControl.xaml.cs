using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
	/// Interaction logic for DialogControl.xaml
	/// </summary>
	public partial class DialogControl : UserControl
	{
		public IObservable<int> Result => _resultSubject;

		private DialogControl()
		{
			InitializeComponent();
		}

		public DialogControl(Grid parent, string text, params string[] buttonTitles)
		{
			InitializeComponent();

			SetValue(Grid.RowSpanProperty, parent.RowDefinitions.Count > 0 ? parent.RowDefinitions.Count : 1);
			SetValue(Grid.ColumnSpanProperty, parent.ColumnDefinitions.Count > 0 ? parent.ColumnDefinitions.Count : 1);

			MessageTextBlock.Text = text;

			_buttonTitles = buttonTitles;
			foreach (var buttonTitle in _buttonTitles)
			{
				var button = new Button { Content = buttonTitle };
				button.Click += HandleButtonClick;
				ButtonStackPanel.Children.Add(button);
			}

			parent.Children.Add(this);
		public DialogControl(Grid parent, string caption, string message, IEnumerable<string> buttons) : this(parent, message, buttons)
		{
			CaptionTextBlock.Text = caption;
			CaptionTextBlock.Visibility = Visibility.Visible;
		}



		private void HandleButtonClick(object sender, RoutedEventArgs args)
		{
			if (_resultSubject == null) return;

			var content = (sender as Button).Content as string;
			if (content == null) return;

			_resultSubject.OnNext(Array.IndexOf(_buttonTitles, content));
			_resultSubject.OnCompleted();
			_resultSubject = null;
			
			//TODO: Close dialog
		}



		private ISubject<int> _resultSubject = new Subject<int>();
		private string[] _buttonTitles;
	}
}
