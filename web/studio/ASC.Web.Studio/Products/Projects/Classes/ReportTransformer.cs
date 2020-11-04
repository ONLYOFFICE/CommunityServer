/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Classes
{
    static class ReportTransformer
    {
        public static string Transform(this Report report, IList<object[]> reportData, ReportViewType view, int templateID)
        {
            var xml = new StringBuilder();

            if (reportData.Count != 0)
            {
                xml = xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
                   .Append("<reportResult>");
                foreach (var row in reportData)
                {
                    xml.Append("<r ");
                    for (var i = 0; i < row.Length; i++)
                    {
                        xml.AppendFormat("c{0}=\"{1}\" ", i, ToString(row[i], view));
                    }
                    xml.Append("/>");
                }
                xml.Append("</reportResult>");
            }
            else
            {
                xml = xml.Append(string.Format("<div class='noContentBlock'>{0}</div>", ProjectsCommonResource.NoData));
            }

            return report.Transform(xml.ToString(), view, templateID);
        }

        public static string Transform(this Report report, string xml, ReportViewType view, int templateID)
        {
            if (view == ReportViewType.Xml)
            {
                return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(xml)));
            }
            if (view == ReportViewType.EMail)
            {
                xml = report.Transform(xml, ReportViewType.Html, templateID);
            }

            var xslt = report.GetXslTransform(view);
            if (xslt == null) throw new InvalidOperationException("Xslt not found for type " + report.ReportType + " and view " + view);

            using (var reader = XmlReader.Create(new StringReader(xml)))
            using (var writer = new StringWriter())
            using (XmlWriter.Create(writer, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                xslt.Transform(reader, GetXslParameters(report, view, templateID), writer);
                return writer.ToString();
            }
        }


        private static string ToString(object value, ReportViewType view)
        {
            if (value == null) return null;
            if (value is Enum) return ((Enum)value).ToString("d");
            if (value is DateTime) return ((DateTime)value).ToString("o");
            if (value is float) return ((float) value).ToString(CultureInfo.InvariantCulture);

            value = value.ToString()
                         .Replace("&", "&amp;")
                         .Replace("<", "&lt;")
                         .Replace(">", "&gt;")
                         .Replace("\"", "&quot;");
            if (view == ReportViewType.Csv && (value as string).Contains(","))
                value = string.Format("&quot;{0}&quot;", value);

            return value.ToString();
        }

        private static XslCompiledTransform GetXslTransform(this Report report, ReportViewType viewType)
        {
            var viewTypeStr = viewType.ToString().ToLower();
            return GetXslTransform(string.Format("{0}_{1}.{2}.xsl", report.ReportType, report.Filter.ViewType, viewTypeStr)) ??
                GetXslTransform(string.Format("{0}.{1}.xsl", report.ReportType, viewTypeStr)) ??
                GetXslTransform(string.Format("{0}.xsl", viewTypeStr));
        }

        public static XslCompiledTransform GetXslTransform(string fileName)
        {
            var transform = new XslCompiledTransform();
            var assembly = Assembly.GetExecutingAssembly();
            var path = Path.GetDirectoryName(assembly.Location);
            if ("bin".Equals(Path.GetFileName(path), StringComparison.InvariantCultureIgnoreCase) && File.Exists(fileName))
            {
                    transform.Load(fileName);
                    return transform;
            }

            using (var stream = assembly.GetManifestResourceStream("ASC.Web.Projects.templates." + fileName))
            {
                if (stream != null)
                {
                    using (var xmlReader = XmlReader.Create(stream))
                    {
                        transform.Load(xmlReader);
                        return transform;
                    }
                }
            }
            return null;
        }

        private static XsltArgumentList GetXslParameters(this Report report, ReportViewType view, int templateID)
        {
            var parameters = new XsltArgumentList();
            var columns = report.GetColumns(view, templateID);
            var logo = string.IsNullOrEmpty(SetupInfo.MainLogoMailTmplURL) ? CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoDark(true)) : SetupInfo.MainLogoMailTmplURL;
            var logoText = TenantWhiteLabelSettings.Load().LogoText;

            for (var i = 0; i < columns.Count; i++)
            {
                parameters.AddParam("p" + i, string.Empty, columns[i]);
            }

            parameters.AddParam("p" + columns.Count, string.Empty, Global.ReportCsvDelimiter.Value);
            parameters.AddParam("logo", string.Empty, logo);
            parameters.AddParam("logoText", string.Empty, logoText);

            return parameters;
        }
    }
}