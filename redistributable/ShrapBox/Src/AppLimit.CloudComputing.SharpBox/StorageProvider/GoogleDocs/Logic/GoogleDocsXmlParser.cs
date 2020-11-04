using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using AppLimit.CloudComputing.SharpBox.Common.Extensions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs.Logic
{
    internal static class GoogleDocsXmlParser
    {
        public static IEnumerable<BaseFileEntry> ParseEntriesXml(IStorageProviderSession session, String xml)
        {
            var doc = XDocument.Load(new StringReader(xml));

            var entries = doc.Descendants(XName.Get("entry", GoogleDocsConstants.AtomNamespace));

            var fsEntries = new List<BaseFileEntry>();

            foreach (var entry in entries)
            {
                var resourceId = entry.Element(XName.Get("resourceId", GoogleDocsConstants.GdNamespace)).ValueOrEmpty().Replace(':', '_');
                var title = entry.Element(XName.Get("title", GoogleDocsConstants.AtomNamespace)).ValueOrEmpty();
                var updated = entry.Element(XName.Get("updated", GoogleDocsConstants.AtomNamespace)).ValueOrEmpty();
                var etag = entry.Attribute(XName.Get("etag", GoogleDocsConstants.GdNamespace)).ValueOrEmpty();
                var kind = entry.Elements(XName.Get("category", GoogleDocsConstants.AtomNamespace)).Single(x => x.Attribute("scheme").ValueOrEmpty().Equals(GoogleDocsConstants.SchemeKind)).Attribute("label").ValueOrEmpty();

                BaseFileEntry fsEntry = kind.Equals("folder")
                                            ? new BaseDirectoryEntry(title, 0, Convert.ToDateTime(updated).ToUniversalTime(), session.Service, session)
                                            : new BaseFileEntry(title, 0, Convert.ToDateTime(updated).ToUniversalTime(), session.Service, session);

                fsEntry.Id = resourceId;
                fsEntry.SetPropertyValue(GoogleDocsConstants.EtagProperty, etag);
                fsEntry.SetPropertyValue(GoogleDocsConstants.KindProperty, kind);
                if (kind.Equals("folder"))
                {
                    var uploadUrl = entry.Elements(XName.Get("link", GoogleDocsConstants.AtomNamespace)).FirstOrDefault(x => x.Attribute("rel").ValueOrEmpty().Equals(GoogleDocsConstants.SchemeResCreateMedia)).AttributeOrNull("href").ValueOrEmpty();
                    fsEntry.SetPropertyValue(GoogleDocsConstants.ResCreateMediaProperty, uploadUrl);
                }
                else
                {
                    var length = entry.Element(XName.Get("quotaBytesUsed", GoogleDocsConstants.GdNamespace)).ValueOrEmpty();
                    var downloadUrl = entry.Elements(XName.Get("content", GoogleDocsConstants.AtomNamespace)).FirstOrDefault().AttributeOrNull("src").ValueOrEmpty();
                    var editUrl = entry.Elements(XName.Get("link", GoogleDocsConstants.AtomNamespace)).FirstOrDefault(x => x.Attribute("rel").ValueOrEmpty().Equals(GoogleDocsConstants.SchemeResEditMedia)).AttributeOrNull("href").ValueOrEmpty();

                    fsEntry.Length = Convert.ToInt64(length);
                    fsEntry.SetPropertyValue(GoogleDocsConstants.DownloadUrlProperty, downloadUrl);
                    if (!String.IsNullOrEmpty(editUrl))
                        fsEntry.SetPropertyValue(GoogleDocsConstants.ResEditMediaProperty, editUrl);

                    var ext = GoogleDocsResourceHelper.GetExtensionByKind(kind);
                    if (!String.IsNullOrEmpty(ext))
                    {
                        fsEntry.Name += '.' + ext;
                    }
                }

                //var parents = entry.Elements(XName.Get("link", GoogleDocsConstants.AtomNamespace))
                //    .Where(x => x.AttributeOrNull("rel").ValueOrEmpty().Equals(GoogleDocsConstants.SchemeParent))
                //    .Select(x =>
                //        {
                //            var parentUrl = x.ValueOrEmpty();
                //            var index = parentUrl.LastIndexOf('/');
                //            return parentUrl.Substring(index).Replace(':', '_').Replace("%3A", "_");
                //        });

                //fsEntry.SetPropertyValue(GoogleDocsConstants.ParentsProperty, String.Join(",", parents.ToArray()));

                fsEntries.Add(fsEntry);
            }

            return fsEntries;
        }

        public static String ParseNext(String xml)
        {
            var doc = XDocument.Load(new StringReader(xml));
            return doc.Elements(XName.Get("link", GoogleDocsConstants.AtomNamespace))
                      .SingleOrDefault(x => x.AttributeOrNull("rel").ValueOrEmpty().Equals("next")).ValueOrEmpty();
        }

        public static void WriteAtom(WebRequest request, params object[] content)
        {
            var xmlDoc = new XDocument { Declaration = new XDeclaration("1.0", "UTF-8", null) };
            xmlDoc.Add(content.Length > 1 ? new XElement(XName.Get("feed", GoogleDocsConstants.AtomNamespace), content) : content[0]);

            var sb = new StringBuilder();
            var tr = new StringWriter(sb);
            xmlDoc.Save(tr);

            var atom = sb.ToString();

            var bytes = Encoding.UTF8.GetBytes(atom);

            request.ContentLength = bytes.Length;
            request.ContentType = "application/atom+xml";

            var stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }

        public static XElement EntryElement(params object[] content)
        {
            return EntryElement(null, content);
        }

        public static XElement EntryElement(String etag, params object[] content)
        {
            var entry = new XElement(XName.Get("entry", GoogleDocsConstants.AtomNamespace), content);
            if (!String.IsNullOrEmpty(etag))
                entry.SetAttributeValue(XName.Get("etag", GoogleDocsConstants.GdNamespace), etag);
            return entry;
        }

        public static XElement OperationElement(String type)
        {
            return new XElement(XName.Get("operation", GoogleDocsConstants.BatchNamespace), new XAttribute("type", type));
        }

        public static XElement CategoryElement()
        {
            return new XElement(XName.Get("category", GoogleDocsConstants.AtomNamespace),
                                new XAttribute("scheme", GoogleDocsConstants.SchemeKind),
                                new XAttribute("term", GoogleDocsConstants.SchemeFolder));
        }

        public static XElement TitleElement(String title)
        {
            return new XElement(XName.Get("title", GoogleDocsConstants.AtomNamespace), title);
        }

        public static XElement IdElement(String id)
        {
            return new XElement(XName.Get("id", GoogleDocsConstants.AtomNamespace), GoogleDocsConstants.GoogleDocsFeedUrl + '/' + id);
        }
    }
}