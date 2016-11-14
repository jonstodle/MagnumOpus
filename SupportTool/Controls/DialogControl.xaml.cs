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

		public DialogControl(Grid parent, string message, params DialogButtonInfo[] buttons)
		{
			InitializeComponent();

			_parent = parent;
			_buttons = new List<DialogButtonInfo>(buttons);

			SetValue(Grid.RowSpanProperty, _parent.RowDefinitions.Count > 0 ? _parent.RowDefinitions.Count : 1);
			SetValue(Grid.ColumnSpanProperty, _parent.ColumnDefinitions.Count > 0 ? _parent.ColumnDefinitions.Count : 1);

			MessageTextBlock.Text = message;

			foreach (var buttonInfo in _buttons)
			{
				var button = new Button { Content = buttonInfo.Text, Tag = buttonInfo.Id };
				button.Click += HandleButtonClick;
				ButtonStackPanel.Children.Add(button);
			}

			_parent.Children.Add(this);
		}

		public DialogControl(Grid parent, string caption, string message, params DialogButtonInfo[] buttons) : this(parent, message, buttons)
		{
			if (caption.HasValue())
			{
				CaptionTextBlock.Text = caption;
				CaptionTextBlock.Visibility = Visibility.Visible; 
			}
		}

		public DialogControl(Grid parent, string caption, string message, TimeSpan timeout, params DialogButtonInfo[] buttons) : this(parent, caption, message, buttons)
		{
			_timeoutObservable = Observable.Timer(timeout)
				.TakeUntil(_resultSubject);
			_timeoutObservable
				.Subscribe(_ => Close());
		}



		public static DialogControl InfoDialog(Grid parent, string caption, string message) => new DialogControl(parent, caption, message, new DialogButtonInfo("OK"));

		public static DialogControl ErrorDialog(Grid parent, string caption, string message) => new DialogControl(parent, caption, message, new DialogButtonInfo("OK"));



		private void HandleButtonClick(object sender, RoutedEventArgs args)
		{
			if (_resultSubject != null)
			{
				var tag = (sender as Button).Tag as Guid?;
				if (tag == null) return;

				var dbi = _buttons.First(x => x.Id == tag);

				_resultSubject.OnNext(_buttons.IndexOf(dbi));
				_resultSubject.OnCompleted();
				_resultSubject = null; 
			}

			Close();
		}

		private void Close()
		{
			if (_resultSubject != null)
			{
				_resultSubject.OnNext(-1);
				_resultSubject.OnCompleted(); 
			}
			_parent.Children.Remove(this);
		}



		private ISubject<int> _resultSubject = new Subject<int>();
		private Grid _parent;
		private List<DialogButtonInfo> _buttons;
		private IObservable<long> _timeoutObservable;
	}

	public struct DialogButtonInfo
	{
		public Guid Id { get; private set; }
		public string Text { get; private set; }
		public bool CloseDialog { get; private set; }

		public DialogButtonInfo(string text, bool closeDialog = true)
		{
			Id = Guid.NewGuid();
			Text = text;
			CloseDialog = closeDialog;
		}
	}
}
