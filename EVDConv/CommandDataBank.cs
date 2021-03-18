using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EVDConv
{
    class CommandData
    {
        public string Name { get; set; }
        public int OpCode { get; set; }
        public int ParamNum { get; set; }
    }

    class CommandDataBank
    {
        readonly Dictionary<int, CommandData> _data;

        public CommandDataBank(string dataFilePath)
        {
            _data = new Dictionary<int, CommandData>();
            string[] lines = File.ReadAllLines(dataFilePath);

            foreach(var line in lines)
            {
                if (String.IsNullOrEmpty(line)) continue;
                if (line[0] == '#') continue;

                var fields = line.Split(',');
                var obj = new CommandData()
                {
                    Name = fields[0],
                    OpCode = int.Parse(fields[1], System.Globalization.NumberStyles.HexNumber),
                    ParamNum = int.Parse(fields[2])
                };

                if(_data.ContainsKey(obj.OpCode))//避免多重注册
                {
                    Logger.Warn($"操作数 {obj.OpCode} 多重注册！后注册的操作数将被忽视");
                    continue;
                }

                _data[obj.OpCode] = obj;
            }
        }

        public CommandData GetCommandData(int Opcode)
        {
            if (_data.ContainsKey(Opcode)) return _data[Opcode];

            return null;
        }

        public void SetCommandData(CommandData data)
        {
            if (_data.ContainsKey(data.OpCode)) throw new InvalidOperationException("尝试设置存在的操作数");

            _data[data.OpCode] = data;
        }

        public void RegCommandData(int OpCode, int ParamCnt)
        {
            if (_data.ContainsKey(OpCode)) throw new InvalidOperationException("尝试设置存在的操作数");

            var obj = new CommandData()
            {
                OpCode = OpCode,
                Name = $"ukn_{OpCode:X}",
                ParamNum = ParamCnt
            };

            _data[OpCode] = obj;
        }

        public void SaveData(string filePath)
        {
            StreamWriter pen = new StreamWriter(filePath);
            
            foreach(var entry in _data)
            {
                pen.WriteLine($"{entry.Value.Name},{entry.Value.OpCode:X},{entry.Value.ParamNum}");
                pen.WriteLine();
            }

            pen.Close();
        }
    }
}
