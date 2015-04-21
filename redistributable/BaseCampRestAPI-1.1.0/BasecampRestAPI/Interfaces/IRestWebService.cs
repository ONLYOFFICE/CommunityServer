using System.Xml;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	public interface IRestWebService
	{
		string Url { get; }
		string UserName { get; }

		JArray GetRequestResponseElement(string requestPath);
        JArray GetRequestResponseElement(string requestPath, string data);
        JArray GetRequestResponseElement(string requestPath, string data, HttpVerb verb);
		string PostRequestGetLocation(string requestPath, string data);
		string PutRequest(string requestPath, string data);
		string DeleteRequest(string requestPath);

        HttpWebRequest GetRequest(string url);
	}
}