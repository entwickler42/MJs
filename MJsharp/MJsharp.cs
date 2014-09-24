using System;
using MJs.MMPP;

namespace MJs
{
	public class MJsharp
	{
		static int Main(String[] args)
		{
			Client client = new Client ();

			try{
				client.Connect ();
				client.Disconnect ();
			}catch(Exception ex){
				for(Exception exx = ex; null != exx; exx = exx.InnerException){
					Console.Error.WriteLine (exx.Message);
					Console.Error.WriteLine (exx.StackTrace);
				}
			}

			return 0;
		}
	}
}

