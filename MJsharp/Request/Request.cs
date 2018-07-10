using System;
using Newtonsoft.Json;

namespace MJs
{
    [JsonObject (MemberSerialization.OptOut)]
	public class Request
    {
        public Request ()
        {
			ID = GetUniqueID();
			Type = String.Format ("{0}.{1}", GetType ().Namespace, GetType ().Name);
        }			

        public UInt64 ID {
			get;
			set;
        }

		public String Type {
			get;
			set;
		}

        public virtual String Serialize ()
        {
            return JsonConvert.SerializeObject (this);
        }			

        private static UInt64 g_CommandCounter = 0;

		private static UInt64 GetUniqueID ()
        {
			UInt64 cid = ++g_CommandCounter;
			if (cid == UInt64.MaxValue) {
				g_CommandCounter = 0;
			}
			return UInt64.MaxValue - cid;
        }
    }
}

