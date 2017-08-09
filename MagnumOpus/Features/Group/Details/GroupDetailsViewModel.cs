using ReactiveUI;

namespace MagnumOpus.Group
{
	public class GroupDetailsViewModel : ViewModelBase
	{
        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }



        private GroupObject _group;
    }
}
