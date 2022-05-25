namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// This class contains the limits of a given cloud storage
    /// configuration
    /// </summary>
    public class CloudStorageLimits
    {
        /// <summary>
        /// Default ctor which sets the limits to an 
        /// unlimited value (no limits)
        /// </summary>
        public CloudStorageLimits()
            : this(-1, -1)
        {
        }

        /// <summary>
        /// Special ctor which allows to initials the limits with
        /// special values in an external protocol provider
        /// </summary>
        /// <param name="maxUploadFileSize"></param>
        /// <param name="maxDownloadFileSite"></param>
        public CloudStorageLimits(long maxUploadFileSize, long maxDownloadFileSite)
        {
            MaxUploadFileSize = maxUploadFileSize;
            MaxChunkedUploadFileSize = MaxUploadFileSize;
            MaxDownloadFileSize = maxDownloadFileSite;
        }

        /// <summary>
        /// defines the maximum file size in bytes during upload
        /// </summary>
        public long MaxUploadFileSize { get; internal set; }

        public long MaxChunkedUploadFileSize { get; internal set; }

        /// <summary>
        /// defines the maximum file size in bytes during download
        /// </summary>
        public long MaxDownloadFileSize { get; internal set; }
    }
}