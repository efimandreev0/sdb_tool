using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdbToolSharp
{
    public static class Utils
    {
		public static void AlignPosition(BinaryWriter writer, int align = 0x10)
		{
			long pos = writer.BaseStream.Position;
			if (pos % align != 0)
				writer.BaseStream.Position = (align - pos % align) + pos;
		}
    }
}
