/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
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

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/usercontrols/management/monitoring/js/loghelper.js");
            Page.RegisterStyle("~/usercontrols/management/monitoring/css/monitoring.less");
            
            if (IsDownloadRequest())
                DownloadArchive();

            HelpLink = CommonLinkUtility.GetHelpLink();
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