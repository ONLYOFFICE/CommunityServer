namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    internal class GoogleDocsConstants
    {
        public const string TokenGoogleDocsAppKey = "TokenGoogleDocsAppKey";
        public const string TokenGoogleDocsAppSecret = "TokenGoogleDocsAppSecret";
        public const string TokenGoogleDocsUsername = "TokenGoogleDocsUsername";
        public const string TokenGoogleDocsPassword = "TokenGoogleDocsPassword";

        public const string EtagProperty = "GoogleDocsEtag";
        public const string KindProperty = "GoogleDocsKind";
        public const string ResCreateMediaProperty = "GoogleDocsResumableCreateMedia";
        public const string ResEditMediaProperty = "GoogleDocsResumableEditMedia";
        public const string DownloadUrlProperty = "GoogleDocsDownload";
        public const string ParentsProperty = "GoogleDocsParents";

        //api access points
        public const string GoogleDocsBaseUrl = "https://drive.google.com";
        public const string GoogleDocsFeedUrl = GoogleDocsBaseUrl + "/feeds/default/private/full";
        public const string GoogleDocsResourceUrlFormat = GoogleDocsFeedUrl + "/{0}";
        public const string GoogleDocsContentsUrlFormat = GoogleDocsResourceUrlFormat + "/contents";
        public const string GoogleDocsBatchUrl = GoogleDocsFeedUrl + "/batch";

        //Google Docs rss feed namespaces
        public const string AtomNamespace = "http://www.w3.org/2005/Atom";
        public const string GdNamespace = "http://schemas.google.com/g/2005";
        public const string BatchNamespace = "http://schemas.google.com/gdata/batch";

        //Google Docs schemas
        public const string SchemeParent = "http://schemas.google.com/docs/2007#parent";
        public const string SchemeKind = "http://schemas.google.com/g/2005#kind";
        public const string SchemeFolder = "http://schemas.google.com/docs/2007#folder";
        public const string SchemeResCreateMedia = "http://schemas.google.com/g/2005#resumable-create-media";
        public const string SchemeResEditMedia = "http://schemas.google.com/g/2005#resumable-edit-media";

        public const string RootFolderId = "folder_root";
        public const string RootResCreateMediaUrl = "https://drive.google.com/feeds/upload/create-session/default/private/full/folder%3Aroot/contents";

        public const string CallbackDefaultUrl = "http://sharpbox.codeplex.com";

        public const string ResourceIdRegexPattern = @"^(document|drawing|file|folder|pdf|presentation|spreadsheet)_[-\w]+$";

        public const int UploadChunkLength = 524288;

        //OAuth 1.0
        public const string OAuthGetRequestUrl = "https://www.google.com/accounts/OAuthGetRequestToken";
        public const string OAuthAuthorizeUrl = "https://www.google.com/accounts/OAuthAuthorizeToken";
        public const string OAuthGetAccessUrl = "https://www.google.com/accounts/OAuthGetAccessToken";
        public const string BasicScopeUrl = "https://docs.google.com/feeds/ https://spreadsheets.google.com/feeds/ https://docs.googleusercontent.com/";
    }
}