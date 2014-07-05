using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs;
using NUnit.Framework;

namespace AppLimit.CloudComputing.SharpBox.Tests.StorageProviderTests.GoogleDrive
{
    [TestFixture]
    public class GoogleDocsProviderTests : StorageProviderTestsBase
    {
        protected override CloudStorage CreateStorage()
        {
            var storage = new CloudStorage();
            var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.Google);
            storage.Open(config, GetAccessToken());
            return storage;
        }

        protected override ICloudStorageAccessToken GetAccessToken()
        {
            return GoogleDocsAuthorizationHelper.BuildToken("1/wARng6YXxNKBtSwCgCZSGac9G2kjSffXIhS92_gnr-w", "_lvJSY84FIZ0mwWEF65K9Eu6", "auth.teamlab.info", "Vb4PZr05O3Czdyyn0ItMENOj");
        }
    }
}
