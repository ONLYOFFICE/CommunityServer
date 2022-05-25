namespace AppLimit.CloudComputing.SharpBox.StorageProvider.API
{
    /// <summary> 
    /// This interface is part of the managed storage provider API 
    /// and implements an authenticated session
    /// </summary>
    public interface IStorageProviderSession
    {
        /// <summary>
        /// The generated access token for this 
        /// session
        /// </summary>
        ICloudStorageAccessToken SessionToken { get; }

        /// <summary>
        /// The associated service which is connected
        /// with this session
        /// </summary>
        IStorageProviderService Service { get; }

        /// <summary>
        /// A valid cloud storage service configuration
        /// </summary>
        ICloudStorageConfiguration ServiceConfiguration { get; }
    }
}