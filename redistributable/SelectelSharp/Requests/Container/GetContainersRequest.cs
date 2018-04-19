using SelectelSharp.Models;
using SelectelSharp.Models.Container;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.Container
{
    /// <summary>
    /// Запрос информации по хранилищу со списком контейнеров
    /// </summary>
    public class GetContainersRequest : BaseRequest<ContainersList>
    {
        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.HEAD;
            }
        }

        /// <param name="limit">Число, ограничивает количество объектов в результате (по умолчанию 10000)</param>
        /// <param name="marker">Cтрока, результат будет содержать объекты по значению больше указанного маркера (полезно использовать для постраничной навигации и при большом количестве контейнеров)</param>
        public GetContainersRequest(int limit = 10000, string marker = null)
        {
            this.TryAddQueryParam("limit", limit);
            this.TryAddQueryParam("marker", marker);
            this.TryAddQueryParam("format", "json");
        }

        internal override void Parse(NameValueCollection headers, object content, HttpStatusCode status)
        {
            if (status == HttpStatusCode.OK)
            {
                base.Parse(headers, content, status);
                this.Result.StorageInfo = new StorageInfo(headers);
            }
            else if (status == HttpStatusCode.NoContent)
            {
                this.Result = null;
            }
            else
            {
                throw new SelectelWebException(status);
            }
        }
    }
}
