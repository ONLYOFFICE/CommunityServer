using System;
using System.IO;

namespace HtmlAgilityPack.Samples
{
	class Html2Xml
	{
		[STAThread]
		static void Main(string[] args)
		{
			Test();
		}

		static void Test()
		{
			HtmlToText htt = new HtmlToText();
			string s = htt.Convert(@"..\..\mshome.htm");
			StreamWriter sw = new StreamWriter("mshome.txt");
			sw.Write(s);
			sw.Flush();
			sw.Close();
		}
	}
}
