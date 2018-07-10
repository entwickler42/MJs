using System;

namespace MJs
{
	public class RequestReceivedEventArgs : MJsEventArgs
	{
		public RequestReceivedEventArgs(Request request, String json)
		{
			Request = request;
			JSON = json;
		}
			
		public Request Request { get; protected set; }

		public String JSON { get; protected set; }
	}

	public delegate void RequestReceivedEventHandler(Object sender, RequestReceivedEventArgs e);
}

