using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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
	/// Interaction logic for DelimitedListInput.xaml
	/// </summary>
	public partial class DelimitedListInput : UserControl
	{
		public DelimitedListInput(Grid parent)
		{
			InitializeComponent();

			_parent = parent;

			SetValue(Grid.RowSpanProperty, _parent.RowDefinitions.Count > 0 ? _parent.RowDefinitions.Count : 1);
			SetValue(Grid.ColumnSpanProperty, _parent.ColumnDefinitions.Count > 0 ? _parent.ColumnDefinitions.Count : 1);

			Observable.FromEventPattern<TextChangedEventArgs>(ItemsTextBox, nameof(TextBox.TextChanged))
				.Select(x => ItemsTextBox.Text.HasValue(3))
				.Subscribe(x => OkButton.IsEnabled = x)
				.AddTo(_subscribtions);

			Observable.FromEventPattern(OkButton, nameof(Button.Click))
				.Subscribe(ev =>
				{
					var items = ItemsTextBox.Text.Split(new char[] { ';', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var item in items) _resultSubject.OnNext(item);
					_resultSubject.OnCompleted();
					_resultSubject = null;
					Close();
				})
				.AddTo(_subscribtions);

			Observable.FromEventPattern(CancelButton, nameof(Button.Click))
				.Subscribe(ev => Close())
				.AddTo(_subscribtions);
		}



		private void Close()
		{
			if (_resultSubject != null)
			{
				_resultSubject.OnNext(null);
				_resultSubject.OnCompleted();
			}

			_subscribtions.Dispose();
			_parent.Children.Remove(this);
		}



		private Grid _parent;
		private Subject<string> _resultSubject = new Subject<string>();
		private CompositeDisposable _subscribtions = new CompositeDisposable();
	}
}
