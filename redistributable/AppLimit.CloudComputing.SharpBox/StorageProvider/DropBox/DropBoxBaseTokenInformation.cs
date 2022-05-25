namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    /// <summary>
    /// The base class of all credentials
    /// </summary>
    public class DropBoxBaseTokenInformation
    {
        /// <summary>
        /// ConsumerKey of the DropBox application, provided by the 
        /// DropBox-Developer-Portal
        /// </summary>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// ConsumerSecret of the DropBox application, provided by the 
        /// DropBox-Developer-Portal
        /// </summary>
        public string ConsumerSecret { get; set; }
    }
}