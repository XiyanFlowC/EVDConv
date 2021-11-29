# EVDConverter
Yet another converter to GUST's EVD file. Original used for converting EVD to TLT so that translators can edit them. (Works on ''Ar tonelico'')

一个对GUST的EVD文件进行转换的工具。主要是转换为 TLT 格式以便于编辑。（适用于：魔塔大陆系列《Ar tonelico 1 & 2》, 炼金工作室系列 A7 ~ A10?）

Camparing to the old version, this version fixed a mismatch between GUST's "align" method and typical aligning method.

相比于导入器中内置的转换器，这个转换器并非将TLT的id和narrator信息作为校验用途，而是作为生成参数。

所以，可以利用这个功能来改变EVD文件中的非文本数据。

## 一些其他功能
设计了以文本形式描述EVD的格式。在之后的版本中可能添加将文本格式的描述转换为EVD来达到完全自定义的效果。

## commands.txt
记录转换器发现的EVD中的宏数据的格式。转换器在遇到未知的 EVD 宏时可以自主猜测宏参数个数。虽然不一定准确，但是猜测所得数据会被保存在这里。可以进行人工编辑来确保宏描述的正确性。

格式为：

名字,操作数,参数数量(,参数描述)

括号中的内容是为未来可能的用途而保留的。

## 第一次使用 Start Up
Firstly, you should add a record to commands.txt, which is the MACRO_END mark of the macro data stream. For example, to ''Ar Tonelico - Melody of Elemia -'', your should add
```
End,7FFFF03F,0
```
to the commands.txt, or the converter will run out of range.

If you are not add this line and run the programe, clean the file and add this line, all the result will not be right.

首先向 commands.txt 中添加一行记录，表记脚本结束的宏操作数需首先指定。否则程序将把结尾的宏误认为具有一个参数，并导致解析错误。

例如，对于 AT1，添加：
```
End,7FFFF03F,0
```
到文件中。如果在添加前已经运行程序，清空 commands.txt 后重新添加前述内容。

## 参数
-i=<input filename> 指定输入文件
-o=<output filename> 指定输出文件
-m=<1/2/3/4>
	1 - TLT to EVD
	2 - EVD to TLT
	3 - TXT to EVD（以TXT形式表述EVD流（包括文本库、宏、收尾宏、选项文本库））
	4 - EVD to TXT
-t=<encoding description.tbl> 指定码表文件
-p=<pattern file.evd> /m=1 时必须，程序依赖此模板 EVD 才能补足 TLT 中缺失信息生成 EVD