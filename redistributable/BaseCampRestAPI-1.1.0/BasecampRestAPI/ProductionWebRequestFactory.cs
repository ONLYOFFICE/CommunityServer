using System.Net;
namespace BasecampRestAPI
{
	class ProductionWebRequestFactory : IWebRequestFactory
	{
		public static ProductionWebRequestFactory GetInstance()
		{
			return new ProductionWebRequestFactory();
		}
		private ProductionWebRequestFactory()
		{
		}

		#region Implementation of IWebRequestFactory
		public IWebRequest CreateWebRequest(string url)
		{
			return ProductionWebRequest.GetInstance(url);
		}
        public HttpWebRequest GetHttpWebRequest(string url)
        {
            return ProductionWebRequest.GetInstance(url).HttpWebRequest;
        }
		#endregion
	}
}
