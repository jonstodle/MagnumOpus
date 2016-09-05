using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
    public class GroupDetailsViewModel : ReactiveObject
    {
        private GroupObject _group;



        public GroupDetailsViewModel()
        {

        }



        public GroupObject Group
        {
            get { return _group; }
            set { this.RaiseAndSetIfChanged(ref _group, value); }
        }
    }
}
