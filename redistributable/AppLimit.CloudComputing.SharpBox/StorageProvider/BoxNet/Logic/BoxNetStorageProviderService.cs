using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet.Logic
{
    internal class BoxNetStorageProviderService : WebDavStorageProviderService
    {
        protected override string OnNameBase(string targetUrl, IStorageProviderService service, IStorageProviderSession session, string nameBase)
        {
            return nameBase.StartsWith("https") ? nameBase : nameBase.Replace("http", "https");
        }
    }
}