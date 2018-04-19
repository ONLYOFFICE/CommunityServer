using SelectelSharp.Common;
using SelectelSharp.Headers;
using SelectelSharp.Models.Container;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SelectelSharp.Requests.Container
{
    /// <summary>
    /// Запрос на преобразовать в галерею для удобного публичного представления в веб-браузере загруженных в нем фотографий и других изображений.
    /// </summary>
    public class UpdateContainerToGalleryRequest : ContainerRequest<UpdateContainerResult>
    {
        /// <param name="containerName">Имя контейнера должно быть меньше 256 символов и не содержать завершающего слеша '/' в конце.</param>
        /// <param name="password">Дополнительно можно установить пароль, по которому будет ограничен доступ.</param>
        public UpdateContainerToGalleryRequest(string containerName, string password = null)
            : base(containerName)
        {
            if (string.IsNullOrEmpty(password) == false)
            {
                TryAddHeader(HeaderKeys.XContainerMetaGallerySecret, Helpers.CalculateSHA1(password));
            }

            TryAddHeader(HeaderKeys.XContainerMetaType, ContainerType.Gallery.ToString().ToLower());
        }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.POST;
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
