using System;

namespace MJs
{
	public class MJsEventArgs : EventArgs
	{
		public MJsEventArgs ()
		{
		}
	}

	public delegate void MJsEventHandler(object sender, MJsEventArgs e);
}

