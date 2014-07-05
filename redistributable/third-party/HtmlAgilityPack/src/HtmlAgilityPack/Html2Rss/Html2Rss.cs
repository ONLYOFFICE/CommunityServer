using System;
using System.IO;
using System.Xml;

namespace HtmlAgilityPack.Samples
{
	class Html2Rss
	{
		[STAThread]
		static void Main(string[] args)
		{
			HtmlWeb hw = new HtmlWeb();

			// we are going to use cache, for demonstration purpose only.
			string cachePath = Path.GetFullPath(@".\cache");
			if (!Directory.Exists(cachePath))
			{
				Directory.CreateDirectory(cachePath);
			}
			hw.CachePath = cachePath;
			hw.UsingCache = true;

			// set the following to true, if you don't want to use the Internet at all and if you are sure something is available in the cache (for testing purposes for example).
//			hw.CacheOnly = true;

			// this is the url we want to scrap
			// note: you want to check Terms Of Services, Copyrights and other legal issues if you plan to use this for non personnal work.
			string url = @"http://www.asp.net/Modules/MoreArticles.aspx?tabindex=0&mid=64";

			// there are two methods to do the work
			// 1st method: use XSLT
			ElegantWay(hw, url);

			// 2nd method: use C# code
//			ManualWay(hw, url);
		}


		static void ElegantWay(HtmlWeb hw, string url)
		{
			string xslt = "www.asp.net.ToRss.xsl";

			// copy the file so it exists aside the .exe
			File.Copy(@"..\..\" + xslt, xslt, true);

			// create an XML file
			XmlTextWriter writer = new XmlTextWriter("rss.xml", System.Text.Encoding.UTF8);

			// get an Internet resource and write it as an XML file, after an XSLT transormation
			// if www.asp.net ever change its HTML format, just changes the XSL file. No need for recompilation.
			hw.LoadHtmlAsXml(url, xslt, null, writer);

			// cleanup
			writer.Flush();
			writer.Close();
		}

		static void ManualWay(HtmlWeb hw, string url)
		{
			// get the document from the Internet resource
			HtmlDocument doc = hw.Load(url);

			// we remarked all articles have discriminant target="_new" attribute.
			HtmlNodeCollection hrefs = doc.DocumentNode.SelectNodes("//a[@href and @target='_new']");
			if (hrefs == null)
			{
				return;
			}

			// create fake rss feed
			XmlDocument rssDoc = new XmlDocument();
			rssDoc.LoadXml("<?xml version=\"1.0\" encoding=\"" + doc.Encoding.BodyName + "\"?><rss version=\"0.91\"/>");

			// add channel element and other information
			XmlElement channel = rssDoc.CreateElement("channel");
			rssDoc.FirstChild.NextSibling.AppendChild(channel);

			XmlElement temp = rssDoc.CreateElement("title");
			temp.InnerText = "ASP.Net articles scrap RSS feed";
			channel.AppendChild(temp);

			temp = rssDoc.CreateElement("link");
			temp.InnerText = url;
			channel.AppendChild(temp);

			XmlElement item;
			// browse each article
			foreach(HtmlNode href in hrefs)
			{
				// get what's interesting for RSS
				string link = href.Attributes["href"].Value;
				string title = href.InnerText;
				string description = null;
				HtmlNode descNode = href.SelectSingleNode("../div/text()");
				if (descNode != null)
					description = descNode.InnerText;

				// create XML elements
				item = rssDoc.CreateElement("item");
				channel.AppendChild(item);
				
				temp = rssDoc.CreateElement("title");
				temp.InnerText = title;
				item.AppendChild(temp);

				temp = rssDoc.CreateElement("link");
				temp.InnerText = link;
				item.AppendChild(temp);

				// description is not always here
				if ((description != null) && (description.Length >0))
				{
					temp = rssDoc.CreateElement("description");
					temp.InnerText = description;
					item.AppendChild(temp);
				}
			}
			rssDoc.Save("rss.xml");
		}
	}
}
