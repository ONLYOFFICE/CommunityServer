using System;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic
{
    internal class DropBoxStorageProviderSession : IStorageProviderSession
    {
        public OAuthConsumerContext Context { get; set; }

        public Boolean SandBoxMode { get; set; }

        public DropBoxStorageProviderSession(DropBoxToken token, DropBoxConfiguration config, OAuthConsumerContext consumerContext, IStorageProviderService service)
        {
            SessionToken = token;
            ServiceConfiguration = config;
            Service = service;
            Context = consumerContext;
        }

        #region IStorageProviderSession Members

        public ICloudStorageAccessToken SessionToken { get; private set; }

        public IStorageProviderService Service { get; private set; }

        public ICloudStorageConfiguration ServiceConfiguration { get; private set; }

        #endregion
    }
}