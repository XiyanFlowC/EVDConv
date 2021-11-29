# EVDConverter
Yet another converter to GUST's EVD file. Original used for converting EVD to TLT so that translators can edit them. (Works on ''Ar tonelico'')

һ����GUST��EVD�ļ�����ת���Ĺ��ߡ���Ҫ��ת��Ϊ TLT ��ʽ�Ա��ڱ༭���������ڣ�ħ����½ϵ�С�Ar tonelico 1 & 2��, ��������ϵ�� A7 ~ A10?��

Camparing to the old version, this version fixed a mismatch between GUST's "align" method and typical aligning method.

����ڵ����������õ�ת���������ת�������ǽ�TLT��id��narrator��Ϣ��ΪУ����;��������Ϊ���ɲ�����

���ԣ�������������������ı�EVD�ļ��еķ��ı����ݡ�

## һЩ��������
��������ı���ʽ����EVD�ĸ�ʽ����֮��İ汾�п�����ӽ��ı���ʽ������ת��ΪEVD���ﵽ��ȫ�Զ����Ч����

## commands.txt
��¼ת�������ֵ�EVD�еĺ����ݵĸ�ʽ��ת����������δ֪�� EVD ��ʱ���������²�������������Ȼ��һ��׼ȷ�����ǲ²��������ݻᱻ������������Խ����˹��༭��ȷ������������ȷ�ԡ�

��ʽΪ��

����,������,��������(,��������)

�����е�������Ϊδ�����ܵ���;�������ġ�

## ��һ��ʹ�� Start Up
Firstly, you should add a record to commands.txt, which is the MACRO_END mark of the macro data stream. For example, to ''Ar Tonelico - Melody of Elemia -'', your should add
```
End,7FFFF03F,0
```
to the commands.txt, or the converter will run out of range.

If you are not add this line and run the programe, clean the file and add this line, all the result will not be right.

������ commands.txt �����һ�м�¼����ǽű������ĺ������������ָ����������򽫰ѽ�β�ĺ�����Ϊ����һ�������������½�������

���磬���� AT1����ӣ�
```
End,7FFFF03F,0
```
���ļ��С���������ǰ�Ѿ����г������ commands.txt ���������ǰ�����ݡ�

## ����
-i=<input filename> ָ�������ļ�
-o=<output filename> ָ������ļ�
-m=<1/2/3/4>
	1 - TLT to EVD
	2 - EVD to TLT
	3 - TXT to EVD����TXT��ʽ����EVD���������ı��⡢�ꡢ��β�ꡢѡ���ı��⣩��
	4 - EVD to TXT
-t=<encoding description.tbl> ָ������ļ�
-p=<pattern file.evd> /m=1 ʱ���룬����������ģ�� EVD ���ܲ��� TLT ��ȱʧ��Ϣ���� EVD