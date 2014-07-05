using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider;
using NUnit.Framework;

namespace AppLimit.CloudComputing.SharpBox.Tests.StorageProviderTests.WebDav
{
    [TestFixture]
    public  class BoxNetTests : StorageProviderTestsBase
    {
        protected override CloudStorage CreateStorage()
        {
            var storage = new CloudStorage();
            var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.BoxNet);
            storage.Open(config, GetAccessToken());
            return storage;
        }

        protected override ICloudStorageAccessToken GetAccessToken()
        {
            return new GenericNetworkCredentials { UserName = "kosov.maxim@gmail.com", Password = "111111" };
        }
    }
}
