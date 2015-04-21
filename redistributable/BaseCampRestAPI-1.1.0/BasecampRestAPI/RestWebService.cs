using System;
using System.Text;
using System.Xml;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	public class RestWebService : IRestWebService
	{
		public static RestWebService GetInstance(string url, string userName, string password)
		{
			return GetInstance(url, userName, password, ProductionWebRequestFactory.GetInstance());
		}
		public static RestWebService GetInstance(string url, string userName, string password, IWebRequestFactory factory)
		{
			return new RestWebService(url, userName, password, factory);
		}
		protected RestWebService(string url, string userName, string password, IWebRequestFactory factory)
		{
			Url = url;
			UserName = userName;
			_authorization = AuthenticationFromUserNamePassword(userName, password);
			_webRequestFactory = factory;
		}
		public string Url { get; private set; }
		public string UserName { get; private set; }
		protected readonly string _authorization;
		protected readonly IWebRequestFactory _webRequestFactory;

		public static string AuthenticationFromUserNamePassword(string userName, string password)
		{
			return Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", userName, password)));
		}

		public JArray GetRequestResponseElement(string requestPath)
		{
			return GetRequestResponseElement(requestPath, string.Empty);
		}

        public JArray GetRequestResponseElement(string requestPath, string data)
		{
			return GetRequestResponseElement(requestPath, data, HttpVerb.Get);
		}

        public JArray GetRequestResponseElement(string requestPath, string data, HttpVerb verb)
		{
			IWebRequest request = _webRequestFactory.CreateWebRequest(string.Format("{0}/{1}", Url, requestPath));
			request.Method = verb;
			request.BasicAuthorization = _authorization;
			if (verb != HttpVerb.Get)
			{
				request.RequestText = data;
			}
			return request.Response;
		}

		public string PostRequestGetLocation(string requestPath, string data)
		{
			IWebRequest request = _webRequestFactory.CreateWebRequest(string.Format("{0}/{1}", Url, requestPath));
			request.Method = HttpVerb.Post;
			request.BasicAuthorization = _authorization;
			request.RequestText = data;
			return request.Location;
		}

		public string PutRequest(string path, string data)
		{
			IWebRequest request = _webRequestFactory.CreateWebRequest(string.Format("{0}/{1}", Url, path));
			request.Method = HttpVerb.Put;
			request.BasicAuthorization = _authorization;
			request.RequestText = data;
			return request.ResponseText;
		}

		public string DeleteRequest(string requestPath)
		{
			IWebRequest request = _webRequestFactory.CreateWebRequest(string.Format("{0}/{1}", Url, requestPath));
			request.Method = HttpVerb.Delete;
			request.BasicAuthorization = _authorization;
			request.RequestText = string.Empty;
			return request.ResponseText;
		}

        public HttpWebRequest GetRequest(string url)
        {
            IWebRequest request = _webRequestFactory.CreateWebRequest(url);
            request.Method = HttpVerb.Get;
            request.BasicAuthorization = _authorization;
            return request.HttpWebRequest;
        }
	}
}