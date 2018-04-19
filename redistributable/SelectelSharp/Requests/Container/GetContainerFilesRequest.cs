using System.Collections.Specialized;
using System.Net;
using SelectelSharp.Models.Container;
using SelectelSharp.Models;

namespace SelectelSharp.Requests.Container
{
    public class GetContainerFilesRequest : ContainerRequest<ContainerFilesList>
    {
        /// <summary>
        /// Запрос на получение списка файлов в контейнере
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="limit">Число, ограничивает количество объектов в результате (по умолчанию 10000)</param>
        /// <param name="marker">Cтрока, результат будет содержать объекты по значению больше указанного маркера (полезно использовать для постраничной навигации и при большом количестве контейнеров)</param>        
        /// <param name="prefix">Строка, вернуть объекты имена которых начинаются с указанного префикса</param>
        /// <param name="path">Строка, вернуть объекты в указанной папке(виртуальные папки)</param>
        /// <param name="delimeter">Символ, вернуть объекты до указанного разделителя в их имени</param>
        public GetContainerFilesRequest(
            string containerName, 
            int limit = 10000, 
            string marker = null,
            string prefix = null,
            string path = null,
            string delimeter = null) : 
            base(containerName)
        {
            if (limit != int.MaxValue)
                this.TryAddQueryParam("limit", limit);
    
            this.TryAddQueryParam("marker", marker);
            this.TryAddQueryParam("prefix", prefix);
            this.TryAddQueryParam("path", path);
            this.TryAddQueryParam("delimeter", delimeter);
            this.TryAddQueryParam("format", "json");
        }

        internal override void Parse(NameValueCollection headers, object content, HttpStatusCode status)
        {
            if (status == HttpStatusCode.OK)
            {
                base.Parse(headers, content, status);
                this.Result.StorageInfo = new StorageInfo(headers);
            }
            else
            {
                throw new SelectelWebException(status);
            }
        }

        internal override void ParseError(WebException ex, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NotFound)
            {
                this.Result = null;
            }
            else
            {
                base.ParseError(ex, status);
            }
        }
    }
}
