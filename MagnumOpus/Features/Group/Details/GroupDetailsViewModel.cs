using ReactiveUI;
using MagnumOpus.Models;

namespace MagnumOpus.ViewModels
{
	public class GroupDetailsViewModel : ViewModelBase
	{
        public GroupDetailsViewModel()
        {

        }



        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }



        private GroupObject _group;
    }
}
