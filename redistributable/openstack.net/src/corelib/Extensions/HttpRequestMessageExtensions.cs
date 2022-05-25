namespace System.Net.Http
{
    internal static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage Copy(this HttpRequestMessage request)
        {
            var message = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version
            };

            foreach (var property in request.Properties)
            {
                message.Properties.Add(property);
            }

            foreach (var header in request.Headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return message;
        }
    }
}