# EVDConv
Yet another Converter for GUST's evd file.

## Usage

EVDConv [options]

options
/i?文件名	输入文件，必须

/t?文件名	指定码表文件，不指定时默认利用SJIS.tbl

/o?文件名	输出文件，必须

/p?文件名	模板文件，某些转换模式必须

/m?方法代号	可用代号：

	1	TLT转为EVD，模板文件必须
  
	2	EVD转为TLT
  
	3	不可用
  
	其他值	抽取EVD全部信息

用例：
抽取EVD：
	EVDConv /i?1.evd /m?0 /o?1.txt
  
不可用：
	EVDConv /i?1.txt /m?3 /o?1.evd
  
转为tlt：
	EVDConv /i?1.txt /m?2 /o?1.evd.txt
  
tlt转为evd
	EVDConv /i?1.evd.txt /m?1 /o?1.evd /p?..\..\Event\1.evd

### Something important
the file command.txt should log the END command or the programe will run out of range.

Just like this for AT1's case:
End,7FFFF03F,0
