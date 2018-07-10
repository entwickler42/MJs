using System;
using System.Collections.Specialized;

namespace MJs
{
	public class RoutineRequest : ReplyRequest
    {
        StringCollection m_Args = new StringCollection ();

        public String Routine {
            get;
            set;
        }

        public String Result {
            get;
            set;
        }

        public StringCollection Arguments {
            get{ return m_Args; }
            set{ m_Args = value; }
        }
    }
}

