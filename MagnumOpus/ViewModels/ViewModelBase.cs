using ReactiveUI;
using MagnumOpus.Models;
using System.Reactive;

namespace MagnumOpus.ViewModels
{
    public class ViewModelBase : ReactiveObject, ISupportsActivation
	{
        public ViewModelActivator Activator => _activator;

		public Interaction<MessageInfo, int> Messages => _messages;

		public Interaction<DialogInfo, Unit> DialogRequests => _dialogRequests;



        protected readonly ViewModelActivator _activator = new ViewModelActivator();
        protected readonly Interaction<MessageInfo, int> _messages = new Interaction<MessageInfo, int>();
        protected readonly Interaction<DialogInfo, Unit> _dialogRequests = new Interaction<DialogInfo, Unit>();
    }
}
