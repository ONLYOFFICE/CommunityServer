using System;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    /*
     *  {
     *      “shared”: 37378890,
     *      “quota”: 62277025792,
     *      “normal”: 263758550
     *  }
     */

    /// <summary>
    /// This class contains information about the dropbox quota
    /// </summary>
    public class DropBoxQuotaInfo
    {
        /// <summary>
        /// How many bytes are shared with other users
        /// </summary>
        public readonly UInt64 SharedBytes;

        /// <summary>
        /// What is the size limit of the dropbox
        /// </summary>
        public readonly UInt64 QuotaBytes;

        /// <summary>
        /// How many not shared bytes are used
        /// </summary>
        public readonly UInt64 NormalBytes;

        internal DropBoxQuotaInfo(String jmstext)
        {
            var jh = new JsonHelper();
            if (jh.ParseJsonMessage(jmstext))
            {
                SharedBytes = Convert.ToUInt64(jh.GetProperty("shared"));
                QuotaBytes = Convert.ToUInt64(jh.GetProperty("quota"));
                NormalBytes = Convert.ToUInt64(jh.GetProperty("normal"));
            }
        }
    }
}