using System;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    /// <summary>
    /// This enum defines possible API versions DropBox offers to 
    /// their customers
    /// </summary>
    public enum DropBoxAPIVersion
    {
        /// <summary>
        /// Version 0 of the DropBox API
        /// </summary>
        V0 = 0,

        /// <summary>
        /// Version 1 of the DropBox API
        /// </summary>
        V1 = 1,

        /// <summary>
        /// The current stable version which will be used by SharpBox by default
        /// </summary>
        Stable = 1
    };

    /// <summary>
    /// DropBoxConfiguration conains the default access information for the DropBox
    /// storage and synchronization services. This class implements from the 
    /// ICloudStorageConfiguration interface and can be used with an instance of CloudStorage    
    /// </summary>
    public class DropBoxConfiguration : ICloudStorageConfiguration
    {
        /// <summary>
        /// This method creates and returns the url which has to be used to get a access token 
        /// during the OAuth based authorization process
        /// </summary>
        public Uri RequestTokenUrl
        {
            get { return new Uri(DropBoxStorageProviderService.GetUrlString(DropBoxStorageProviderService.DropBoxOAuthRequestToken, this)); }
        }

        /// <summary>
        /// This method creates and returns the url which has to be used to get a access token 
        /// during the OAuth based authorization process
        /// </summary>
        public Uri AccessTokenUrl
        {
            get { return new Uri(DropBoxStorageProviderService.GetUrlString(DropBoxStorageProviderService.DropBoxOAuthAccessToken, this)); }
        }

        /// <summary>
        /// This method creates and returns the url which can be used in the browser
        /// to authorize the end user
        /// </summary>
        public Uri AuthorizationTokenUrl
        {
            get { return new Uri(DropBoxStorageProviderService.GetUrlString(DropBoxStorageProviderService.DropBoxOAuthAuthToken, null)); }
        }

        /// <summary>
        /// This field contains the callback URL which will be used in the oAuth 
        /// request. Default will be sharpbox.codeplex.com 
        /// </summary>
        public Uri AuthorizationCallBack = new Uri("http://sharpox.codeplex.com");

        /// <summary>
        /// The APIVersion which will be used for communication with dropbox. As default 
        /// the stable version is set!
        /// </summary>
        public DropBoxAPIVersion APIVersion { get; set; }

        /// <summary>
        /// Constructor of a dropbox configuration
        /// </summary>
        public DropBoxConfiguration()
            : this(new Uri(DropBoxStorageProviderService.DropBoxBaseUrl))
        {
            APIVersion = DropBoxAPIVersion.Stable;
        }

        /// <summary>
        /// Allows to create the dopbox configuration with a special 
        /// service provider location
        /// </summary>
        /// <param name="serviceLocator"></param>
        public DropBoxConfiguration(Uri serviceLocator)
        {
            ServiceLocator = serviceLocator;
        }

        /// <summary>
        /// This method generates an instance of the default dropbox configuration
        /// </summary>
        /// <returns>Instance of the DropBoxConfiguration-Class with default settings</returns>
        public static DropBoxConfiguration GetStandardConfiguration()
        {
            return new DropBoxConfiguration();
        }

        /// <summary>
        /// If true this value indicates the all ssl connection are valid. This means also ssl connection
        /// with an invalid certificate will be accepted.
        /// </summary>
        public bool TrustUnsecureSSLConnections
        {
            get { return false; }
        }

        /// <summary>
        /// The limits of dropbox
        /// </summary>
        public CloudStorageLimits Limits
        {
            get { return new CloudStorageLimits(150*1024*1024, -1) { MaxChunkedUploadFileSize = -1 }; }
        }

        /// <summary>
        /// Gets the currently used dropbox url
        /// </summary>
        public Uri ServiceLocator { get; private set; }
    }
}