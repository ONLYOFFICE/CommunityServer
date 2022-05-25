using System;

namespace AppLimit.CloudComputing.SharpBox.Common.Net
{
    public class HttpException : Exception
    {
        private int _HttpCode;

        public HttpException(int httpCode, string message)
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
