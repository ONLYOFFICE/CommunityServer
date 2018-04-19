using SelectelSharp.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SelectelSharp.Headers
{
    public class CORSHeaders
    {
        /// <summary>
        /// Cache-Control
        /// Описание: Управление кэшированием
        /// Распространенные значения: 
        /// no-cache - запрет кеширования
        /// public - разрешение кеширования страницы как локальным клиентом, так и прокси-сервером
        /// private - разрешение кеширования только локальным клиентом
        /// </summary>
        [Header(HeaderKeys.CacheControl)]
        public string CacheControl { get; set; }

        /// <summary>
        /// Expires
        /// Описание: Дата предполагаемого истечения срока актуальности данных
        /// Распространенные значения: 
        /// Формат: День недели(сокр.), число(2 цифры) Месяц(сокр.) год часы:минуты:секунды GMT
        /// Пример: Tue, 31 Jan 2012 15:02:53 GMT
        /// </summary>
        [Header(HeaderKeys.Expires)]
        public DateTime? Expires { get; set; }

        /// <summary>
        /// Origin
        /// </summary>
        [Header(HeaderKeys.Origin)]
        public string Origin { get; set; }

        /// <summary>
        /// Access-Control-Allow-Origin
        /// Описание: Разрешение на кросс-доменные запросы
        /// </summary>
        [Header(HeaderKeys.AccessControlAllowOrigin)]
        public string AccessControlAllowOrigin { get; set; }

        /// <summary>
        /// Access-Control-Max-Age
        /// Описание: Время кэширования разрешения (в секундах)
        /// </summary>
        [Header(HeaderKeys.AccessControlMaxAge)]
        public int? AccessControlMaxAge { get; set; }

        /// <summary>
        /// Access-Control-Allow-Methods
        /// Описание: Разрешенные методы запросов
        /// Распространенные значения: GET, HEAD
        /// </summary>
        [Header(HeaderKeys.AccessControlAllowMethods)]
        public string AccessControlAllowMethods { get; set; }

        /// <summary>
        /// Access-Control-Allow-Credentials
        /// </summary>
        [Header(HeaderKeys.AccessControlAllowCredentials)]
        public string AccessControlAllowCredentials { get; set; }

        /// <summary>
        /// Access-Control-Expose-Headers
        /// </summary>
        [Header(HeaderKeys.AccessControlExposeHeaders)]
        public string AccessControlExposeHeaders { get; set; }

        /// <summary>
        /// Access-Control-Request-Headers
        /// </summary>
        [Header(HeaderKeys.AccessControlRequestHeaders)]
        public string AccessControlRequestHeaders { get; set; }

        /// <summary>
        /// Access-Control-Request-Method
        /// </summary>
        [Header(HeaderKeys.AccessControlRequestMethod)]
        public string AccessControlRequestMethod { get; set; }

        /// <summary>
        /// Content-Disposition(только для конечного файла)
        /// </summary>
        [Header(HeaderKeys.ContentDisposition)]
        public string ContentDisposition { get; set; }

        /// <summary>
        /// Strict-Transport-Security(только на уровне контейнера)
        /// </summary>
        [Header(HeaderKeys.StrictTransportSecurity)]
        public string StrictTransportSecurity { get; set; }

        public CORSHeaders() { }

        public CORSHeaders(
            string cacheControl = null,
            DateTime? expires = null,
            string origin = null,
            string accessControlAllowOrigin = null,
            int? accessControlMaxAge = null,
            string accessControlAllowMethods = null,
            string accessControlAllowCredentials = null,
            string accessControlExposeHeaders = null,
            string accessControlRequestHeaders = null,
            string accessControlRequestMethod = null,
            string contentDisposition = null,
            string strictTransportSecurity = null)
        {
            this.CacheControl = cacheControl;
            this.Expires = expires;
            this.Origin = origin;
            this.AccessControlAllowOrigin = accessControlAllowOrigin;
            this.AccessControlMaxAge = accessControlMaxAge;
            this.AccessControlAllowMethods = accessControlAllowMethods;
            this.AccessControlAllowCredentials = accessControlAllowCredentials;
            this.AccessControlExposeHeaders = accessControlExposeHeaders;
            this.AccessControlRequestHeaders = accessControlRequestHeaders;
            this.AccessControlRequestMethod = accessControlRequestMethod;
            this.ContentDisposition = contentDisposition;
            this.StrictTransportSecurity = strictTransportSecurity;
        }

        public CORSHeaders(NameValueCollection headers)
        {
            HeaderParsers.ParseHeaders(this, headers);
        }

        public IDictionary<string, object> GetHeaders()
        {
            var result = new Dictionary<string, object>();

            TryAddHeader(result, HeaderKeys.CacheControl, CacheControl);
            TryAddHeader(result, HeaderKeys.Origin, Origin);
            TryAddHeader(result, HeaderKeys.AccessControlAllowOrigin, AccessControlAllowOrigin);
            TryAddHeader(result, HeaderKeys.AccessControlAllowOrigin, AccessControlAllowOrigin);
            TryAddHeader(result, HeaderKeys.AccessControlMaxAge, AccessControlMaxAge);
            TryAddHeader(result, HeaderKeys.AccessControlAllowMethods, AccessControlAllowMethods);
            TryAddHeader(result, HeaderKeys.AccessControlAllowCredentials, AccessControlAllowCredentials);
            TryAddHeader(result, HeaderKeys.AccessControlExposeHeaders, AccessControlExposeHeaders);
            TryAddHeader(result, HeaderKeys.AccessControlRequestHeaders, AccessControlRequestHeaders);
            TryAddHeader(result, HeaderKeys.AccessControlRequestMethod, AccessControlRequestMethod);
            TryAddHeader(result, HeaderKeys.ContentDisposition, ContentDisposition);
            TryAddHeader(result, HeaderKeys.StrictTransportSecurity, StrictTransportSecurity);

            if (Expires.HasValue)
            {
                TryAddHeader(result, HeaderKeys.Expires, Expires.Value.ToString(HeaderKeys.DateFormat));
            }

            return result;
        }

        private void TryAddHeader(Dictionary<string, object> headers, string header, object value)
        {
            if (value != null)
            {
                headers.Add(header, value.ToString());
            }
        }
    }
}
