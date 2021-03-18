using System;
using System.Collections.Generic;
using System.Text;

namespace EVDConv
{
    static class Config
    {
        public enum ConvMethod
        {
            EVD2TLT,
            TLT2EVD,
            EVD2TXT,
            TXT2EVD
        }

        public static ConvMethod convertMethod = ConvMethod.EVD2TLT;

        public static string InputFile;
        public static string OutputFile;
        public static string TableFile = "SJIS.tbl";
        public static string PatternFile;
    }
}
