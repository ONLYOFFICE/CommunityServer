using SelectelSharp.Models.Container;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.Container
{
    /// <summary>
    /// Запрос на удаление контейнера
    /// </summary>
    public class DeleteContainerRequest : ContainerRequest<DeleteContainerResult>
    {
        public DeleteContainerRequest(string containerName)
            : base(containerName)
        {
        }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.DELETE;
            }
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NoContent)
            {
                this.Result = DeleteContainerResult.Deleted;
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
                this.Result = DeleteContainerResult.NotFound;
            }
            else if (status == HttpStatusCode.Conflict)
            {
                this.Result = DeleteContainerResult.NotEmpty;
            }
            else
            {
                base.ParseError(ex, status);
            }
        }
    }
}
