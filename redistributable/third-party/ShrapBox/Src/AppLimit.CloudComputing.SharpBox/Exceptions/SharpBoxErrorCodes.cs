namespace AppLimit.CloudComputing.SharpBox.Exceptions
{
    /// <summary>
    /// This enum contains all existing error codes
    /// which can be used in a SharpBoxException
    /// </summary>
    public enum SharpBoxErrorCodes
    {
        /// <summary>
        /// This error occurs when the cloud storage 
        /// service will be not available via network
        /// </summary>
        ErrorCouldNotContactStorageService          = 1,   
        
        /// <summary>
        /// This error occurs when a given path is not
        /// in the correct format, e.g. a directory is 
        /// needed but the path targets a files
        /// </summary>
        ErrorInvalidFileOrDirectoryName             = 2,        

        /// <summary>
        /// This error occurs when a file or path was 
        /// not found behind the given location
        /// </summary>
        ErrorFileNotFound                           = 3,        

        /// <summary>
        /// This error occurs when a cloud storage service
        /// provider was not registered for a specific
        /// configuration
        /// </summary>
        ErrorNoValidProviderFound                   = 4,        

        /// <summary>
        /// This error occurs when the creation of a 
        /// cloud storage service provider was not 
        /// possible. This can happens when the author of
        /// a provider is doing complex things in the
        /// constructor of the class
        /// </summary>
        ErrorCreateProviderInstanceFailed           = 5,        

        /// <summary>
        /// This error occurs when the give credentials, e.g.
        /// Password, UserName, ApplicationKey are wrong
        /// </summary>
        ErrorInvalidCredentialsOrConfiguration      = 6,  
      
        /// <summary>
        /// This error occures when one or more parameters 
        /// of the called function was wrong
        /// </summary>
        ErrorInvalidParameters                      = 7,

        /// <summary>
        /// This error occurs when the client can't request 
        /// the directory childs from the server
        /// </summary>
        ErrorCouldNotRetrieveDirectoryList          = 8,

        /// <summary>
        /// This error occurs when a create operation 
        /// failed
        /// </summary>
        ErrorCreateOperationFailed                  = 9,

        /// <summary>
        /// This error occurs when a configured limit 
        /// exceeds
        /// </summary>
        ErrorLimitExceeded                          = 10,

        /// <summary>
        /// This error occures when the storage limit of
        /// the cloud storage is exceeded
        /// </summary>
        ErrorInsufficientDiskSpace                  = 11,

        /// <summary>
        /// The datatransfer was interrupted from the
        /// application during a callback
        /// </summary>
        ErrorTransferAbortedManually                = 12,

        /// <summary>
        /// The operation needs an opened connection
        /// to the cloud storage, please call the 
        /// open method before!
        /// </summary>
        ErrorOpenedConnectionNeeded                = 13,

        /// <summary>
        /// This error occurs when consumer
        /// Key/Secret pair are not valid
        /// </summary>
        ErrorInvalidConsumerKeySecret = 14
    }
}
