using ReactiveUI;
using SupportTool.Models;

namespace SupportTool.ViewModels
{
	public class ComputerDetailsViewModel : ReactiveObject
    {
        private ComputerObject computer;



        public ComputerDetailsViewModel()
        {

        }



        public ComputerObject Computer
        {
            get { return computer; }
            set { this.RaiseAndSetIfChanged(ref computer, value); }
        }
    }
}
