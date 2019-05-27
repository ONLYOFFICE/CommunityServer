using System.Collections.Generic;
using System.Collections.Specialized;
using SelectelSharp.Models;
using System.Net;

namespace SelectelSharp.Requests
{
    /// <summary>
    /// Запрос информации о хранилище
    /// </summary>
    public class StorageInfoRequest : BaseRequest<StorageInfo>
    {
        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.HEAD;
            }
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            this.Result = new StorageInfo(headers);
        }
    }
}
