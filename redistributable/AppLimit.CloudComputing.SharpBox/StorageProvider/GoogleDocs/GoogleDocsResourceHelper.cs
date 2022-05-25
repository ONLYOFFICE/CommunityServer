using System.Linq;
using System.Text.RegularExpressions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    internal static class GoogleDocsResourceHelper
    {
        public static bool IsFileEntry(ICloudFileSystemEntry entry)
        {
            return !(entry is ICloudDirectoryEntry);
        }

        public static bool IsRootName(string name)
        {
            return name.Equals("/") || name.Equals(GoogleDocsConstants.RootFolderId);
        }

        public static bool IsNoolOrRoot(ICloudFileSystemEntry dir)
        {
            return dir == null || ((dir is ICloudDirectoryEntry) && dir.Id.Equals(GoogleDocsConstants.RootFolderId));
        }

        public static void UpdateResourceByXml(IStorageProviderSession session, out ICloudFileSystemEntry resource, string xml)
        {
            var parsed = GoogleDocsXmlParser.ParseEntriesXml(session, xml).Single();
            resource = parsed;
        }

        public static string GetStreamExtensionByKind(string kind)
        {
            switch (kind)
            {
                case "document":
                    return "docx";
                case "presentation":
                    return "pptx";
                case "spreadsheet":
                    return "xlsx";
                case "drawing":
                    return "svg";
                default:
                    return string.Empty;
            }
        }

        public static string GetExtensionByKind(string kind)
        {
            switch (kind)
            {
                case "document":
                    return "gdoc";
                case "presentation":
                    return "gslides";
                case "spreadsheet":
                    return "gsheet";
                case "drawing":
                    return "svg";
                default:
                    return string.Empty;
            }
        }

        public static bool IsResorceId(string name)
        {
            return Regex.IsMatch(name, GoogleDocsConstants.ResourceIdRegexPattern);
        }

        public static bool OfGoogleDocsKind(ICloudFileSystemEntry entry)
        {
            var kind = entry.GetPropertyValue(GoogleDocsConstants.KindProperty);
            return kind.Equals("document") || kind.Equals("presentation") || kind.Equals("spreadsheet") || kind.Equals("drawing");
        }
    }
}