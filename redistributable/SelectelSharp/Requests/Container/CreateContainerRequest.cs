using SelectelSharp.Headers;
using SelectelSharp.Models;
using SelectelSharp.Models.Container;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.Container
{
    /// <summary>
    /// Запрос на создание контейнера
    /// </summary>
    public class CreateContainerRequest : ContainerRequest<CreateContainerResult>
    {
        /// <param name="containerName">Имя контейнера должно быть меньше 256 символов и не содержать завершающего слеша '/' в конце.</param>
        /// <param name="customHeaders">Произвольные мета-данные через передачу заголовков с префиксом X-Container-Meta-.</param>
        /// <param name="type">X-Container-Meta-Type: Тип контейнера (public, private, gallery)</param>
        /// <param name="corsHeaders">Дополнительные заголовки кэшировани и CORS</param>
        public CreateContainerRequest(
            string containerName, 
            ContainerType type = ContainerType.Private, 
            Dictionary<string, object> customHeaders = null, 
            CORSHeaders corsHeaders = null)
            : base(containerName)
        {
            if (customHeaders == null) {
                customHeaders = new Dictionary<string, object>();
            }
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
            if (status == HttpStatusCode.Created)
            {
                this.Result = CreateContainerResult.Created;
            }
            else if (status == HttpStatusCode.Accepted)
            {
                this.Result = CreateContainerResult.Exists;
            }
            else
            {
                base.ParseError(null, status);
            }
        }
    }
}
