using System;
using System.Collections;

namespace HtmlAgilityPack.Samples
{
	class GetDocLinks
	{
		[STAThread]
		static void Main(string[] args)
		{
			HtmlWeb hw = new HtmlWeb();
			string url = @"http://www.microsoft.com";
			HtmlDocument doc = hw.Load(url);
			doc.Save("mshome.htm");

			DocumentWithLinks nwl = new DocumentWithLinks(doc);
			Console.WriteLine("Linked urls:");
			for(int i=0;i<nwl.Links.Count;i++)
			{
				Console.WriteLine(nwl.Links[i]);
			}

			Console.WriteLine("Referenced urls:");
			for(int i=0;i<nwl.References.Count;i++)
			{
				Console.WriteLine(nwl.References[i]);
			}
            Console.ReadKey();
		}
	}

	/// <summary>
	/// Represents a document that needs linked files to be rendered, such as images or css files, and points to other HTML documents.
	/// </summary>
	public class DocumentWithLinks
	{
		private ArrayList _links;
		private ArrayList _references;
		private HtmlDocument _doc;

		/// <summary>
		/// Creates an instance of a DocumentWithLinkedFiles.
		/// </summary>
		/// <param name="doc">The input HTML document. May not be null.</param>
		public DocumentWithLinks(HtmlDocument doc)
		{
			if (doc == null)
			{
				throw new ArgumentNullException("doc");
			}
			_doc = doc;
			GetLinks();
			GetReferences();
		}

		private void GetLinks()
		{
			_links = new ArrayList();
			HtmlNodeCollection atts = _doc.DocumentNode.SelectNodes("//*[@background or @lowsrc or @src or @href]");
			if (atts == null)
				return;

			foreach(HtmlNode n in atts)
			{
				ParseLink(n, "background");
				ParseLink(n, "href");
				ParseLink(n, "src");
				ParseLink(n, "lowsrc");
			}
		}

		private void GetReferences()
		{
			_references = new ArrayList();
			HtmlNodeCollection hrefs = _doc.DocumentNode.SelectNodes("//a[@href]");
			if (hrefs == null)
				return;

			foreach(HtmlNode href in hrefs)
			{
				_references.Add(href.Attributes["href"].Value);
			}
		}


		private void ParseLink(HtmlNode node, string name)
		{
			HtmlAttribute att = node.Attributes[name];
			if (att == null)
				return;

			// if name = href, we are only interested by <link> tags
			if ((name == "href") && (node.Name != "link"))
				return;

			_links.Add(att.Value);
		}

		/// <summary>
		/// Gets a list of links as they are declared in the HTML document.
		/// </summary>
		public ArrayList Links
		{
			get
			{
				return _links;
			}
		}

		/// <summary>
		/// Gets a list of reference links to other HTML documents, as they are declared in the HTML document.
		/// </summary>
		public ArrayList References
		{
			get
			{
				return _references;
			}
		}
	}
}
