using System;
using System.Threading;
using MJs.MMPP;

namespace MJs
{
	/*!\todo Documentation...
	 */
	public class MJsClient
	{
		Client m_MMPP = null;

		/*!\todo Documentation...
	 	*/
		Client MMPP 
		{
			get{ 
				Client client = m_MMPP;
				if (null == client) {
					client = new Client ();
					client.Shutdown += OnShutdownEvent;
					client.ExceptionEvent += OnExceptionEvent;
					client.PackageReceived += OnPackageReceived;
					client.Connect ("localhost", 4200);
				}
				m_MMPP = client;

				return client;
			}
		}
		/*!\todo Documentation...
	 	*/
		public MJsClient ()
		{
		}
		/*!\todo Documentation...
	 	*/
		public void Disconnect()
		{
			MMPP.Disconnect ();
		}
		/*!\todo Documentation...
	 	*/
		public String Routine(String name, params String[] args)
		{
			Package pkg = new Package ();
			pkg.Data = String.Format (
				"DO {0}({1})",
				name, String.Join(",",args)
			);
			MMPP.Send (pkg);
			return "";		
		}
		/*!\todo Documentation...
	 	*/
		public String Get(String global, params String[] subscript)
		{
			String value = "";
			return value;
		}
		/*!\todo Documentation...
	 	*/
		public void Set(String value, String global, params String[] subscript)
		{
		}
		/*!\todo Documentation...
	 	*/
		private void OnExceptionEvent (Client client, Exception ex)
		{
			Console.Error.WriteLine (String.Format ("socket exception for {0} thread {1}", client.Hostname, Thread.CurrentThread.ManagedThreadId));
			for (Exception exx = ex; null != exx; exx = exx.InnerException) {
				Console.Error.WriteLine (exx.Message);
				Console.Error.WriteLine (exx.StackTrace);
			}
		}
		/*!\todo Documentation...
	 	*/
		private void OnShutdownEvent (Client client)
		{
			Console.Error.WriteLine (String.Format ("shutdown for {0} thread {1}", client.Hostname, Thread.CurrentThread.ManagedThreadId));

			m_MMPP.Shutdown -= OnShutdownEvent;
			m_MMPP.ExceptionEvent -= OnExceptionEvent;
			m_MMPP.PackageReceived -= OnPackageReceived;
			m_MMPP = null;

			MJsharp.Interactive = false;
		}
		/*!\todo Documentation...
	 	*/
		private void OnPackageReceived (Client client, Package package)
		{
			Console.Error.WriteLine (String.Format ("received package from {0} thread {1}", client.Hostname, Thread.CurrentThread.ManagedThreadId));
		}
	}
}

