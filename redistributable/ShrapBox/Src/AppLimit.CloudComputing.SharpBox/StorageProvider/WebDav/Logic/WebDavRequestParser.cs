using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AppLimit.CloudComputing.SharpBox.Common.Extensions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using System.IO;
using System.Xml;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net;

#if SILVERLIGHT || MONODROID
using System.Net;
#else
using System.Web;

#endif

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic
{
    internal delegate String NameBaseFilterCallback(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String NameBase);

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

#if !WINDOWS_PHONE && !MONODROID
        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session)
        {
            return CreateObjectsFromNetworkStream(data, targetUrl, service, session, null);
        }

        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session, NameBaseFilterCallback callback)
        {
            var config = session.ServiceConfiguration as WebDavConfiguration;
            var results = new WebDavRequestResult();

            var queryLessUri = HttpUtilityEx.GetPathAndQueryLessUri(config.ServiceLocator).ToString().TrimEnd('/');
            var decodedTargetUrl = HttpUtility.UrlDecode(targetUrl);
                
            var s = new StreamReader(data).ReadToEnd();
            //todo:
            var xDoc = XDocument.Load(new StringReader(s.Replace("d:d:", "d:")));
            var responses = xDoc.Descendants(XName.Get("response", DavNamespace));

            foreach (var response in responses)
            {
                bool isHidden = false;
                bool isDirectory = false;
                DateTime lastModified = DateTime.Now;
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
                    if (resourceType.Element(XName.Get("collection", DavNamespace)) != null)
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

                bool isSelf = nameBaseForSelfCheck.Equals(decodedTargetUrl);

                var ph = new PathHelper(nameBase);
                var resourceName = HttpUtility.UrlDecode(ph.GetFileName());

                BaseFileEntry entry = !isDirectory
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

        /// <summary>
        /// This method checks if the attached 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static bool CheckIfNameSpaceDAVSpace(String element, XmlTextReader reader)
        {
            // split the element into tag and field
            String[] fields = element.Split(':');

            // could be that the element has no namespace attached, so it is not part
            // of the webdav response
            if (fields.Length == 1)
                return false;

            // get the namespace list
            IDictionary<String, String> nameSpaceList = reader.GetNamespacesInScope(XmlNamespaceScope.All);

            // get the namespace of our node
            if (!nameSpaceList.ContainsKey(fields[0]))
                return false;

            // get the value
            String NsValue = nameSpaceList[fields[0]];

            // compare if it's a DAV namespce
            if (NsValue.ToLower().Equals("dav:"))
                return true;
            else
                return false;
        }
#else
        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session, NameBaseFilterCallback callback)
        {
            return null;
        }
#endif
    }
}