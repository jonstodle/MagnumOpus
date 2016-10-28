using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for DetailsWindow.xaml
	/// </summary>
	public partial class DetailsWindow : Window
	{
		private IDisposable _closeSubscription;
		private IDisposable _onCloseSubscription;
		private CompositeDisposable _subscriptions;

		public DetailsWindow()
		{
			InitializeComponent();

			Observable.Interval(TimeSpan.FromHours(2), Scheduler.TaskPool)
				.ObserveOnDispatcher()
				.Subscribe(x => this.Close())
				.AddTo(_subscriptions);

			this.Events().Closed
				.Subscribe(x =>
				{
					_closeSubscription.Dispose();
					_onCloseSubscription.Dispose();
				})
				.AddTo(_subscriptions);
		}
	}
}
