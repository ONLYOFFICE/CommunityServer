using System;
using System.Xml;

namespace BasecampRestAPI
{
	public class XmlHelpers
	{
		public static string ChildNodeText(XmlNode node, string tag)
		{
		    var childNode = node.SelectSingleNode(tag);
			return childNode==null?string.Empty:childNode.InnerText;
		}

		public static string ChildNodeText(XmlNode node, string tag, string defaultValue)
		{
			XmlNode childNode = node.SelectSingleNode(tag);
			return childNode != null ? childNode.InnerText : defaultValue;
		}

		public static int ParseInteger(XmlNode node, string tag)
		{
			string text = ChildNodeText(node, tag);
			return String.IsNullOrEmpty(text) ? -1 : Int32.Parse(text);
		}

		public static DateTime ParseDateTime(XmlNode node, string tag, DateTime defaultValue)
		{
			string text = ChildNodeText(node, tag, String.Empty);
			return String.IsNullOrEmpty(text) ? defaultValue : DateTime.Parse(text);
		}

		public static DateTime ParseDateTime(XmlNode node, string tag)
		{
			return ParseDateTime(node, tag, new DateTime());
		}

		public static bool ParseBool(XmlNode node, string tag)
		{
		    bool parsed;
		    bool.TryParse(ChildNodeText(node, tag, "false"), out parsed);
		    return parsed;
		}

		public static string DateString(DateTime date)
		{
			return String.Format("{0}-{1:D2}-{2:D2}", date.Year, date.Month, date.Day);
		}

		public static string EscapeXml(string text)
		{
			return System.Security.SecurityElement.Escape(text);
		}
	}
}