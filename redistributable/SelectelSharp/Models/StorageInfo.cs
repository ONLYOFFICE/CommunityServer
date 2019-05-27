using SelectelSharp.Common;
using SelectelSharp.Headers;
using System.Collections.Specialized;

namespace SelectelSharp.Models
{
    /// <summary>
    /// Информация о хранилище
    /// </summary>
    public class StorageInfo
    {
        /// <summary>
        /// Суммарный объём хранения данных в хранилище(в байтах)
        /// </summary>
        [Header(HeaderKeys.XAccountBytesUsed)]
        public long AccountBytesUsed { get; private set; }

        /// <summary>
        /// Количество контейнеров
        /// </summary>
        [Header(HeaderKeys.XAccountContainerCount)]
        public int AccountContainerCount { get; private set; }

        /// <summary>
        /// Суммарное количество хранимых объектов
        /// </summary>
        [Header(HeaderKeys.XAccountObjectCount)]
        public int AccountObjectCount { get; private set; }

        /// <summary>
        /// Скачено байт из хранилища(за всё время)
        /// </summary>
        [Header(HeaderKeys.XTransferedBytes)]
        public long TransferedBytes { get; private set; }

        /// <summary>
        /// Закачено байт в хранилище(за всё время)
        /// </summary>
        [Header(HeaderKeys.XReceivedBytes)]
        public long ReceivedBytes { get; private set; }

        public StorageInfo(NameValueCollection headers)
        {
            HeaderParsers.ParseHeaders(this, headers);
        }
    }
}
