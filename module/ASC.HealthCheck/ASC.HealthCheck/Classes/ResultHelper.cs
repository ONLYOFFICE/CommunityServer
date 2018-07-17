using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using ASC.HealthCheck.Resources;

namespace ASC.HealthCheck.Classes
{
    public class ResultHelper
    {
        private readonly MediaTypeFormatter formatter;
        public ResultHelper(MediaTypeFormatter formatter)
        {
            this.formatter = formatter;
        }

        public HttpResponseMessage GetContent(object result)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<object>(result, formatter)
            };
        }

        public HttpResponseMessage Success(string status = "", string message = "")
        {
            return GetContent(new {code = 1, status, message});
        }

        public HttpResponseMessage Error(string status, string message = "")
        {
            return GetContent(new { code = 0, status, message });
        }

        public HttpResponseMessage StatusServerError(string message = "")
        {
            return Error(HealthCheckResource.StatusServiceError, message);
        }

        public HttpResponseMessage WrongParameterError()
        {
            return StatusServerError(HealthCheckResource.WrongParameter);
        }

        public HttpResponseMessage ServerError()
        {
            return StatusServerError(HealthCheckResource.ServerError);
        }

        public HttpResponseMessage WrongServiceNameError()
        {
            return StatusServerError(HealthCheckResource.WrongServiceName);
        }
    }
}
