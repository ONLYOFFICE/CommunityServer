using System;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net
{
    public class HttpException : Exception
    {
        private int _HttpCode;

        public HttpException(int httpCode, String message)
            : base(message)
        {
            _HttpCode = httpCode;
        }

        public int GetHttpCode()
        {
            return _HttpCode;
        }
    }
}
