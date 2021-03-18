using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EVDConv
{
    /// <summary>
    /// 宏元数据，描述宏的基本信息
    /// </summary>
    class CommandData
    {
        /// <summary>
        /// 宏名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 操作数
        /// </summary>
        public int OpCode { get; set; }
        /// <summary>
        /// 参数个数
        /// </summary>
        public int ParamNum { get; set; }
    }

    /// <summary>
    /// 宏元数据库
    /// </summary>
    class CommandDataBank
    {
        readonly Dictionary<int, CommandData> _data;

        /// <summary>
        /// 构造方法，初始化数据库
        /// </summary>
        /// <param name="dataFilePath">数据文件路径</param>
        public CommandDataBank(string dataFilePath)
        {
            _data = new Dictionary<int, CommandData>();
            string[] lines = File.ReadAllLines(dataFilePath);

            foreach(var line in lines)
            {
                if (String.IsNullOrEmpty(line)) continue;
                if (line[0] == '#') continue;//注释行，忽略

                var fields = line.Split(',');//正常格式中不会出现逗号
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

        /// <summary>
        /// 以操作数获取宏元数据
        /// </summary>
        /// <param name="Opcode">操作数</param>
        /// <returns>获得的元数据，失败返回null</returns>
        public CommandData GetCommandData(int Opcode)
        {
            if (_data.ContainsKey(Opcode)) return _data[Opcode];

            return null;
        }

        /// <summary>
        /// 新增宏元数据
        /// </summary>
        /// <param name="data">要新增的元数据</param>
        /// <exception cref="InvalidOperationException">当尝试设置业已存在的操作数时触发</exception>
        public void SetCommandData(CommandData data)
        {
            if (_data.ContainsKey(data.OpCode)) throw new InvalidOperationException("尝试设置存在的操作数");

            _data[data.OpCode] = data;
        }

        /// <summary>
        /// 注册宏元数据，可以注册一个新发现的宏，并赋予默认名字
        /// </summary> 
        /// <param name="OpCode">欲注册的操作数</param>
        /// <param name="ParamCnt">对应的参数个数</param>
        /// <exception cref="InvalidOperationException">当尝试设置业已存在的操作数时触发</exception>
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

        /// <summary>
        /// 保存数据到数据文件
        /// </summary>
        /// <param name="filePath">数据文件路径</param>
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
