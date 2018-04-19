namespace SelectelSharp.Headers
{
    internal static class HeaderKeys
    {
        internal const string DateFormat = "ddd, dd MMM yyyy HH:mm:ss 'GMT'";

        internal const string AcceptJson = "application/json";
        internal const string Accept = "Accept";

        internal const string Destination = "Destination";

        internal const string XAccountBytesUsed = "X-Account-Bytes-Used";
        internal const string XAccountContainerCount = "X-Account-Container-Count";
        internal const string XAccountObjectCount = "X-Account-Object-Count";

        internal const string XTransferedBytes = "X-Transfered-Bytes";
        internal const string XReceivedBytes = "X-Received-Bytes";

        internal const string XContainerObjectCount = "X-Container-Object-Count";
        internal const string XContainerBytesUsed = "X-Container-Bytes-Used";

        internal const string XContainerMetaPrefix = "X-Container-Meta-";
        internal const string XContainerMetaType = "X-Container-Meta-Type";
        internal const string XContainerMetaGallerySecret = "X-Container-Meta-Gallery-Secret";
        internal const string XContainerDomains = "X-Container-Domains";

        internal const string CacheControl = "Cache-Control";
        internal const string Expires = "Expires";
        internal const string Origin = "Origin";
        internal const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        internal const string AccessControlMaxAge = "Access-Control-Max-Age";
        internal const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        internal const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        internal const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";
        internal const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        internal const string AccessControlRequestMethod = "Access-Control-Request-Method";
        internal const string ContentDisposition = "Content-Disposition";
        internal const string StrictTransportSecurity = "Strict-Transport-Security";

        internal const string XDeleteAt = "X-Delete-At";
        internal const string XDeleteAfter = "X-Delete-After";

        internal const string ContentLenght = "Content-Length";
        internal const string ContentType = "Content-Type";
        internal const string ETag = "ETag";

        internal const string LastModified = "Last-Modified";
        internal const string Date = "Date";

        #region Authorization Headers

        internal const string XAuthUser = "X-Auth-User";
        internal const string XAuthKey = "X-Auth-Key";
        internal const string XStorageUrl = "X-Storage-Url";
        internal const string XAuthToken = "X-Auth-Token";
        internal const string XExpireAuthToken = "X-Expire-Auth-Token";

        #endregion

        #region File Conditional Request Headers

        internal const string IfMatch = "If-Match";
        internal const string IfNoneMatch = "If-None-Match";
        internal const string IfModifiedSince = "If-Modified-Since";
        internal const string IfUnmodifiedSince = "If-Unmodified-Since";

        #endregion

        internal const string ExtractId = "Extract-Id";

        #region Link Headers

        internal const string XObjectMetaLocation = "X-Object-Meta-Location";
        internal const string XObjectMetaDeleteAt = "X-Object-Meta-Delete-At";
        internal const string XObjectMetaLinkKey = "X-Object-Meta-Link-Key";

        #endregion
    }
}
