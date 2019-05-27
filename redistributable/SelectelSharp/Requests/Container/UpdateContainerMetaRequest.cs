using SelectelSharp.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net;
using System.Collections.Generic;
using SelectelSharp.Models.Container;
using SelectelSharp.Headers;

namespace SelectelSharp.Requests.Container
{
    /// <summary>
    /// Запрос на изменение мета-данных контейнера
    /// </summary>
    public class UpdateContainerMetaRequest : ContainerRequest<UpdateContainerResult>
    {
        /// <param name="containerName">Имя контейнера должно быть меньше 256 символов и не содержать завершающего слеша '/' в конце.</param>
        /// <param name="customHeaders">Произвольные мета-данные через передачу заголовков с префиксом X-Container-Meta-.</param>
        /// <param name="type">X-Container-Meta-Type: Тип контейнера (public, private, gallery)</param>
        public UpdateContainerMetaRequest(
            string containerName, 
            ContainerType type = ContainerType.Private, 
            IDictionary<string, object> customHeaders = null, 
            CORSHeaders corsHeaders = null)
            : base(containerName)
        {
            customHeaders.Add(HeaderKeys.XContainerMetaType, type.ToString().ToLower());
            SetCustomHeaders(customHeaders);
            SetCORSHeaders(corsHeaders);
        }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.PUT;
            }
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.Accepted)
            {
                this.Result = UpdateContainerResult.Updated;
            }
            else if (status == HttpStatusCode.Created)
            {
                this.Result = UpdateContainerResult.Created;
            }
            else
            {
                base.ParseError(null, status);
            }
        }

        internal override void ParseError(WebException ex, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NotFound)
            {
                this.Result = UpdateContainerResult.NotFound;
            }
            else
            {
                base.ParseError(null, status);
            }
        }
    }
}
