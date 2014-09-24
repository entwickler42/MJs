using System;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MJs.MMPP
{
	public class Client
	{
		Thread m_Thread = null;
		TcpClient m_TCP = null;

		bool m_RunSocketReader = false;

		String m_Hostname;
		UInt16 m_Port;

		Queue<Package> m_SendBuffer = new Queue<Package> ();

		public delegate void PackageReceivedEventHander (Client client, Package package);
		public delegate void ExceptionEventHandler (Client client, Exception ex);
		public delegate void ShutdownEventHandler (Client client);

		public event PackageReceivedEventHander PackageReceived;
		public event ExceptionEventHandler ExceptionEvent;
		public event ShutdownEventHandler Shutdown;

		public Client()
		{		
		}

		~Client()
		{
			Disconnect ();
		}
		//! Indicates whenever the socket reader is active or not.
		public Boolean Connected
		{
			get{
				return null != m_Thread;
			}
		}
		//! Join socket reader thread
		public void Join()
		{
			if (null != m_Thread) {
				m_Thread.Join ();
			}
		}
		//! Try to gracefully shutodwn the socket reader, but abort it after a timeout of 5 seconds.
		public void Disconnect()
		{
			m_RunSocketReader = false;

			if (null != m_Thread) {
				bool cleanExit = m_Thread.Join (5000);
				if (!cleanExit) { 
					m_Thread.Abort ();
					Console.Error.WriteLine ("thread aborted!");
				}
				m_Thread = null;
			}
		}
		/*! Establish a new TCP connection and poll for IO using a threaded socket reader.
		 * This method throws a bunch of TcpClient and Thread related Exceptions...
		 */
		public void Connect (String hostname = "127.0.0.1", UInt16 port = 4200)
		{
			if (null != m_Thread) {
				m_Thread = new Thread (ThreadStart);
				m_Hostname = hostname;
				m_Port = port;
				m_RunSocketReader = true;

				TcpClient tcpc = new TcpClient ();
				tcpc.Connect(m_Hostname,m_Port);
				SetSocketOptions (tcpc.Client);	
				m_TCP = tcpc;

				m_Thread.Start ();
			}
		}	

		private void SendPackage(Package pkg)
		{
			lock (m_SendBuffer) {
				m_SendBuffer.Enqueue (pkg);
			}
		}

		//! Main method of the threaded socket reader
		private void ThreadStart()
		{
			if (m_TCP.Connected) {
				try {
					TcpSession ();							
				} catch (Exception ex) {
					DumpException (ex);
					EmitException (ex);
				}
				try {				
					m_TCP.Close ();				
				} catch (Exception ex) {
					DumpException (ex);
					EmitException (ex);
				}	
			}

			EmitShutdown ();
		}
		//! Poll Socket for IO and process package queues accordingly
		private void TcpSession()
		{
			Socket cs = m_TCP.Client;;
			Stopwatch minLoopTime = new Stopwatch ();
			byte[] buf = new byte[Package.HeaderSize];

			while (m_RunSocketReader) {
				minLoopTime.Start ();
				if (cs.Poll (10, SelectMode.SelectError)) {
					Console.Error.WriteLine ("socket error!");
					break;
				}
				if (cs.Poll (10, SelectMode.SelectRead)) {
					Package pkg = new Package ();
					ReadPackage (ref buf, ref cs, ref pkg);
				}
				if (cs.Poll (10, SelectMode.SelectWrite)) {
					lock (m_SendBuffer) {
						int i = 0;
						while (i < 10 && i < m_SendBuffer.Count()) {
							Package pkg = m_SendBuffer.Dequeue ();
							byte[] cpkg = pkg.Compile ();
							int c = cs.Send (cpkg);
							if (c != cpkg.Length) {
								Console.Error.WriteLine ("failed to send complete package!");
								break;
							}
						}
					}
				}
				minLoopTime.Stop ();
			}
		}	
		//! Used receiv Package objects and trigger error/event handling
		private void ReadPackage(ref byte[] buf, ref Socket so, ref Package pkg)
		{
			int c = so.Receive(buf);
			if (c == 0) {
				Console.Error.WriteLine ("receive header timeout!");
			}else if(c == Package.HeaderSize){
				int status = pkg.ReadHeader (buf);
				if (status == 0){
					byte[] data = new byte[pkg.datalen];
					c = so.Receive (data);
					if (c == 0) {
						Console.Error.WriteLine ("receive data timeout!");
					} else if (c == pkg.datalen) {
						pkg.data = Encoding.UTF8.GetString (data);
						EmitPackageReceived (pkg);
					} else {
						Console.Error.WriteLine ("unable to read complete data!");
					}
				}else{
					Console.Error.WriteLine(String.Format("failed to parse package - errorcode {0}", status));
				}
			}else{
				Console.Error.WriteLine ("unable to read complete header!");
			}
		}

		private static void DumpException(Exception ex)
		{
			for(Exception exx = ex; null != exx; exx = exx.InnerException){
				Console.Error.WriteLine (exx.Message);
				Console.Error.WriteLine (exx.StackTrace);
			}
		}

		private void EmitPackageReceived(Package pkg)
		{
			var e = PackageReceived;
			if (null != e) {
				e(this, pkg);
			}
		}

		private void EmitException(Exception ex)
		{
			var e = ExceptionEvent;
			if (null != e) {
				e(this, ex);
			}
		}

		private void EmitShutdown()
		{
			var e = Shutdown;
			if (null != e) {
				e(this);
			}
		}

		/**\todo move the stuff below to somekind of helper class(es) */

		private static void SetSocketOptions(Socket so)
		{
			so.ReceiveBufferSize = 8192;
			so.ReceiveTimeout = 1000;

			so.SendBufferSize = 8192;
			so.SendTimeout = 1000;

			so.Ttl = 42;
			so.NoDelay = true;
			so.LingerState = new LingerOption (false, 0);
			so.ExclusiveAddressUse = true;
		}

	}
}

