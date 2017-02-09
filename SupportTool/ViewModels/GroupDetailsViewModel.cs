using ReactiveUI;
using SupportTool.Models;

namespace SupportTool.ViewModels
{
	public class GroupDetailsViewModel : ViewModelBase
	{
        public GroupDetailsViewModel()
        {

        }



        public GroupObject Group
        {
            get { return _group; }
            set { this.RaiseAndSetIfChanged(ref _group, value); }
        }



        private GroupObject _group;
    }
}
