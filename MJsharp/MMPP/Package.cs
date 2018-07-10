using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Collections;

namespace MJs.MMPP
{
    public class Package
    {
		public const int HeaderSize = 4 * sizeof(UInt32);       
		public const UInt32 Magic = 0x23071981;	

		public UInt32 Version = 0x00000001;
		public UInt32 Options = 0x00000000;
		public UInt32 Datalen = 0x00000000;

		String m_Data = "";

		public Package ()
		{}			

		public String Data
		{
			get{ 
				return m_Data; 
			}
			set { 
				Datalen = Convert.ToUInt32(value.Length);
				m_Data = value;
			}
		}

		public UInt32 Size
		{
			get { return Package.HeaderSize + Datalen; }
		}

		public int Receive (Socket so)
        {
            UInt32 magic = 0;
            UInt32 version = 0;		
            UInt32 options = 0;
            UInt32 datalen = 0;
			String data = "";

            byte[] buf = new byte[Package.HeaderSize];

            int c = so.Receive (buf);

            if (c == Package.HeaderSize) {
                using (MemoryStream mstream = new MemoryStream (buf)) {
                    BinaryReader r = new BinaryReader (mstream);
                    magic = r.ReadUInt32 ();
                    if (magic == Package.Magic) {
                        version = r.ReadUInt32 ();
                        options = r.ReadUInt32 ();
                        datalen = r.ReadUInt32 ();
                    }
                }                    
            } 
                
            if (datalen > 0) {
                byte[] rbuf = new byte[datalen];
                c = so.Receive (rbuf);
                if (c == datalen) {
                    data = Encoding.UTF8.GetString (rbuf);
                }
            }

            this.Version = version;
            this.Options = options;            
			this.Data = data;

            return c;
        }

        public int Send (Socket cs)
        {
			byte[] buf = new byte[HeaderSize + Data.Length];

			using (MemoryStream mstream = new MemoryStream (buf)) {
				BinaryWriter w = new BinaryWriter (mstream);				 
				w.Write (Package.Magic);
				w.Write (this.Version);
				w.Write (this.Options);                
				w.Write (Datalen);
				w.Write (Encoding.UTF8.GetBytes (this.Data));
			}

            return cs.Send (buf);
        }
    }
}

