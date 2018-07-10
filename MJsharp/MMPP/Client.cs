using System;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace MJs.MMPP
{
    public class Client
    {
        const UInt32 THREAD_TIMEOUT_MS = 3000;
        const UInt16 TCP_PORT_DEFAULT = 4200;
        const String TCP_HOST_DEFAULT = "127.0.0.1";

        Thread m_Thread = null;
        TcpClient m_TCP = null;

        bool m_RunSocketReader = false;

        String m_Hostname;
        UInt16 m_Port;
		        
        BlockingCollection<Package> m_TQueue = new BlockingCollection<Package> ();

        public delegate void PackageReceivedEventHander (Client client, Package package);

        public delegate void ExceptionEventHandler (Client client, Exception ex);

        public delegate void ShutdownEventHandler (Client client);

        public event PackageReceivedEventHander PackageReceived;
        public event ExceptionEventHandler ExceptionEvent;
        public event ShutdownEventHandler Shutdown;

        /*! Create a new, unconnected Client instance
		 */
        public Client ()
        {		
        }

        ~Client ()
        {
            Disconnect ();
        }
        /*! Indicates whenever the socket reader is active or not.
		 */
        public Boolean Connected {
            get {
                return null != m_Thread;
            }
        }
        /*! Returns the remote hostname
		 */
        public String Hostname {
            get { return m_Hostname; }
        }
        /*! Returns the remote port
		 */
        public UInt16 Port {
            get { return m_Port; }
        }
        /*! Try to gracefully shutodwn the socket reader, but abort it after a timeout of 5 seconds. 
		*/
        public void Disconnect ()
        {
            m_RunSocketReader = false;

            if (null != m_Thread) {
                bool cleanExit = m_Thread.Join (THREAD_TIMEOUT_MS);
                if (!cleanExit) { 
                    m_Thread.Abort ();
                    Console.Error.WriteLine ("thread aborted!");
                }
                m_Thread = null;
            }
            if (null != m_TCP) {
                m_TCP.Close ();
            }                    
        }
        /*! Establish a new TCP connection and poll for IO using a threaded socket reader.
		 * This method throws a bunch of TcpClient and Thread related Exceptions...
		 */
        public void Connect (String hostname = TCP_HOST_DEFAULT, UInt16 port = TCP_PORT_DEFAULT)
        {
            if (null == m_Thread) {
                TcpClient tcpc = new TcpClient ();
                tcpc.Connect (hostname, port);
                SetSocketOptions (tcpc.Client);	
                m_Hostname = hostname;
                m_Port = port;
                m_TCP = tcpc;

                m_Thread = new Thread (ThreadStart);
                m_RunSocketReader = true;
                m_Thread.Start ();
            }
        }
        /*! Enqueue Package for transmission
		 */
        public Boolean Send (Package pkg)
        {
            if (!m_TQueue.TryAdd (pkg)) {
                Console.Error.WriteLine ("failed to enqueue package!");
                return false;
            }
            return true;
        }
        /*! Main method of the threaded socket reader
		 */
        private void ThreadStart ()
        {
            if (m_TCP.Connected) {
                try {
                    TcpSession ();							
                } catch (Exception ex) {
                    DumpException (ex);
                    EmitException (ex);
                }	
            }            
            EmitShutdown ();
        }
        /*! Poll Socket for IO and process package queues accordingly
		 */
        private void TcpSession ()
        {
            Socket cs = m_TCP.Client;
            Stopwatch minLoopTime = new Stopwatch ();            
            bool sockerr = false;

            while (!sockerr && m_RunSocketReader) {
                minLoopTime.Start ();
                if (!sockerr && cs.Poll (500, SelectMode.SelectError)) {
                    Console.Error.WriteLine ("socket error!");
                    break;
                }
                if (!sockerr && cs.Poll (500, SelectMode.SelectRead)) {
                    Package pkg = new Package ();
                    int c = pkg.Receive (cs);
                    if (Package.HeaderSize == c) {
                        EmitPackageReceived (pkg);
                    } else if (c == 0) {
                        Console.Error.WriteLine (String.Format ("receive failed: {0}/{1}", c, pkg.Size)); 
                        sockerr = true;
                    } else {
                        Console.Error.WriteLine (String.Format ("receive failed: {0}/{1}", c, pkg.Size)); 
                    }
                }
                if (!sockerr && cs.Poll (500, SelectMode.SelectWrite)) {
                    Package pkg = null;
                    while (!sockerr && m_TQueue.TryTake (out pkg)) {
                        int c = pkg.Send (cs);
                        if (c != pkg.Size) {
                            Console.Error.WriteLine (String.Format ("send failed failed: {0}/{1}", c, pkg.Size)); 
                            sockerr = true;
                        }
                    }
                }
                minLoopTime.Stop ();
            }
        }
        /*!\todo Write method documentation
		 */
        private void EmitPackageReceived (Package pkg)
        {
            var e = PackageReceived;
            if (null != e) {
                e (this, pkg);
            }
        }
        /*!\todo Write method documentation
		 */
        private void EmitException (Exception ex)
        {
            var e = ExceptionEvent;
            if (null != e) {
                e (this, ex);
            }
        }
        /*!\todo Write method documentation
		 */
        private void EmitShutdown ()
        {
            var e = Shutdown;
            if (null != e) {
                e (this);
            }
        }



        /*!\todo move the stuff below to somekind of helper class(es) */

        private static void SetSocketOptions (Socket so)
        {
            so.ReceiveBufferSize = 8192;
            so.ReceiveTimeout = 5000;

            so.SendBufferSize = 8192;
            so.SendTimeout = 5000;

            so.Ttl = 42;
            so.NoDelay = true;
            so.LingerState = new LingerOption (false, 0);
            // ? so.ExclusiveAddressUse = true;
        }

        private static void DumpException (Exception ex)
        {
            for (Exception exx = ex; null != exx; exx = exx.InnerException) {
                Console.Error.WriteLine (exx.Message);
                Console.Error.WriteLine (exx.StackTrace);
            }
        }
    }
}
	