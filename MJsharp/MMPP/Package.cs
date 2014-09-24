using System;
using System.IO;
using System.Text;
using System.Collections;

namespace MJs.MMPP
{
	public class Package
	{
		public const int HeaderSize = 4 * sizeof(UInt32);

		public String data = "";
			
		public Package ()
		{
		}

		public UInt32 magic     = 0x23071981;
		public UInt32 version   = 0x00000001;
		public UInt32 options   = 0x00000000;
		public UInt32 datalen   = 0x00000000;

		public int ReadHeader (byte[] pkg)
		{
			if (pkg.Length < 0x0C) {
				return -1;
			}

			using (MemoryStream mstream = new MemoryStream (pkg)) {
				BinaryReader r = new BinaryReader (mstream);
				UInt32 _magic = r.ReadUInt32 ();
				if (_magic != this.magic) {
					return -2;
				}
				UInt32 version = r.ReadUInt32 ();
				UInt32 options = r.ReadUInt32 ();
				UInt32 datalen = r.ReadUInt32 ();
				this.version = version;
				this.options = options;
				this.datalen = datalen;
			}

			return 0;
		}

		public byte[] Compile ()
		{
			byte[] pkg = null;

			using (MemoryStream mstream = new MemoryStream (pkg)) {
				BinaryWriter w = new BinaryWriter (mstream);				 
				w.Write (this.magic);
				w.Write (this.version);
				w.Write (this.options);
				Byte[] _data = Encoding.UTF8.GetBytes (this.data);
				UInt32 size = Convert.ToUInt32 (_data.Length);
				w.Write (size);
				w.Write (data);
			}

			return pkg;
		}
	}
}

