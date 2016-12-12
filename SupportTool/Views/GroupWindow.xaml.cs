using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for GroupWindow.xaml
	/// </summary>
	public partial class GroupWindow : DetailsWindow, IViewFor<GroupWindowViewModel>
	{
		public GroupWindow()
		{
			InitializeComponent();

			ViewModel = new GroupWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Group.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetails.Group);
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupGroups.Group);
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupNotes.Group);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.SetGroup, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(x => x.Group.CN)));
			d(new List<Interaction<MessageInfo, Unit>>
				{
					GroupDetails.InfoMessages,
					GroupGroups.InfoMessages,
					GroupNotes.InfoMessages
				}.RegisterInfoHandler(ContainerGrid));
				d(new List<Interaction<MessageInfo, Unit>>
				{
					GroupDetails.ErrorMessages,
					GroupGroups.ErrorMessages,
					GroupNotes.ErrorMessages
				}.RegisterErrorHandler(ContainerGrid));
				d(GroupGroups
					.DialogRequests
					.RegisterDialogHandler(ContainerGrid));
			});
		}

		public GroupWindowViewModel ViewModel
		{
			get { return (GroupWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupWindowViewModel), typeof(GroupWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as GroupWindowViewModel; }
		}
	}
}
