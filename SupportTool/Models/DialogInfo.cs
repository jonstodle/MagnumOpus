using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
	public struct DialogInfo
	{
		public Type Control { get; private set; }
		public object Parameter { get; private set; }

		public DialogInfo(Type control, object parameter)
		{
			Control = control;
			Parameter = parameter;
		}
	}
}
