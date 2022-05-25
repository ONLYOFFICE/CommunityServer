using System;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    internal class DropBoxStorageProvider : GenericStorageProvider
    {
        public DropBoxStorageProvider()
            : base(new CachedServiceWrapper(new DropBoxStorageProviderService()))
        {
        }

        public override Uri GetFileSystemObjectUrl(string path, ICloudDirectoryEntry parent)
        {
            // get the filesystem
            var entry = GetFileSystemObject(path, parent);

            // get the download url
            var url = DropBoxStorageProviderService.GetDownloadFileUrlInternal(Session, entry);

            // get the right session
            var session = (DropBoxStorageProviderSession)Session;

            // generate the oauth url
            var svc = new OAuthService();
            url = svc.GetProtectedResourceUrl(url, session.Context, session.SessionToken as DropBoxToken, null, WebRequestMethodsEx.Http.Get);

            // go ahead
            return new Uri(url);
        }

        public override string GetFileSystemObjectPath(ICloudFileSystemEntry fsObject)
        {
            var path = DropBoxResourceIDHelpers.GetResourcePath(fsObject);
            return "/" + path;
        }
    }
}