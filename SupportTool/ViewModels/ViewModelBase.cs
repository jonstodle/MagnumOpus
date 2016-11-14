using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		protected readonly Interaction<MessageInfo, Unit> _infoMessages = new Interaction<MessageInfo, Unit>();
		protected readonly Interaction<MessageInfo, Unit> _errorMessages = new Interaction<MessageInfo, Unit>();



		public Interaction<MessageInfo, Unit> InfoMessages => _infoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => _errorMessages;
	}
}
