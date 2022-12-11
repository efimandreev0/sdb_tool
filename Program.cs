using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sdbToolSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            string extension = Path.GetExtension(args[0]);
            SDB sdb = new SDB();
            switch(extension)
            {
                case ".sdb":
                    {
                        sdb.ExtractText(args[0]);
                        break;
                    }
                case ".txt":
                    {
                        sdb.Rebuild(args[0]);
                        break;
                    }
                default: return;
            }
        }
    }
}
