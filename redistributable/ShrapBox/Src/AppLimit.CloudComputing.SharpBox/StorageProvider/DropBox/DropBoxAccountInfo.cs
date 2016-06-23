using System;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    /*    
     * {
     *     “country”: “”,
     *     “display_name”: “John Q. User”,
     *     “quota_info”: 
     *         {
     *             “shared”: 37378890,
     *             “quota”: 62277025792,
     *             “normal”: 263758550
     *         },
     *     “uid”: “174”
     * }
     */

    /// <summary>
    /// This class contains information about the dropbox account
    /// </summary>
    public class DropBoxAccountInfo
    {
        /// <summary>
        /// The country where the user comes from
        /// </summary>
        public readonly String Country;

        /// <summary>
        /// The displayname of the user 
        /// </summary>
        public readonly String DisplayName;

        /// <summary>
        /// The user Id
        /// </summary>
        public readonly Int32 UserId;

        /// <summary>
        /// Quotainformation of dropbox
        /// </summary>
        public readonly DropBoxQuotaInfo QuotaInfo;

        internal DropBoxAccountInfo(String jmstext)
        {
            var jh = new JsonHelper();
            if (!jh.ParseJsonMessage(jmstext))
                return;

            Country = jh.GetProperty("country");
            DisplayName = jh.GetProperty("display_name");
            UserId = jh.GetPropertyInt("uid");

            var quotainfo = jh.GetSubObjectString("quota_info");
            if (quotainfo != null)
                QuotaInfo = new DropBoxQuotaInfo(quotainfo.ToString());
        }
    }
}