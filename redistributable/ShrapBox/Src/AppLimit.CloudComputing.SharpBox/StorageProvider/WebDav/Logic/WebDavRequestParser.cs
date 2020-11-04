using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml.Linq;
using AppLimit.CloudComputing.SharpBox.Common.Extensions;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic
{
    internal delegate String NameBaseFilterCallback(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String nameBase);

    internal class WebDavRequestResult
    {
        public BaseFileEntry Self { get; set; }
        public List<BaseFileEntry> Childs { get; set; }

        public WebDavRequestResult()
        {
            Childs = new List<BaseFileEntry>();
        }
    }

    internal class WebDavRequestParser
    {
        private const String DavNamespace = "DAV:";
        private const String HttpOk = "HTTP/1.1 200 OK";

        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session, NameBaseFilterCallback callback)
        {
            var config = session.ServiceConfiguration as WebDavConfiguration;
            var results = new WebDavRequestResult();

            var queryLessUri = HttpUtilityEx.GetPathAndQueryLessUri(config.ServiceLocator).ToString().TrimEnd('/');
            var decodedTargetUrl = HttpUtility.UrlDecode(targetUrl);
            string s;
            using (var streamReader = new StreamReader(data))
            {
                s = streamReader.ReadToEnd();
            }
            //todo:
            var xDoc = XDocument.Load(new StringReader(s.Replace("d:d:", "d:")));
            var responses = xDoc.Descendants(XName.Get("response", DavNamespace));

            foreach (var response in responses)
            {
                var isHidden = false;
                var isDirectory = false;
                var lastModified = DateTime.Now;
                long contentLength = 0;

                var href = response.Element(XName.Get("href", DavNamespace)).ValueOrEmpty();
                var propstats = response.Descendants(XName.Get("propstat", DavNamespace));
                foreach (var propstat in propstats)
                {
                    var prop = propstat.Element(XName.Get("prop", DavNamespace));
                    var status = propstat.Element(XName.Get("status", DavNamespace)).ValueOrEmpty();

                    if (!status.Equals(HttpOk) || prop == null) continue;

                    var strLastModified = prop.Element(XName.Get("getlastmodified", DavNamespace)).ValueOrEmpty();
                    var strContentLength = prop.Element(XName.Get("getcontentlength", DavNamespace)).ValueOrEmpty();
                    var resourceType = prop.Element(XName.Get("resourcetype", DavNamespace));
                    var strIsHidden = prop.Element(XName.Get("ishidden", DavNamespace)).ValueOrEmpty();

                    if (!String.IsNullOrEmpty(strIsHidden))
                    {
                        int code;
                        if (!int.TryParse(strIsHidden, out code))
                            code = 0;
                        isHidden = Convert.ToBoolean(code);
                    }
                    if (resourceType != null && resourceType.Element(XName.Get("collection", DavNamespace)) != null)
                        isDirectory = true;
                    if (!String.IsNullOrEmpty(strContentLength))
                        contentLength = Convert.ToInt64(strContentLength);
                    if (!String.IsNullOrEmpty(strLastModified) && DateTime.TryParse(strLastModified, out lastModified))
                    {
                        lastModified = lastModified.ToUniversalTime();
                    }
                }

                //entry not to be encluded
                if (isHidden) continue;

                var nameBase = href;

                if (callback != null)
                    nameBase = callback(targetUrl, service, session, nameBase);

                String nameBaseForSelfCheck;

                if (nameBase.StartsWith(config.ServiceLocator.ToString()))
                {
                    nameBaseForSelfCheck = HttpUtility.UrlDecode(nameBase);
                    nameBase = nameBase.Remove(0, config.ServiceLocator.ToString().Length);
                }
                else
                {
                    nameBaseForSelfCheck = queryLessUri + HttpUtilityEx.PathDecodeUTF8(nameBase);
                }

                nameBase = nameBase.TrimEnd('/');
                nameBaseForSelfCheck = nameBaseForSelfCheck.TrimEnd('/');
                if (targetUrl.EndsWith("/"))
                    nameBaseForSelfCheck += "/";

                var isSelf = nameBaseForSelfCheck.Equals(decodedTargetUrl);

                var ph = new PathHelper(nameBase);
                var resourceName = HttpUtility.UrlDecode(ph.GetFileName());

                var entry = !isDirectory
                                ? new BaseFileEntry(resourceName, contentLength, lastModified, service, session)
                                : new BaseDirectoryEntry(resourceName, contentLength, lastModified, service, session);

                if (isSelf)
                {
                    results.Self = entry;
                }
                else
                {
                    results.Childs.Add(entry);
                }
            }

            return results;
        }
    }
}