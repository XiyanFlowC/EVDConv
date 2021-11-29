using System;

namespace EVDConv
{
    class Program
    {
        static void Main(string[] args)
        {
            int ret = ProcessArgu(args);
            if (ret != 0) Environment.Exit(ret);

            if (Config.InputFile == null || Config.OutputFile == null) Environment.Exit(-10);

            //Logger.pen = new System.IO.StreamWriter(System.IO.File.OpenWrite("./log.txt"));

            if (!System.IO.File.Exists("./command.txt")) System.IO.File.Create("./command.txt").Close();
            CommandDataBank cdb = new CommandDataBank("./command.txt");

            if (!System.IO.File.Exists(Config.TableFile))
            {
                Logger.Error($"无法确认给定码表文件 ({Config.TableFile}) 的存在");
                Logger.Log("程序退出。");

                Environment.Exit(-11);
            }

            switch(Config.convertMethod)
            {
                case Config.ConvMethod.EVD2TLT:
                    EVD2TLT(cdb);
                    break;
                case Config.ConvMethod.TLT2EVD:
                    TLT2EVD(cdb);
                    break;
                case Config.ConvMethod.EVD2TXT:
                    ExEVD(cdb);
                    break;
            }

            cdb.SaveData("./command.txt");
        }

        private static void ExEVD(CommandDataBank cdb)
        {
            Converter cvt = new Converter(Config.TableFile);
            EVDFile src = new EVDFile();
            src.LoadEVD(Config.InputFile, cdb);
            string result = cvt.DumpToText(src, cdb);
            System.IO.File.WriteAllText(Config.OutputFile, result);
        }

        private static void TLT2EVD(CommandDataBank cdb)
        {
            string[] data = System.IO.File.ReadAllLines(Config.InputFile);
            Converter cvt = new Converter(Config.TableFile);
            EVDFile pattern = new EVDFile();
            pattern.LoadEVD(Config.PatternFile, cdb);
            EVDFile result = cvt.Convert(data, pattern);
            result.SaveEVD(Config.OutputFile);
        }

        private static void EVD2TLT(CommandDataBank cdb)
        {
            EVDFile ori = new EVDFile();
            ori.LoadEVD(Config.InputFile, cdb);
            Converter cvt = new Converter(Config.TableFile);
            string data = cvt.Convert(ori);
            System.IO.File.WriteAllText(Config.OutputFile, data);
        }

        static int ProcessArgu(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg[0] != '-')
                {
                    Console.WriteLine($"Arg {arg} incorrect.");
                    return -1;
                }
                string[] kvp = arg.Split('=');
                string key = kvp[0][1..];
                string value = kvp.Length > 1 ? kvp[1] : null;

                if (key == "i")
                {
                    if (value == null)
                    {
                        Console.WriteLine("Parameter incorrect.");
                        return -2;
                    }
                    Config.InputFile = value;
                }
                if (key == "o")
                {
                    if (value == null)
                    {
                        Console.WriteLine("Parameter incorrect.");
                        return -3;
                    }
                    Config.OutputFile = value;
                }
                if (key == "m")
                {
                    if (value == null)
                    {
                        Console.WriteLine("Parameter incorrect.");
                        return -4;
                    }
                    int tmpa = int.Parse(value);
                    Config.convertMethod = tmpa switch
                    {
                        1 => Config.ConvMethod.TLT2EVD,
                        2 => Config.ConvMethod.EVD2TLT,
                        3 => Config.ConvMethod.TXT2EVD,
                        _ => Config.ConvMethod.EVD2TXT,
                    };
                }
                if (key == "t")
                {
                    if (value == null)
                    {
                        Console.WriteLine("Parameter incorrect.");
                        return -5;
                    }
                    Config.TableFile = value;
                }
                if (key == "p")
                {
                    if (value == null)
                    {
                        Console.WriteLine("Parameter incorrect.");
                        return -6;
                    }
                    Config.PatternFile = value;
                }
                if (key == "?")
                {
                    Console.Write(
                        "EVD 转换器 作者：希研" +
                        "=======================" +
                        "参数说明：" +
                        "-i=<input filename> 指定输入文件" +
                        "-o=<output filename> 指定输出文件" +
                        "-m=<1/2/3/4>" +
                        "    1 - TLT to EVD" +
                        "    2 - EVD to TLT" +
                        "    3 - TXT to EVD（以TXT形式表述EVD流（包括文本库、宏、收尾宏、选项文本库））" +
                        "    4 - EVD to TXT" +
                        "-t=<encoding description.tbl> 指定码表文件" +
                        "-p=<pattern file.evd> -m=1 时必须，程序依赖此模板 EVD 才能补足 TLT 中缺失信息生成 EVD");
                }
            }
            return 0;
        }
    }
}
