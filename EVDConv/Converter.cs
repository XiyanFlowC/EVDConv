using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EVDConv
{
    class Converter
    {
        readonly Dictionary<int, char> decodeTable;
        readonly Dictionary<char, int> encodeTable;

        /// <summary>
        /// 初始化转换器
        /// </summary>
        /// <param name="tablePath">转换码表路径</param>
        public Converter(string tablePath)
        {
            decodeTable = new Dictionary<int, char>();
            encodeTable = new Dictionary<char, int>();
            string[] lines = File.ReadAllLines(tablePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                string[] field = line.Split('=');

                if (field.Length == 3)
                {
                    decodeTable[int.Parse(field[0], System.Globalization.NumberStyles.AllowHexSpecifier)] = '=';
                    encodeTable['='] = int.Parse(field[0], System.Globalization.NumberStyles.AllowHexSpecifier);
                }
                else
                {
                    decodeTable[int.Parse(field[0], System.Globalization.NumberStyles.AllowHexSpecifier)] = field[1][0];
                    encodeTable[field[1][0]] = int.Parse(field[0], System.Globalization.NumberStyles.AllowHexSpecifier);
                }
            }
        }

        /// <summary>
        /// 转换为EVD
        /// </summary>
        /// <param name="tltLines">TLT的全部内容</param>
        /// <param name="pattern">模板EVD</param>
        /// <returns>取得的EVD，失败返回null</returns>
        public EVDFile Convert(string[] tltLines, EVDFile pattern)
        {
            EVDFile ret = pattern.Clone();
            int i = 0, j = 0, k = 0;
            foreach (string fileLine in tltLines)
            {
                if (string.IsNullOrEmpty(fileLine)) continue;

                string[] field = fileLine.Split(',');

                if (i == -1)
                {
                    //result.Author = fileLine;
                    continue;
                }

                if (field.Length == 3)
                {
                    ret.EventTexts[i].ID = int.Parse(field[0]);
                    ret.EventTexts[i].Narrator = int.Parse(field[1]);
                    ret.EventTexts[i++].Data = ConvANSIString(field[2]);
                }
                else if (field.Length == 1)
                {
                    if (j < ret.EventSelectionGroups.Count)
                    {
                        ret.EventSelectionGroups[j].Selections[k++] = ConvANSIString(fileLine);
                        if (k == ret.EventSelectionGroups[j].Selections.Length) { j++; k = 0; }
                        if (j == ret.EventSelectionGroups.Count) i = -1;
                    }
                }
                else return null;
            }

            return ret;
        }

        /// <summary>
        /// 转换EVD为TLT字符串
        /// </summary>
        /// <param name="src">欲转换的EVD文件</param>
        /// <returns>TLT字符串</returns>
        public string Convert(EVDFile src)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var text in src.EventTexts)
            {
                sb.Append(string.Format("{0},{1},{2}\n\n", text.ID, text.Narrator, ConvANSIString(text.Data)));
            }

            foreach(var group in src.EventSelectionGroups)
            {
                foreach(var selection in group.Selections)
                {
                    sb.Append(string.Format("{0}\n\n", ConvANSIString(selection)));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 转换原始C字符串为字符串
        /// </summary>
        private string ConvANSIString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            int tmp = 0;
            foreach(byte a in data)
            {
                if(tmp != 0)
                {
                    tmp |= a;
                    sb.Append(decodeTable[tmp]);
                    tmp = 0;
                    continue;
                }

                if(a == 0)
                {
                    break;
                }
                else if(a > 0x7f)//DBC 起始标志
                {
                    tmp = a << 8;
                }
                else
                {
                    sb.Append(decodeTable[a]);
                }
            }

            return sb.ToString();
        }

        private byte[] ConvANSIString(string v)
        {
            List<byte> result = new List<byte>();

            foreach(char t in v)
            {
                int tmp = encodeTable[t];
                if(tmp <= 0xff)
                {
                    result.Add((byte)tmp);
                }
                else
                {
                    result.Add((byte)((tmp & 0xff00) >> 8));
                    result.Add((byte)(tmp & 0xff));
                }
            }
            result.Add(0x0);

            return result.ToArray();
        }

        public string DumpToText(EVDFile src, CommandDataBank cdb)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[TextDataBank]\n");
            sb.Append("#ID,Narrator,PortraidID,Position,PortraitDiffA,PortraitDiffB,Text\n");
            foreach(var entry in src.EventTexts)
            {
                sb.Append($"{entry.ID},{entry.Narrator},{entry.PortraitID},{entry.Position},{entry.PortraitDiffA},{entry.PortraitDiffB},{ConvANSIString(entry.Data)}\n\n");
            }

            sb.Append("[ControllMacro]\n");
            foreach(var entry in src.EventMacros)
            {
                var data = cdb.GetCommandData(entry.OpCode);
                sb.Append(data.Name);

                if(data.ParamNum != 0)
                {
                    sb.Append("\t\t");
                    sb.Append(entry.Args[0]);

                    for (int i = 1; i < entry.Args.Length; i++)
                    {
                        sb.Append(',');
                        sb.Append(entry.Args[i]);
                    }
                }

                sb.Append('\n');
            }
            sb.Append('\n');

            sb.Append("[FinnalizeMacro]\n");
            foreach(var entry in src.EventFinalizeSeqMacros)
            {
                var data = cdb.GetCommandData(entry.OpCode);
                sb.Append(data.Name);

                if (data.ParamNum != 0)
                {
                    sb.Append("\t\t");
                    sb.Append(entry.Args[0]);

                    for (int i = 1; i < entry.Args.Length; i++)
                    {
                        sb.Append(',');
                        sb.Append(entry.Args[i]);
                    }

                    sb.Append('\n');
                }
            }
            sb.Append('\n');

            sb.Append("[SelectionGroup]\n.Count=");
            sb.Append(src.EventSelectionGroups.Count);
            sb.Append("\n");
            foreach(var group in src.EventSelectionGroups)
            {
                sb.Append($".group\n");
                foreach(var entry in group.Selections)
                {
                    sb.Append(ConvANSIString(entry));
                    sb.Append("\n\n");
                }
            }
            return sb.ToString();
        }
    }
}
