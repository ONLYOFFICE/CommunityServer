using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

#if !WINDOWS_PHONE && !MONODROID
using System.Net.Security;
#endif

using AppLimit.CloudComputing.SharpBox.Common;
using AppLimit.CloudComputing.SharpBox.Exceptions;

using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet;

#if !WINDOWS_PHONE && !MONODROID
using System.Net;
using AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS;
using AppLimit.CloudComputing.SharpBox.StorageProvider.FTP;
#endif



namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// The class CloudStorage implements all needed methods to get access
    /// to a supported cloud storage infrastructure. The following vendors
    /// are currently supported:
    /// 
    ///  - DropBox
    ///  
    /// </summary>
    public partial class CloudStorage : ICloudStorageProvider, ICloudStorageAsyncInterface, ICloudStoragePublicAPI
    {
        // some information for token storage
        internal const String TokenProviderConfigurationType = "TokenProvConfigType";
        internal const String TokenCredentialType = "TokenCredType";
        internal const String TokenMetadataPrefix = "TokenMetadata";

        #region Member Declarations

        private ICloudStorageProviderInternal _provider;
        private ICloudStorageConfiguration _configuration;
        private readonly Dictionary<String, Type> _configurationProviderMap;

        #endregion

        #region Constructure and logistics

        /// <summary>
        /// returns the currently setted access token 
        /// </summary>
        public ICloudStorageAccessToken CurrentAccessToken 
        {
            get
            {
                if (_provider == null)
                    return null;
                else
                    return _provider.CurrentAccessToken;
            }
        }

        /// <summary>
        /// Allows access to the current configuration which was used in the open call
        /// </summary>
        public ICloudStorageConfiguration CurrentConfiguration
        {
            get
            {
                return _configuration;
            }
        }

        /// <summary>
        /// The default constructure for a cloudstorage 
        /// </summary>
        public CloudStorage()
        {
            // build config provider
            _configurationProviderMap = new Dictionary<String, Type>();

            // register provider
            RegisterStorageProvider(typeof(DropBoxConfiguration), typeof(DropBoxStorageProvider));			
            RegisterStorageProvider(typeof(WebDavConfiguration), typeof(WebDavStorageProvider));
            RegisterStorageProvider(typeof(BoxNetConfiguration), typeof(BoxNetStorageProvider));
#if !WINDOWS_PHONE && !MONODROID
            RegisterStorageProvider(typeof(FtpConfiguration), typeof(FtpStorageProvider));
            RegisterStorageProvider(typeof(CIFSConfiguration), typeof(CIFSStorageProvider));
            RegisterStorageProvider(typeof (GoogleDocsConfiguration), typeof (GoogleDocsStorageProvider));
            RegisterStorageProvider(typeof (StorageProvider.SkyDrive.SkyDriveConfiguration), typeof (StorageProvider.SkyDrive.Logic.SkyDriveStorageProvider));
#endif
        }

        /// <summary>
        /// copy ctor
        /// </summary>
        /// <param name="src"></param>
        public CloudStorage(CloudStorage src)
            : this(src, true)
        { }

        /// <summary>
        /// copy ctor 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="OpenIfSourceWasOpen"></param>
        public CloudStorage(CloudStorage src, Boolean OpenIfSourceWasOpen)
            : this()
        {
            // copy all registered provider from src
            _configurationProviderMap = src._configurationProviderMap;

            // open the provider
            if (src.IsOpened && OpenIfSourceWasOpen)
                Open(src._configuration, src.CurrentAccessToken);
            else
                _configuration = src._configuration;
        }


        /// <summary>
        /// This method allows to register a storage provider for a specific configuration
        /// type
        /// </summary>
        /// <param name="configurationType">
        /// A <see cref="Type"/>
        /// </param>
        /// <param name="storageProviderType">
        /// A <see cref="Type"/>
        /// </param>
        /// <returns>
        /// A <see cref="Boolean"/>
        /// </returns>
        public Boolean RegisterStorageProvider(Type configurationType, Type storageProviderType)
        {
            // do double check
            if (_configurationProviderMap.ContainsKey(configurationType.FullName))
                return false;

            // register
            _configurationProviderMap.Add(configurationType.FullName, storageProviderType);

            // go ahead
            return true;
        }

#if !WINDOWS_PHONE && !MONODROID
        /// <summary>
        /// Ignores all invalid ssl certs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        static bool ValidateAllServerCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
#endif

        #endregion

        #region Base Functions

        /// <summary>
        /// True when a the connection to the Cloudstorage will be established
        /// </summary>
        public Boolean IsOpened { get; private set; }
   
        /// <summary>
        /// Calling this method with vendor specific configuration settings and token based credentials
        /// to get access to the cloud storage. The following exceptions are possible:
        /// 
        /// - System.UnauthorizedAccessException when the user provides wrong credentials
        /// - AppLimit.CloudComputing.SharpBox.Exceptions.SharpBoxException when something 
        ///   happens during cloud communication
        /// 
        /// </summary>
        /// <param name="configuration">Vendor specific configuration of the cloud storage</param>
        /// <param name="token">Vendor specific authorization token</param>        
        /// <returns>A valid access token or null</returns>
        public ICloudStorageAccessToken Open(ICloudStorageConfiguration configuration, ICloudStorageAccessToken token)
        {
            // check state
            if (IsOpened)
                return null;

            // ensures that the right provider will be used
            SetProviderByConfiguration(configuration);
            
            // save the configuration
            _configuration = configuration;

#if !WINDOWS_PHONE && !MONODROID
            // verify the ssl config
            if (configuration.TrustUnsecureSSLConnections)
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateAllServerCertificates;

            // update the max connection settings 
            ServicePointManager.DefaultConnectionLimit = 250;

            // disable the not well implementes Expected100 header settings
            ServicePointManager.Expect100Continue = false;
#endif

            // open the cloud connection                                    
            token = _provider.Open(configuration, token);

            // ok without Exception every is good
            IsOpened = true;

            // return the token
            return token;
        }
        
        /// <summary>
        /// This method will close the connection to the cloud storage system
        /// </summary>
        public void Close()
        {
            if (_provider == null)
                return;

            _provider.Close();

            IsOpened = false;
        }

        /// <summary>
        /// This method returns access to the root folder of the storage system
        /// </summary>
        /// <returns>Reference to the root folder of the storage system</returns>
        public ICloudDirectoryEntry GetRoot()
        {
            return _provider == null ? null : _provider.GetRoot();
        }

        /// <summary>
        /// This method returns access ro a specific subfolder or file addressed via 
        /// unix like file system path, e.g. /Public/SubFolder/SubSub
        /// 
        /// Valid Exceptions are:
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="name">The path to the target subfolder</param>
        /// <param name="parent">The startfolder for the searchpath</param>
        /// <returns></returns>
        public ICloudFileSystemEntry GetFileSystemObject(String name, ICloudDirectoryEntry parent)
        {
            return _provider == null ? null : _provider.GetFileSystemObject(name, parent);
        }

        /// <summary>
        /// This method creates a child folder in the give folder
        /// of the cloud storage
        /// </summary>
        /// <param name="name">Name of the new folder</param>
        /// <param name="parent">Parent folder</param>
        /// <returns>Reference to the created folder</returns>
        public ICloudDirectoryEntry CreateFolder(String name, ICloudDirectoryEntry parent)
        {
            return _provider == null ? null : _provider.CreateFolder(name, parent);
        }

        /// <summary>
        /// This method removes a file or a directory from 
        /// the cloud storage
        /// </summary>
        /// <param name="fsentry">Reference to the filesystem object which has to be removed</param>
        /// <returns>Returns true or false</returns>
        public bool DeleteFileSystemEntry(ICloudFileSystemEntry fsentry)
        {
            return _provider != null && _provider.DeleteFileSystemEntry(fsentry);
        }

        /// <summary>
        /// This method moves a file or a directory from its current
        /// location to a new onw
        /// </summary>
        /// <param name="fsentry">Filesystem object which has to be moved</param>
        /// <param name="newParent">The new location of the targeted filesystem object</param>
        /// <returns></returns>
        public bool MoveFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _provider == null ? false : _provider.MoveFileSystemEntry(fsentry, newParent);
        }

        /// <summary>
        ///  This method copy a file or a directory from its current
        /// location to a new onw
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public bool CopyFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _provider == null ? false : _provider.CopyFileSystemEntry(fsentry, newParent);
        }

        /// <summary>
        /// This mehtod allows to perform a server side renam operation which is basicly the same
        /// then a move operation in the same directory
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameFileSystemEntry(ICloudFileSystemEntry fsentry, String newName)
        {
            return _provider == null ? false : _provider.RenameFileSystemEntry(fsentry, newName);
        }

        /// <summary> 
        /// This method creates a new file object in the cloud storage. Use the GetContentStream method to 
        /// get a .net stream which usable in the same way then local stream are usable.
        /// </summary>
        /// <param name="parent">Link to the parent container, null means the root directory</param>
        /// <param name="Name">The name of the targeted file</param>        
        /// <returns></returns>
        public ICloudFileSystemEntry CreateFile(ICloudDirectoryEntry parent, String Name)
        {
            // pass through the provider
            return _provider == null ? null : _provider.CreateFile(parent, Name);
        }
        /// <summary>
        /// This method returns the direct URL to a specific file system object,
        /// e.g. a file or folder
        /// </summary>
        /// <param name="path">A relative path to the file</param>
        /// <param name="parent">A reference to the parent of the path</param>
        /// <returns></returns>
        public Uri GetFileSystemObjectUrl(String path, ICloudDirectoryEntry parent)
        {
            // pass through the provider
            return _provider == null ? null : _provider.GetFileSystemObjectUrl(path, parent);
        }

        /// <summary>
        /// Returns the path of the targeted object
        /// </summary>
        /// <param name="fsObject"></param>
        /// <returns></returns>
        public String GetFileSystemObjectPath(ICloudFileSystemEntry fsObject)
        {
            // pass through the provider
            return _provider == null ? null : _provider.GetFileSystemObjectPath(fsObject);
        }

        #endregion

        #region AccessTokenHandling

        /// <summary>
        /// This method allows to store a security token into a serialization stream
        /// </summary>        
        /// <param name="token"></param>
        /// <returns></returns>
        public Stream SerializeSecurityToken(ICloudStorageAccessToken token)
        {
            return SerializeSecurityToken(token, null);
        }

        /// <summary>
        /// This method allows to store a security token into a serialization stream and 
        /// makes it possible to store a couple of meta data as well
        /// </summary>
        /// <param name="token"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public Stream SerializeSecurityToken(ICloudStorageAccessToken token, Dictionary<String, String> additionalMetaData)
        {
            if (!IsOpened)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorOpenedConnectionNeeded);

            return SerializeSecurityTokenEx(token, _configuration.GetType(), additionalMetaData);
        }        

        /// <summary>
        /// This method can be used for serialize a token without have the connection opened :-)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configurationType"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public Stream SerializeSecurityTokenEx(ICloudStorageAccessToken token, Type configurationType, Dictionary<String, String> additionalMetaData)
        {
            var items = new Dictionary<String, String>();
            var stream = new MemoryStream();
            var serializer = new DataContractSerializer(items.GetType());

            // add the metadata 
            if (additionalMetaData != null)
            {
                foreach (KeyValuePair<String, string> kvp in additionalMetaData)
                {
                    items.Add(TokenMetadataPrefix + kvp.Key, kvp.Value);
                }
            }

            // save the token into our list
            StoreToken(items, token, configurationType);

            // write the data to stream
            serializer.WriteObject(stream, items);

            // go to start
            stream.Seek(0, SeekOrigin.Begin);

            // go ahead
            return stream;
        }

        /// <summary>
        /// This method stores a token into a file
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configurationType"></param>
        /// <param name="additionalMetaData"></param>
        /// <param name="fileName"></param>
        public void SerializeSecurityTokenEx(ICloudStorageAccessToken token, Type configurationType, Dictionary<String, String> additionalMetaData, String fileName)
        {
            using(FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (Stream ts = SerializeSecurityTokenEx(token, configurationType, additionalMetaData))
                {
                    // copy
                    StreamHelper.CopyStreamData(this, ts, fs, null, null);                    
                }

                // flush
                fs.Flush();

                // close
                fs.Close();
            }
        }

        /// <summary>
        /// This method allows to serialize a security token into a base64 string 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configurationType"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public String SerializeSecurityTokenToBase64Ex(ICloudStorageAccessToken token, Type configurationType, Dictionary<String, String> additionalMetaData)
        {
            using(Stream tokenStream = SerializeSecurityTokenEx(token, configurationType, additionalMetaData))
            {
                using(MemoryStream memStream = new MemoryStream())
                {
                    // copy to memory
                    StreamHelper.CopyStreamData(this, tokenStream, memStream, null, null);

                    // reset
                    memStream.Position = 0;

                    // convert 
                    return Convert.ToBase64String(memStream.ToArray());
                }
            }
        }

        /// <summary>
        /// This method allows to load a token from a previously generated stream
        /// </summary>
        /// <param name="tokenStream"></param>        
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream)
        {
            Dictionary<String, String> metadata = null;
            return DeserializeSecurityToken(tokenStream, out metadata);
        }

        /// <summary>
        /// This method restores a tokene from file 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityTokenEx(String fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return DeserializeSecurityToken(fs);
            }             
        }

        /// <summary>
        /// This method allows to load a token from a previously generated stream 
        /// and the attached metadata
        /// </summary>
        /// <param name="tokenStream"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream, out Dictionary<String, String> additionalMetaData)
        {
            // load the data in our list            
            var serializer = new DataContractSerializer(typeof(Dictionary<String, String>));

            // check the type
            Object obj = serializer.ReadObject(tokenStream);
            if (!obj.GetType().Equals(typeof(Dictionary<String, String>)))
#if SILVERLIGHT || MONODROID || MONOTOUCH
                throw new Exception("A List<String> was expected");
#else
                throw new InvalidDataException("A List<String> was expected");
#endif

            // evaluate the right provider
            Dictionary<String, String> tokendata = (Dictionary<String, String>)obj;

            // find the right provider by typename
            ICloudStorageProviderInternal provider = GetProviderByConfigurationTypeName(tokendata[TokenProviderConfigurationType]);

            // set the output parameter
            additionalMetaData = new Dictionary<string,string>();

            // fill the metadata
            foreach (KeyValuePair<String, string> kvp in tokendata)
            {
                if (kvp.Key.StartsWith(TokenMetadataPrefix))
                {
                    additionalMetaData.Add(kvp.Key.Remove(0, TokenMetadataPrefix.Length), kvp.Value);
                }
            }

            // build the token            
            return provider.LoadToken(tokendata);
        }

        /// <summary>
        ///  This methd allows to deserialize a base64 tokenstring
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityTokenFromBase64(String tokenString)
        {
            Dictionary<String, String> additionalMetaData = null;
            return DeserializeSecurityTokenFromBase64(tokenString, out additionalMetaData);
        }

        /// <summary>        
        ///  This methd allows to deserialize a base64 tokenstring
        /// </summary>
        /// <param name="tokenString"></param>
        /// <param name="additionalMetaData"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken DeserializeSecurityTokenFromBase64(String tokenString, out Dictionary<String, String> additionalMetaData)
        {
            // convert base64 to byte array
            Byte[] data = Convert.FromBase64String(tokenString);

            // create a token stream 
            using (MemoryStream tokenStream = new MemoryStream(data))
            {
                // read the token stream
                return DeserializeSecurityToken(tokenStream, out additionalMetaData);
            }            
        }

        /// <summary>
        /// This method stores the content of an access token in to 
        /// a list of string. This list can be serialized.
        /// </summary>
        /// <param name="tokendata">Target list</param>
        /// <param name="token">the token</param>
        /// <param name="configurationType">type of configguration which is responsable for the token</param>
        internal void StoreToken(Dictionary<String, String> tokendata, ICloudStorageAccessToken token, Type configurationType)
        {            
            // add the configuration information into the 
            tokendata.Add(TokenProviderConfigurationType, configurationType.FullName);
            tokendata.Add(TokenCredentialType, token.GetType().FullName);

            // get the provider by toke
            ICloudStorageProviderInternal provider = GetProviderByConfigurationTypeName(configurationType.FullName);

            // store all the other information to tokendata
            provider.StoreToken(tokendata, token);
        }

        /// <summary>
        /// This method generated a access token from the given data 
        /// string list
        /// </summary>
        /// <param name="tokendata">the string list</param>
        /// <returns>The unserialized token</returns>
        internal ICloudStorageAccessToken LoadToken(Dictionary<String,String> tokendata)
        {
            return _provider.LoadToken(tokendata);
        }

        #endregion

        #region Helper

        private static ICloudStorageProviderInternal CreateProviderByType(Type providerType)
        {
            ICloudStorageProviderInternal provider;

            try
            {
                provider = Activator.CreateInstance(providerType) as ICloudStorageProviderInternal;
            }
            catch (Exception e)
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateProviderInstanceFailed, e);
            }

            if (provider == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateProviderInstanceFailed);

            return provider;
        }

        private void SetProviderByConfiguration(ICloudStorageConfiguration configuration)
        {
            // check
            if (configuration == null && _provider == null)
                throw new InvalidOperationException("It's only allowed to set the configuration parameter to null when a provider was set before");

            // read out the right provider type
            SetProviderByConfigurationTypeName(configuration.GetType().FullName);
        }

        private void SetProviderByConfigurationTypeName(String TypeName)
        {
            _provider = GetProviderByConfigurationTypeName(TypeName);
        }        

        private ICloudStorageProviderInternal GetProviderByConfigurationTypeName(String TypeName)
        {
            // read out the right provider type
            Type providerType = null;
            if (!_configurationProviderMap.TryGetValue(TypeName, out providerType))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorNoValidProviderFound);

            // build up the provider
            return CreateProviderByType(providerType);
        }
        #endregion
    }
}
