using System;
using System.Text.RegularExpressions;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive
{
    internal static class SkyDriveConstants
    {
        //authorization
        public const String OAuth20AuthUrl = "https://login.live.com/oauth20_authorize.srf";
        public const String OAuth20TokenUrl = "https://login.live.com/oauth20_token.srf";
        public const String DefaultScopes = "wl.signin wl.skydrive_update wl.offline_access";
        public const String DefaultRedirectUri = "https://login.live.com/oauth20_desktop.srf"; //for desktop and mobile apps only

        //property keys
        public const String SerializedDataKey = "serializedData";
        public const String UploadLocationKey = "uploadLocation";
        public const String ParentIDKey = "parentID";
        public const String TimestampKey = "timestamp";
        public const String InnerIDKey = "resourceID";

        //access paths
        public const String BaseAccessUrl = "https://apis.live.net/v5.0";
        public const String RootAccessUrl = BaseAccessUrl + "/me/skydrive";
        public const String FilesAccessUrlFormat = BaseAccessUrl + "/{0}/files";

        //misc
        public static readonly String[] SupportedFileExtensions = new[] { "" };
        public static readonly Regex ResourceIDRegex = new Regex(@"^(file|folder|photo|album)\.[!\.\w]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex RootIDRegex = new Regex(@"^folder.\w+$");

    }
}