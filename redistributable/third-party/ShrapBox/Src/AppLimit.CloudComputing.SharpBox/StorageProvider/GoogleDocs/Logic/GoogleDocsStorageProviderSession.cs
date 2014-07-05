using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs.Logic
{
    internal class GoogleDocsStorageProviderSession : IStorageProviderSession
    {
        public GoogleDocsStorageProviderSession(IStorageProviderService service, ICloudStorageConfiguration configuration, OAuthConsumerContext context, ICloudStorageAccessToken token)
        {
            Service = service;
            ServiceConfiguration = configuration;
            Context = context;
            SessionToken = token;
        }

        public OAuthConsumerContext Context { get; private set; }

        public ICloudStorageAccessToken SessionToken { get; private set; }

        public IStorageProviderService Service { get; private set; }

        public ICloudStorageConfiguration ServiceConfiguration { get; private set; }
    }
}