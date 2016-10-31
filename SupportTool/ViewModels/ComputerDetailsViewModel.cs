using ReactiveUI;
using SupportTool.Models;

namespace SupportTool.ViewModels
{
	public class ComputerDetailsViewModel : ReactiveObject
    {
        private ComputerObject _computer;



        public ComputerDetailsViewModel()
        {

        }



        public ComputerObject Computer
        {
            get { return _computer; }
            set { this.RaiseAndSetIfChanged(ref _computer, value); }
        }
    }
}
