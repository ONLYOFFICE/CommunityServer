using System;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    class ProductionWebRequest : IWebRequest
    {
        private readonly HttpWebRequest _request;
        private JArray _response;

        public static ProductionWebRequest GetInstance(string url)
        {
            return new ProductionWebRequest(url);
        }
        private ProductionWebRequest(string url)
        {
            _request = (HttpWebRequest)WebRequest.Create(url);
            _request.ContentType = "application/json";
            _request.ServicePoint.Expect100Continue = false;
            _request.UserAgent = "http://www.tm.com";
        }

        #region Implementation of IWebRequest
        public HttpVerb Method
        {
            set { _request.Method = value.ToString().ToUpper(); }
        }

        public string BasicAuthorization
        {
            set { _request.Headers.Add("Authorization", string.Format("Basic {0}", value)); }
        }

        public string RequestText
        {
            set
            {
                using (var writer = new StreamWriter(_request.GetRequestStream()))
                {
                    writer.WriteLine(value);
                }
            }
        }

        public JArray Response
        {
            get
            {
                try
                {
                    return _response ?? (_response = JArray.Parse(ResponseText));
                }
                catch
                {
                    return _response ?? (_response = new JArray(JObject.Parse(ResponseText)));
                }

            }
        }

        public string Location
        {
            get
            {
                if (_location == null)
                {
                    GetResponse();
                    if (_location == null)
                    {
                        _location = string.Empty;
                    }
                }
                return _location;
            }
        }

        #endregion

        public string ResponseText
        {
            get
            {
                if (_responseText == null)
                {
                    GetResponse();
                }
                return _responseText;
            }
        }

        public HttpWebRequest HttpWebRequest
        {
            get
            {
                return _request;
            }
        }

        private void GetResponse()
        {
            GetResponse(0);
        }

        private void GetResponse(int numRetries)
        {
            try
            {
                using (var response = _request.GetResponse())
                {
                    _location = response.Headers["Location"];
                    if (_location != null)
                    {
                        _location = (new Uri(_location)).AbsolutePath;
                    }
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var reader = new StreamReader(responseStream))
                            {
                                _responseText = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (numRetries > 5)
                    throw;

                var httpResponce = e.Response as HttpWebResponse;
                if (httpResponce != null)
                {
                    if (httpResponce.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        //Rate limiting
                        if (!string.IsNullOrEmpty(httpResponce.GetResponseHeader("Retry-After")))
                        {
                            int retryAfter;
                            if (int.TryParse(httpResponce.GetResponseHeader("Retry-After"), out retryAfter))
                            {
                                //Sleep for timeout
                                Thread.Sleep(TimeSpan.FromSeconds(retryAfter + 5));
                                //And then retry
                                GetResponse(++numRetries);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        private string _responseText;
        private string _location;
    }
}