using System;
using System.Threading;
using MJs;

namespace MJsConsole
{
	public class MJsConsole
	{
		public static bool Interactive = true;

		static void RunClient ()
		{			
			MJsClient client = new MJsClient ();

			try {
				while (Interactive) {
					Console.Write (":>");
					String[] cmdLine = Console.ReadLine ().Split (' ');
					String cmd = cmdLine [0].ToLower();

					if (!Interactive) { break; }

					switch (cmd) {
					case "e":
					case "exit":
						Interactive = false;
						break;

					case "d":
					case "do":                        
						if (cmdLine.Length > 1) {
							String[] args = new string[cmdLine.Length - 2];
							for (int i = 0; i < args.Length; i++) {
								args [i] = cmdLine [i + 2];
							}
							String rval = client.Routine (cmdLine [1], args);
							Console.WriteLine(rval);
						}
						break;

					default:
						Console.WriteLine ("unknown command: " + cmdLine [0]);
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
			// Console.ReadKey ();
			return 0;
		}
	}
}
