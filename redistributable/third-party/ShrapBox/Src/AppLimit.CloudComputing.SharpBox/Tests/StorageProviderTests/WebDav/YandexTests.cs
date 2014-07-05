using AppLimit.CloudComputing.SharpBox.StorageProvider;
using NUnit.Framework;

namespace AppLimit.CloudComputing.SharpBox.Tests.StorageProviderTests.WebDav
{
    [TestFixture]
    public class YandexTests : StorageProviderTestsBase
    {
        protected override CloudStorage CreateStorage()
        {
            var storage = new CloudStorage();
            var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.Yandex);
            storage.Open(config, GetAccessToken());
            return storage;
        }

        protected override ICloudStorageAccessToken GetAccessToken()
        {
            return new GenericNetworkCredentials {UserName = "---", Password = "---"};
        }
    }
}
