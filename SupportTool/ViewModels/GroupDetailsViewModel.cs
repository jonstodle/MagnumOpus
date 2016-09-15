﻿using ReactiveUI;
using SupportTool.Models;

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
