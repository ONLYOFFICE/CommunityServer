using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using ASC.Common.Logging;

namespace ASC.Common.Radicale
{
    public static class RadicaleClient
    {
        private static readonly ILog Logger = BaseLogManager.GetLogger("ASC.Radicale");

        public async static Task<DavResponse> CreateAsync(DavRequest davRequest)
        {
            davRequest.Method = "MKCOL";
            var response = await RequestAsync(davRequest).ConfigureAwait(false);
            return GetDavResponse(response);
        }

        public async static Task<DavResponse> GetAsync(DavRequest davRequest)
        {
            davRequest.Method = "GET";
            var response = await RequestAsync(davRequest).ConfigureAwait(false);
            var davResponse = new DavResponse()
            {
                StatusCode = (int)response.StatusCode
            };

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                davResponse.Completed = true;
                davResponse.Data = await response.Content.ReadAsStringAsync();
            }
            else
            {
                davResponse.Completed = false;
                davResponse.Error = response.ReasonPhrase;
            }

            return davResponse;
        }


        public async static Task<DavResponse> UpdateItemAsync(DavRequest davRequest)
        {
            davRequest.Method = "PUT";
            var response = await RequestAsync(davRequest).ConfigureAwait(false);
            return GetDavResponse(response);
        }

        public async static Task<DavResponse> UpdateAsync(DavRequest davRequest)
        {
            davRequest.Method = "PROPPATCH";
            var response = await RequestAsync(davRequest).ConfigureAwait(false);
            return GetDavResponse(response);
        }

        public async static Task RemoveAsync(DavRequest davRequest)
        {
            davRequest.Method = "DELETE";
            await RequestAsync(davRequest).ConfigureAwait(false);
        }

        private async static Task<HttpResponseMessage> RequestAsync(DavRequest davRequest)
        {
            try
            {
                using (var hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(davRequest.Authorization)));
                    if (!String.IsNullOrEmpty(davRequest.Header)) hc.DefaultRequestHeaders.Add("X_REWRITER_URL", davRequest.Header);
                    var method = new HttpMethod(davRequest.Method);
                    var request = new HttpRequestMessage(method, davRequest.Url);

                    if (davRequest.Data != null)
                    {
                        request.Content = new StringContent(davRequest.Data);
                    }
                    return await hc.SendAsync(request).ConfigureAwait(false);

                }
            }
            catch (AggregateException ex)
            {
                throw new RadicaleException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw new RadicaleException(ex.Message);
            }
        }


        private static DavResponse GetDavResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return new DavResponse()
                {
                    Completed = true,
                    Data = response.IsSuccessStatusCode ? response.RequestMessage.RequestUri.ToString() : response.ReasonPhrase,
                };

            }
            else
            {
                return new DavResponse()
                {
                    Completed = false,
                    StatusCode = (int)response.StatusCode,
                    Error = response.ReasonPhrase
                };
            }

        }
    }
}
