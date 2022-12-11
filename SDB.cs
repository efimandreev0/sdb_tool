using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace sdbToolSharp
{
    internal class SDB
    {
        Encoding utf8 = Encoding.UTF8; //Заюзал глобальную переменную
        public int Count { get; set; }
        public int[] Offsets { get; set; }

        public List<SDBEntry> Entities = new List<SDBEntry>();

        public void Rebuild(string fname)
        {
            //Вытаксиваем подстроку из входящего имени, чтобы использовать ее в имени .sdb и поиске всех частей .txt
            string namePattern = Path.GetFileName(fname).Substring(0, Path.GetFileName(fname).IndexOf("_"));
            //Ищем все .txt согласно паттерну
            string[] parts = Directory.GetFiles(Directory.GetCurrentDirectory(), $"{namePattern}*.txt", SearchOption.TopDirectoryOnly);
            int[] blockPointers = new int[parts.Length]; //Массив поинтеров к блокам
            using(BinaryWriter writer = new BinaryWriter(File.Create(namePattern + ".sdb")))
            {
                writer.Write(parts.Length); //Записываем кол-во блоков
                writer.BaseStream.Position += parts.Length * 4; //Сразу переходим за проеделы диапазона поинтеров
                Utils.AlignPosition(writer); //И выравниваем позицию
                for(int i = 0; i < parts.Length; i++) //Цикл для блоков
                {
                    blockPointers[i] = (int)writer.BaseStream.Position; //Сохраняем позицию в массив

                    //Открываем BinaryWriter в памяти. Этот BinaryWriter не имеет отношения
                    //к уже открытому writer. 
                    using(MemoryStream block = new MemoryStream())
                    using(BinaryWriter blockWriter = new BinaryWriter(block))
                    {
                        string[] text = File.ReadAllLines(parts[i]); //читаем текст
                        //Создаем массивы для поинтеров к тексту и массив с размерами строк
                        int[] textPointers = new int[text.Length];
                        int[] blockSizes = new int[text.Length];
                        blockWriter.Write(text.Length); //Записываем кол-во строк в тексте
                        blockWriter.BaseStream.Position += text.Length * 8; //Переходим на позицию вне диапазона таблицы
                        for(int k = 0; k < text.Length; k++)
                        {
                            //Запись текста и заполнение массивов с поинтерами и размерами
                            string line = text[k];
                            textPointers[k] = (int)blockWriter.BaseStream.Position;
                            blockSizes[k] = utf8.GetByteCount(line) + 1; // + 1 - это с учетом 0 на конце
                            blockWriter.Write(utf8.GetBytes(line));
                            blockWriter.Write(new byte()); //Этого нуля
                        }
                        blockWriter.BaseStream.Position = 4; //Переходим в начало
                        for (int k = 0; k < text.Length; k++)
                        {
                            //Чтобы записать поинтеры/размеры в таблицу
                            blockWriter.Write(textPointers[k]);
                            blockWriter.Write(blockSizes[k]);
                        }
                        //И записываем blockWriter в основной writer
                        writer.Write(block.ToArray());
                    }
                    //Выравнивание между блоками
                    Utils.AlignPosition(writer);
                }
                //Переходим в начало sdb файла и записываем поинтеры к блокам
                writer.BaseStream.Position = 4;
                for (int i = 0; i < blockPointers.Length; i++)
                    writer.Write(blockPointers[i]);
            }
            //Вот так мы записали новый SDB, но не заполнили объекты класса
        }
        public void ExtractText(string file)
        {
            //Тут примерно тот же самый процесс, как у тебя в коде, но записан в подклассы.
            //Поэтому не буду разжевывать как это работает.
            using (BinaryReader reader = new BinaryReader(File.OpenRead(file)))
            {
                ReadSDB(reader);
                int c = 0;
                foreach (var block in Entities)
                {
                    string fname = Path.GetFileNameWithoutExtension(file) + $"_{++c}.txt";
                    File.WriteAllLines(fname, block.Lines);
                    Console.WriteLine("{0} extracted", fname);
                }
            }
        }
        public void ReadSDB(BinaryReader reader)
        {
            Count = reader.ReadInt32();
            Offsets = new int[Count];
            for (int i = 0; i < Count; i++)
                Offsets[i] = reader.ReadInt32();
            for (int i = 0; i < Count; i++)
            {
                reader.BaseStream.Position = Offsets[i];
                Entities.Add(new SDBEntry(reader));
            }
        }
    }

    internal class SDBEntry
    {
        public int Count { get; set; }
        public int[] Offsets { get; set; }
        public int[] Sizes { get; set; }
        public string[] Lines { get; set; }

        public SDBEntry(BinaryReader reader)
        {
            int pos = (int)reader.BaseStream.Position;
            Count = reader.ReadInt32();
            Offsets = new int[Count];
            Sizes = new int[Count];
            Lines = new string[Count];
            for (int i = 0; i < Count; i++)
            {
                Offsets[i] = reader.ReadInt32();
                Sizes[i] = reader.ReadInt32();
            }
            for (int i = 0; i < Count; i++)
            {
                reader.BaseStream.Position = Offsets[i] + pos;
                Lines[i] = Encoding.UTF8.GetString(reader.ReadBytes(Sizes[i] - 1));
            }
        }
    }
}
