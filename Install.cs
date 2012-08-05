using System;
using Assassin;
using System.ComponentModel;

namespace Assassin.DataCache
{
	[RunInstaller(true)]
	public partial class Install : BackgroundServiceInstaller
	{
		public Install()
			: base("DataCache")
		{
			InitializeComponent();
		}
	}
}
