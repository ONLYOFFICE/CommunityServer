using SelectelSharp.Common;
using SelectelSharp.Headers;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SelectelSharp.Models.Container
{
    public class ContainerInfo
    {
        /// <summary>
        /// Количество объектов в контейнере
        /// </summary>
        [Header(HeaderKeys.XContainerObjectCount)]
        public int ContainerObjectCount { get; set; }

        /// <summary>
        /// Суммарный размер объектов в контейнере
        /// </summary>
        [Header(HeaderKeys.XContainerBytesUsed)]
        public long ContainerBytesUsed { get; set; }

        /// <summary>
        /// Скачено байт из контейнера
        /// </summary>
        [Header(HeaderKeys.XTransferedBytes)]
        public long TransferedBytes { get; set; }

        /// <summary>
        /// Передано байт в контейнер
        /// </summary>
        [Header(HeaderKeys.XReceivedBytes)]
        public long ReceivedBytes  { get; set; }

        /// <summary>
        /// Тип контейнера
        /// </summary>
        [Header(HeaderKeys.XContainerMetaType)]
        public ContainerType Type { get; set; }

        /// <summary>
        /// Домены, привзанные к контейнеру
        /// </summary>        
        [Header(HeaderKeys.XContainerDomains)]
        public string ContainerDomains { get; set; }

        /// <summary>
        /// Заголовки с произвольной пользовательской информацией
        /// </summary>
        [Header(CustomHeaders = true)]
        public Dictionary<string, string> CustomHeaders { get; set; }

        [Header(CORSHeaders = true)]
        public CORSHeaders CORSHeaders { get; set; }

        public ContainerInfo(NameValueCollection headers)
        {
            HeaderParsers.ParseHeaders(this, headers);
        }
    }
}
