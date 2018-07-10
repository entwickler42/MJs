using System;
using System.Threading;
using Newtonsoft.Json;

namespace MJs
{
	public class ReplyRequest : Request
	{
		AutoResetEvent m_WaitReply = new AutoResetEvent (false);

		public delegate void ReplyReadyHandler (Request sender);

		public event ReplyReadyHandler ReplyReady;

		public ReplyRequest ()
		{
			RequestID = 0;
		}

		public UInt64 RequestID {
			get;
			set;
		}

		public bool WaitReply (int timeout = -1)
		{
			return m_WaitReply.WaitOne (timeout);
		}

		public virtual void ReceiveReply (String reply)
		{
			Reply = reply;

			var ev = ReplyReady;
			if (null != ev) {
				ev (this);
			}

			m_WaitReply.Set ();
		}

		[JsonIgnore]
		public String Reply {
			get;
			protected set;
		}

		[JsonIgnore]
		public bool IsReply {
			get{ return RequestID != 0; }
		}
	}
}

