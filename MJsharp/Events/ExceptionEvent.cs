using System;

namespace MJs
{
	using MJsException = System.Exception;

	public class ExceptionEventArgs : MJsEventArgs
	{
		public ExceptionEventArgs (Exception ex)
		{
			Exception = ex;
		}

		public MJsException Exception {
			get;
			private set;
		}
	}

	public delegate void ExecptionEventHandler(object sender, ExceptionEventArgs e);
}

