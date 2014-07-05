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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Web.Studio.Utility;
using Ionic.Zip;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Monitoring, Location, SortOrder = 100)]
    public partial class LogHelper : UserControl
    {
        public const string Location = "~/UserControls/Management/Monitoring/LogHelper.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/monitoring/js/loghelper.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/monitoring/css/monitoring.less"));
            
            if (IsDownloadRequest())
                DownloadArchive();
        }

        private void DownloadArchive()
        {
            var archiveName = GetArchiveName();
            Response.ContentType = MimeMapping.GetMimeMapping(archiveName);
            Response.AddHeader("Content-Disposition", "attachment; filename=" + archiveName);
            CreateArchive(Response.OutputStream);
            Response.End();
        }
        
        private void CreateArchive(Stream outputStream)
        {
            using (var zip = new ZipFile())
            {
                zip.AddFiles(EnumerateLogFiles(GetStartDate(), GetEndDate()), true, "");
                zip.Save(outputStream);
            }
        }

        private string GetArchiveName()
        {
            return string.Format("teamlab_office_logs_{0:yyyy-MM-dd}_{1:yyyy-MM-dd}.zip", GetStartDate(), GetEndDate());
        }

        private bool IsDownloadRequest()
        {
            return HttpContext.Current.Request["download"] == "true";
        }

        private DateTime GetStartDate()
        {
            string startDate = HttpContext.Current.Request["start"];
            return !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : DateTime.MinValue;
        }

        private DateTime GetEndDate()
        {
            string endDate = HttpContext.Current.Request["end"];
            return !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : DateTime.MaxValue;
        }
        
        private IEnumerable<string> EnumerateLogFiles(DateTime startDate, DateTime endDate)
        {
            return GetLogFolders()
                .SelectMany(logFolder => Directory.EnumerateFiles(logFolder, "*.log", SearchOption.AllDirectories)
                                                  .Where(logFile => IsLogMatchingDate(logFile, startDate, endDate)));
        }

        private IEnumerable<string> GetLogFolders()
        {
            var paths = ConfigurationManager.AppSettings["monitoring.log-folder"] ?? @"..\Logs\";
            return paths.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(path =>
                            {
                                if (!Path.IsPathRooted(path))
                                    path = Path.Combine(Request.PhysicalApplicationPath, path);
                                return path;
                            })
                        .Where(Directory.Exists);
        }

        private bool IsLogMatchingDate(string logFile, DateTime startDate, DateTime endDate)
        {
            var fileInfo = new FileInfo(logFile);
            return fileInfo.LastWriteTimeUtc.Date >= startDate && fileInfo.LastWriteTimeUtc.Date <= endDate;
        }
    }
}