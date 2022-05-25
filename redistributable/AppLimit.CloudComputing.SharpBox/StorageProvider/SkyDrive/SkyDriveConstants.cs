using System.Text.RegularExpressions;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive
{
    internal static class SkyDriveConstants
    {
        //authorization
        public const string OAuth20AuthUrl = "https://login.live.com/oauth20_authorize.srf";
        public const string OAuth20TokenUrl = "https://login.live.com/oauth20_token.srf";
        public const string DefaultScopes = "wl.signin wl.skydrive_update wl.offline_access";
        public const string DefaultRedirectUri = "https://login.live.com/oauth20_desktop.srf"; //for desktop and mobile apps only

        //property keys
        public const string SerializedDataKey = "serializedData";
        public const string UploadLocationKey = "uploadLocation";
        public const string ParentIDKey = "parentID";
        public const string TimestampKey = "timestamp";
        public const string InnerIDKey = "resourceID";

        //access paths
        public const string BaseAccessUrl = "https://apis.live.net/v5.0";
        public const string RootAccessUrl = BaseAccessUrl + "/me/skydrive";
        public const string FilesAccessUrlFormat = BaseAccessUrl + "/{0}/files";

        //misc
        public static readonly string[] SupportedFileExtensions = new[] { "" };
        public static readonly Regex ResourceIDRegex = new Regex(@"^(file|folder|photo|album)\.[!\.\w]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex RootIDRegex = new Regex(@"^folder.\w+$");

    }
}