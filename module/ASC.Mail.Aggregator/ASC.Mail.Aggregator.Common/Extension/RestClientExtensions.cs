using System;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ASC.Mail.Aggregator.Common.Extension
{
    static class RestClientExtensions
    {
        public static IRestResponse ExecuteSafe(this RestClient client, RestRequest request)
        {
            IRestResponse response;
            try
            {
                response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.InternalServerError &&
                    response.ErrorException == null &&
                    !string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        var json = JObject.Parse(response.Content);
                        if (json == null || json["error"] == null || json["error"]["message"] == null)
                            return response;

                        response.ErrorException = new ApiHelperException(json["error"]["message"].ToString(), response.StatusCode, response.Content);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                response = new RestResponse {ErrorException = ex};
            }

            return response;
        }
    }
}
