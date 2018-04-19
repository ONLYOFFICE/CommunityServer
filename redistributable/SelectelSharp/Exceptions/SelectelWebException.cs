using System;
using System.Net;

namespace SelectelSharp
{
    public class SelectelWebException : Exception
    {
        public HttpStatusCode HttpStatus { get; private set; }

        public SelectelWebException(HttpStatusCode status, string message)
            : base(message)
        {
            this.HttpStatus = status;
        }

        public SelectelWebException(HttpStatusCode status)
            : base(string.Format("Запрос возвратил ошибку с кодом {0}.", (int)status))
        {
            this.HttpStatus = status;
        }

    }
}
