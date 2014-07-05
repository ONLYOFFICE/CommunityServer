using System.Net;
namespace BasecampRestAPI
{
	public interface IWebRequestFactory
	{
		IWebRequest CreateWebRequest(string url);
        HttpWebRequest GetHttpWebRequest(string url);
	}
}