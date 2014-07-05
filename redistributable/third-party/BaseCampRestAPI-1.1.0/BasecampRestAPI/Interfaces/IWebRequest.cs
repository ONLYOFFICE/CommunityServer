using System.Xml;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	public enum HttpVerb
	{
		Get,
		Post,
		Put,
		Delete
	};
	public interface IWebRequest
	{
		HttpVerb Method { set; }
		string BasicAuthorization { set; }
		string RequestText { set;  }
		JArray Response { get; }
		string ResponseText { get; }
		string Location { get; }
        HttpWebRequest HttpWebRequest { get; }
	}
}