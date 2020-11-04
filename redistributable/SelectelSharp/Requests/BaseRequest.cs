using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SelectelSharp.Headers;
using SelectelSharp.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SelectelSharp.Requests
{
    public abstract class BaseRequest<T>
    {
        private const int BufferSize = 4096;

        public bool AutoCloseStream { get; protected set; }
        public bool AutoResetStreamPosition { get; protected set; }
        public Stream ContentStream { get; protected set; }

        public T Result { get; protected set; }
        public bool IsSuccesful { get; set; }

        public virtual bool AllowAnonymously
        {
            get
            {
                return false;
            }
        }

        public virtual bool DownloadData
        {
            get
            {
                return false;
            }
        }

        internal virtual RequestMethod Method
        {
            get
            {
                return RequestMethod.GET;
            }
        }

        private IDictionary<string, string> query = new Dictionary<string, string>();
        private IDictionary<string, string> headers = new Dictionary<string, string>();

        public BaseRequest()
        {
        }

        protected void SetCustomHeaders(IDictionary<string, object> headers = null)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                var headerKey = header.Key;
                //if (!headerKey.ToLower().StartsWith(HeaderKeys.XContainerMetaPrefix.ToLower()))
                //{
                //    headerKey = string.Concat(HeaderKeys.XContainerMetaPrefix, header.Key);
                //}

                this.TryAddHeader(headerKey, header.Value);
            }
        }

        protected void SetCORSHeaders(CORSHeaders cors = null)
        {
            if (cors == null)
            {
                return;
            }

            foreach (var header in cors.GetHeaders())
            {
                this.TryAddHeader(header.Key, header.Value);
            }
        }

        protected void SetConditionalHeaders(ConditionalHeaders conditional = null)
        {
            if (conditional == null)
            {
                return;
            }

            foreach (var header in conditional.GetHeaders())
            {
                this.TryAddHeader(header.Key, header.Value);
            }
        }

        protected virtual string GetUrl(string storageUrl)
        {
            return storageUrl;
        }

        private Uri GetUri(string storageUrl)
        {
            var url = GetUrl(storageUrl);

            // set query params
            if (query != null && query.Any())
            {
                var queryParamsList = query
                    .Where(x => !string.IsNullOrEmpty(x.Value))
                    .Select(x => string.Concat(x.Key, "=", x.Value));

                var queryParams = string.Join("&", queryParamsList);
                if (url.Contains("?"))
                {
                    url = string.Concat(url, queryParams);
                }
                else
                {
                    url = string.Concat(url, "?", queryParams);
                }
            }

            return new Uri(url);
        }

        internal void TryAddQueryParam(string key, object value)
        {
            if (value != null)
            {
                query.Add(key, value.ToString());
            }
        }

        internal void TryAddHeader(string key, object value)
        {
            if (value != null)
            {
                headers.Add(key, value.ToString());
            }
        }

        internal virtual async Task Execute(string storageUrl, string authToken)
        {
            var uri = GetUri(storageUrl);

            if (ContentStream != null && AutoResetStreamPosition)
            {
                ContentStream.Position = 0;
            }

#if DEBUG
            Debug.WriteLine(uri.ToString());
#endif

            var request = HttpWebRequest.CreateHttp(uri);
            request.Method = this.Method.ToString();

            if (!AllowAnonymously)
            {
                TryAddHeader(HeaderKeys.XAuthToken, authToken);
            }

            // set Accept header
            if (headers.ContainsKey(HeaderKeys.Accept))
            {
                request.Accept = headers[HeaderKeys.Accept];
                headers.Remove(HeaderKeys.Accept);
            }

            // set Content-Type header
            if (headers.ContainsKey(HeaderKeys.ContentType))
            {
                request.ContentType = headers[HeaderKeys.ContentType];
                headers.Remove(HeaderKeys.ContentType);
            }

            // set Content-Length header
            // todo: make SetContLength method
            if (headers.ContainsKey(HeaderKeys.ContentLenght))
            {
                request.ContentLength = long.Parse(headers[HeaderKeys.ContentLenght]);
                headers.Remove(HeaderKeys.ContentLenght);
            }

            // set custom headers
            if (headers != null)
            {
                foreach (var kv in headers.Where(x => !string.IsNullOrEmpty(x.Value)))
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }

            if (ContentStream != null)
            {
                long fsLength = 0;

                using (Stream rs = request.GetRequestStream())
                {
                    var buffer = new byte[BufferSize];

                    int readed;

                    while ((readed = ContentStream.Read(buffer, 0, BufferSize)) != 0)
                    {
                        await rs.WriteAsync(buffer, 0, readed);
                        fsLength += readed;
                    }

                    if (AutoCloseStream)
                        ContentStream.Close();

                }
            }

            try
            {
                var response = request.GetResponse();

                var status = ((HttpWebResponse)response).StatusCode;

                if (DownloadData)
                {
                    var responseStream = response.GetResponseStream();

                    if (!responseStream.CanSeek || responseStream.CanTimeout)
                    {
                        //Buffer it
                        var memStream = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, BufferSize, FileOptions.DeleteOnClose);

                        try
                        {
                            var buffer = new byte[BufferSize];

                            int readed;

                            while ((readed = responseStream.Read(buffer, 0, BufferSize)) != 0)
                            {
                                memStream.Write(buffer, 0, readed);                               
                            }

                        }
                        finally
                        {
                            responseStream.Close();
                        }
                        
                        memStream.Position = 0;

                        responseStream = memStream;
                    }

                    this.Parse(response.Headers, responseStream, status);
                }
                else
                {
                    using (var rs = response.GetResponseStream())
                    {
                        ParseString(response, status, rs);
                    }
                }

            }
            catch (WebException ex)
            {
                var status = ((HttpWebResponse)ex.Response).StatusCode;
                ParseError(ex, status);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void ParseString(WebResponse response, HttpStatusCode status, Stream rs)
        {
            using (var reader = new StreamReader(rs, Encoding.UTF8))
            {
                string content = reader.ReadToEnd();
                this.Parse(response.Headers, content, status);
            }
        }

        private void ParseData(WebResponse response, HttpStatusCode status, Stream rs)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                rs.CopyTo(ms);
                this.Parse(response.Headers, ms.ToArray(), status);
            }
        }

        internal virtual void Parse(NameValueCollection headers, object content, HttpStatusCode status)
        {
            if (content != null)
            {
                SetResult(content);
            }
            else
            {
                SetStatusResult(status);
            }
        }

        internal virtual void ParseError(WebException ex, HttpStatusCode status)
        {
            if (ex != null)
            {
                throw ex;
            }
            else
            {
                throw new SelectelWebException(status);
            }
        }

        internal virtual void SetStatusResult(HttpStatusCode status, params HttpStatusCode[] successfulStatuses)
        {
            if (typeof(T).IsEnum && typeof(T).Equals(status.GetType()))
            {
                if (successfulStatuses == null || !successfulStatuses.Any() || successfulStatuses.Contains(status))
                {
                    SetResult(status);
                }
                else
                {
                    throw new SelectelWebException(status);
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void SetResult(object content)
        {
            if (content is T)
            {
                this.Result = (T)content;
                IsSuccesful = true;
            }
            else if (content is string)
            {
                this.Result = JsonConvert.DeserializeObject<T>(content as string, new IsoDateTimeConverter { DateTimeFormat = "YYYY-MM-DDThh:mm:ss.sTZD" });
                IsSuccesful = true;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}