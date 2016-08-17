using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
