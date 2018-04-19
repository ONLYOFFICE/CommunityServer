using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SelectelSharp.Headers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectelSharp.Requests.CDN
{
    public class CDNIvalidationRequest : BaseRequest<CDNIvalidationResult>
    {
        public CDNIvalidationRequest(Uri[] uri)
        {
            if (uri.Length == 0)
                throw new ArgumentNullException("uri");
                        
            this.ContentStream = new MemoryStream(Encoding.UTF8.GetBytes(String.Join("\n", uri.Select(x => x.ToString()))));
            this.AutoCloseStream = true;
            this.AutoResetStreamPosition = false;

            //this.TryAddHeader(HeaderKeys.ContentLenght, this.File.Length);
            this.TryAddHeader(HeaderKeys.ContentType, "text/plain");           
        }
       

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.PURGE;
            }            
        }

        internal override void Parse(System.Collections.Specialized.NameValueCollection headers, object content, System.Net.HttpStatusCode status)
        {
            this.Result = JsonConvert.DeserializeObject<CDNIvalidationResult>(content as string);        
        }

        internal override void ParseError(System.Net.WebException ex, System.Net.HttpStatusCode status)
        {
            base.ParseError(ex, status);
        }

        protected override string GetUrl(string storageUrl)
        {
            return "https://api.selcdn.ru/v1/cdn";
        }
    }
}
