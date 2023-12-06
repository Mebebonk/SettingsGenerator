using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingsGenerator
{
	internal class SimpleErrorHandler : BaseErrorHandler
	{
		public override void HandleEmptyLoadFile(object caller) { return; }
		public override FileStream? HandleNoFileFound(object caller) => null;	

	}
}
