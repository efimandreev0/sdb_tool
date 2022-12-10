using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace sdb_Tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Extractor("00000192.sdb");
        }

            public static void Extractor(string sdbFile)
            { 
            var reader = new BinaryReader(File.OpenRead(sdbFile));
            int count = reader.ReadInt32();
            int[] blockOffset = new int[count]; //Создаём массив для сбора оффсетов блоков
            

            for (int i = 0; i < count; i++)
            {
                blockOffset[i] = reader.ReadInt32();
            }
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = blockOffset[i];
                int textCount = reader.ReadInt32();

                int[] qstrA = new int[textCount];
                int[] qstrB = new int[textCount];
                string[] strings = new string[textCount];
                for (int x = 0; x < textCount; x++)
                {
                    qstrA[x] = reader.ReadInt32();
                    qstrB[x] = reader.ReadInt32();
                }
                for (int x = 0; x < textCount; x++)
                {
                    reader.BaseStream.Position = qstrA[x] + blockOffset[i];
                    strings[x] = Encoding.UTF8.GetString(reader.ReadBytes(qstrB[x] - 1));
                }
                File.WriteAllLines(sdbFile + "_" + i.ToString() + ".txt", strings);
            }
            {

            }
        }
    }
}
