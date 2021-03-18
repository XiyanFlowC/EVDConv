using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EVDConv
{
    /// <summary>
    /// 操作GUST的EVD事件数据文件
    /// </summary>
    class EVDFile
    {
        public EVDFile()
        {
            EventTexts = new List<TextEntry>();
            EventMacros = new List<Macro>();
            EventFinalizeSeqMacros = new List<Macro>();
            EventSelectionGroups = new List<SelectionGroup>();
        }

        /// <summary>
        /// 文本数据
        /// </summary>
        public class TextEntry
        {
            public int ID { get; set; }
            public int Narrator { get; set; }
            public int PortraitID { get; set; }
            public short PortraitDiffA { get; set; }
            public short PortraitDiffB { get; set; }
            public int Position { get; set; }
            public byte[] Data { get; set; }
        }
        /// <summary>
        /// 宏数据
        /// </summary>
        public class Macro
        {
            /// <summary>
            /// 操作数
            /// </summary>
            public int OpCode { get; set; }
            /// <summary>
            /// 参数
            /// </summary>
            public int[] Args { get; set; }
        }
        /// <summary>
        /// 选项组
        /// </summary>
        public class SelectionGroup
        {
            /// <summary>
            /// 分支选项
            /// </summary>
            public byte[][] Selections { get; set; }
        }

        /// <summary>
        /// 获取事件文本数据
        /// </summary>
        public List<TextEntry> EventTexts { get; private set; }
        /// <summary>
        /// 获取事件宏序列
        /// </summary>
        public List<Macro> EventMacros { get; private set; }

        public void SaveEVD(string outputFile)
        {
            BinaryWriter pen = new BinaryWriter(File.OpenWrite(outputFile));
            SaveEVD(pen);
            pen.Close();
        }

        /// <summary>
        /// 获取事件收尾宏
        /// </summary>
        public List<Macro> EventFinalizeSeqMacros { get; private set; }
        /// <summary>
        /// 获取事件选项组数据
        /// </summary>
        public List<SelectionGroup> EventSelectionGroups { get; private set; }

        /// <summary>
        /// 从二进制流载入EVD
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <param name="cdb">宏元数据</param>
        public void LoadEVD(BinaryReader reader, CommandDataBank cdb)
        {
            int eventCount = reader.ReadInt32();//获取事件文本个数

            for (int i = 0; i < eventCount; ++i)
            {
                EventTexts.Add(GetTextEntry(reader));//获取事件文本
            }

            int macroCount = reader.ReadInt32();//获取事件宏个数

            for(int i = 0;i < macroCount; ++i)//获取事件宏
            {
                var tmp = GetMacroEntry(reader, cdb);
                i += tmp.Args.Length;
                EventMacros.Add(tmp);
            }

            macroCount = reader.ReadInt32();//获取事件收尾宏个数

            for(int i = 0; i < macroCount; ++i)//获取事件收尾宏
            {
                var tmp = GetMacroEntry(reader, cdb);
                i += tmp.Args.Length;
                EventFinalizeSeqMacros.Add(tmp);
            }

            int choiceCount = reader.ReadInt32();//获取选项组个数
            for(int i = 0; i < choiceCount; ++i)
            {
                int selectionCount = reader.ReadInt32();//获取选项个数
                List<byte[]> strs = new List<byte[]>();
                for(int j = 0; j < selectionCount; ++j)
                {
                    int length = reader.ReadInt32();//获取选项文本长度
                    strs.Add(GetANSIString(reader, length));//获取选项文本
                }
                EventSelectionGroups.Add(new SelectionGroup
                {
                    Selections = strs.ToArray()
                });
            }
        }

        /// <summary>
        /// 保存EVD文件到给定二进制流中
        /// </summary>
        /// <param name="writer">要写入的二进制流</param>
        public void SaveEVD(BinaryWriter writer)
        {
            writer.Write(EventTexts.Count);//写入事件文本数据个数
            
            foreach(var entry in EventTexts)
            {
                WriteEventTextEntry(entry, writer);//写入事件文本数据
            }

            //writer.Write(EventMacros.Count);
            int leng = 0;
            foreach(var entry in EventMacros)
            {
                leng += entry.Args.Length;
                leng += 1;
            }
            writer.Write(leng);//写入事件脚本个数

            foreach(var entry in EventMacros)
            {
                writer.Write(entry.OpCode);//写入操作数

                foreach(var arg in entry.Args)
                {
                    writer.Write(arg);//写入参数
                }
            }

            //writer.Write(EventFinalizeSeqMacros.Count);
            leng = 0;
            foreach (var entry in EventFinalizeSeqMacros)
            {
                leng += entry.Args.Length;
                leng += 1;
            }
            writer.Write(leng);//写入事件收尾脚本个数

            foreach (var entry in EventFinalizeSeqMacros)
            {
                writer.Write(entry.OpCode);//写入操作数

                foreach(var arg in entry.Args)
                {
                    writer.Write(arg);//写入参数
                }
            }

            writer.Write(EventSelectionGroups.Count);//写入选项组个数

            foreach(var entry in EventSelectionGroups)
            {
                writer.Write(entry.Selections.Length);//写入选项数

                foreach(var text in entry.Selections)
                {
                    byte[] padded = GetPaddedData(text);

                    writer.Write(text.Length);//写入选项文本长度
                    writer.Write(padded);//写入选项文本
                }
            }
        }

        /// <summary>
        /// 辅佐方法，将给定的文本项写入给定的二进制流中
        /// </summary>
        /// <param name="entry">要写入的文本项</param>
        /// <param name="writer">要被写入的流</param>
        private void WriteEventTextEntry(TextEntry entry, BinaryWriter writer)
        {
            writer.Write(entry.Position);//显示位置
            writer.Write(entry.Narrator);//讲述人，映射关系未知
            writer.Write(entry.PortraitID);//立绘ID，显示的立绘的基本索引
            writer.Write(entry.ID);//索引ID，用于数据定位
            writer.Write(entry.PortraitDiffA);//立绘差分A（同时存在于A6，内存布局可见，映射关系未知
            writer.Write(entry.PortraitDiffB);//立绘差分B（同时存在于A6

            byte[] padded = GetPaddedData(entry.Data);

            writer.Write(entry.Data.Length);//写入文本
            writer.Write(padded);
        }

        /// <summary>
        /// 获取将传入数据块进行了尾部填充以对齐的数据块
        /// </summary>
        /// <param name="data">要执行对齐填充的数据块</param>
        /// <param name="padByte">对齐字节数，默认4字节</param>
        /// <returns></returns>
        private static byte[] GetPaddedData(byte[] data, int padByte = 4)
        {
            int leng = data.Length;
            leng = (leng + (padByte - 1)) & ~(padByte - 1);
            byte[] padded = new byte[leng];
            data.CopyTo(padded, 0);
            return padded;
        }

        /// <summary>
        /// 辅佐方法，通过宏元数据从流中获取单个宏数据
        /// 如果宏元数据中不存在某宏的元数据，将被自动添加
        /// 流将停止于此宏项末尾字节的下一个字节
        /// </summary>
        /// <param name="reader">二进制流读取器</param>
        /// <param name="cdb">宏元数据</param>
        /// <returns>获取到的宏项</returns>
        private Macro GetMacroEntry(BinaryReader reader, CommandDataBank cdb)
        {
            int opcode = reader.ReadInt32();//获取操作数
            var mdata = cdb.GetCommandData(opcode);//获取宏元数据
            if(mdata == null)//不存在
            {
                List<int> paras1 = new List<int>();
                int tmp = reader.ReadInt32();
                while(0 != ((tmp & 0xfffff000) ^ 0x7ffff000))//连续查找直到某个值具有操作数特征（0x7ffffNNN）
                {
                    paras1.Add(tmp);
                    tmp = reader.ReadInt32();
                }
                reader.BaseStream.Seek(-4, SeekOrigin.Current);//回退，使流停止于下一个宏项起始处

                cdb.RegCommandData(opcode, paras1.Count);//注册猜测的元数据
                Logger.Log($"Guessed a opcode {opcode}.");

                return new Macro()//返回取得的宏
                {
                    Args = paras1.ToArray(),
                    OpCode = opcode
                };
            }

            int[] paras = new int[mdata.ParamNum];
            for(int i = 0; i < mdata.ParamNum; ++i)
            {
                paras[i] = reader.ReadInt32();
            }

            return new Macro
            {
                OpCode = opcode,
                Args = paras
            };
        }

        /// <summary>
        /// 从指定的文件载入EVD
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="cdb">宏元数据</param>
        public void LoadEVD(string path, CommandDataBank cdb)
        {
            var pen = new BinaryReader(File.OpenRead(path));
            LoadEVD(pen, cdb);
            pen.Close();
        }

        /// <summary>
        /// 辅佐方法，获取文本项
        /// 二进制流应当处于文本项头部
        /// 流指针将停止于数据末尾的下一个字节
        /// </summary>
        /// <param name="reader">要读取的二进制流</param>
        /// <returns>取得的文本项</returns>
        private TextEntry GetTextEntry(BinaryReader reader)
        {
            TextEntry rlt = new TextEntry
            {
                Position = reader.ReadInt32(),
                Narrator = reader.ReadInt32(),
                PortraitID = reader.ReadInt32(),
                ID = reader.ReadInt32(),
                PortraitDiffA = reader.ReadInt16(),
                PortraitDiffB = reader.ReadInt16()
            };

            int length = reader.ReadInt32();//文本长度，我们需要【注意，获取到的长度不一定准确，请万分注意】

            rlt.Data = GetANSIString(reader, length);

            return rlt;
        }

        /// <summary>
        /// 辅佐方法，获取文本数据，并返回原始字节（包括终止标志'\0'）
        /// 流将停止于填充区末尾的下一个字节
        /// </summary>
        /// <param name="reader">二进制流读取器</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        private byte[] GetANSIString(BinaryReader reader, int length)//TODO:增加码表转换
        {
            length = (length + 3) & (~3);
            byte[] ori = reader.ReadBytes(length);
            List<byte> result = new List<byte>();
            foreach(byte bt in ori)
            {
                if (bt == 0x0) break;
                result.Add(bt);
            }
            result.Add(0x0);
            return result.ToArray();
        }

        /// <summary>
        /// 获取一个完全的EVD副本
        /// </summary>
        /// <returns>EVD副本</returns>
        public EVDFile Clone()
        {
            EVDFile result = new EVDFile();

            for(int i = 0; i < EventTexts.Count; ++i)
            {
                TextEntry dst = new TextEntry(), src = EventTexts[i];

                dst.ID = src.ID;
                dst.Narrator = src.Narrator;
                dst.PortraitDiffA = src.PortraitDiffA;
                dst.PortraitDiffB = src.PortraitDiffB;
                dst.PortraitID = src.PortraitID;
                dst.Position = src.Position;
                dst.Data = (byte[])src.Data.Clone();

                result.EventTexts.Add(dst);
            }

            for(int i = 0; i < EventMacros.Count; ++i)
            {
                Macro dst = new Macro(), src = EventMacros[i];
                dst.OpCode = src.OpCode;
                dst.Args = (int[])src.Args.Clone();
                result.EventMacros.Add(dst);
            }

            for (int i = 0; i < EventFinalizeSeqMacros.Count; ++i)
            {
                Macro dst = new Macro(), src = EventFinalizeSeqMacros[i];
                dst.OpCode = src.OpCode;
                dst.Args = (int[])src.Args.Clone();
                result.EventFinalizeSeqMacros.Add(dst);
            }

            for(int i = 0; i < EventSelectionGroups.Count; ++i)
            {
                SelectionGroup dst = new SelectionGroup(), src = EventSelectionGroups[i];

                dst.Selections = new byte[src.Selections.Length][];

                for(int j = 0; j < src.Selections.Length; ++j)
                {
                    dst.Selections[j] = (byte[])src.Selections[j].Clone();
                }
                result.EventSelectionGroups.Add(dst);
            }

            return result;
        }
    }
}
