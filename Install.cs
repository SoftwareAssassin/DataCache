///
/// Original Author: Software Assassin
/// 
/// GNU All-Permissive License:
/// Copying and distribution of this file, with or without modification,
/// are permitted in any medium without royalty provided the copyright
/// notice and this notice are preserved.  This file is offered as-is,
/// without any warranty.
/// 
/// Source code available at:
/// https://github.com/SoftwareAssassin/AssassinLibrary
/// 
/// Professional support available at:
/// http://www.softwareassassin.com
/// 

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
