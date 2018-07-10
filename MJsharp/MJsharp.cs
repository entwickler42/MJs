using System;
using System.Threading;

namespace MJs
{
    public class MJsharp
    {
		public static bool Interactive = true;

		static void RunClient()
		{
			Console.WriteLine ("Starting MJ Client Thread " + Thread.CurrentThread.ManagedThreadId);
			MJsClient client = new MJsClient();

			try {
				while (Interactive) {
					Console.Write (":>");
					String[] cmdLine = Console.ReadLine ().Split(' ');

					if (!Interactive){
						break;
					}

					switch (cmdLine[0]) {
					case "exit":
						Interactive = false;
						break;

					case "do":                        
						if (cmdLine.Length > 1){
							client.Routine(cmdLine[1]);
						}                        
						break;

					default:
						Console.WriteLine ("unknown command: " + cmdLine[0]);
						break;
					}
				}
			} catch (Exception ex) {
				for (Exception exx = ex; null != exx; exx = exx.InnerException) {
					Console.Error.WriteLine (exx.Message);
					Console.Error.WriteLine (exx.StackTrace);
				}
			}

			client.Disconnect ();
		}

        static int Main (String[] args)
        {
			RunClient ();		
			Console.ReadKey ();
            return 0;
        }
    }
}

