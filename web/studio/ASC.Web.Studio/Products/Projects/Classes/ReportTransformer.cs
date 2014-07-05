/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Reflection;

using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core;

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
            var logo = string.IsNullOrEmpty(SetupInfo.MainLogoMailTmplURL) ? "http://cdn.teamlab.com/media/newsletters/images/00.jpg" : SetupInfo.MainLogoMailTmplURL;

            for (var i = 0; i < columns.Count; i++)
            {
                parameters.AddParam("p" + i, string.Empty, columns[i]);
            }

            parameters.AddParam("p" + columns.Count, string.Empty, Global.ReportCsvDelimiter.Value);
            parameters.AddParam("logo", string.Empty, logo);

            return parameters;
        }
    }
}