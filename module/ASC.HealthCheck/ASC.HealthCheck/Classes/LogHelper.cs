/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using Ionic.Zip;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ASC.HealthCheck.Settings;

namespace ASC.HealthCheck.Classes
{
    public class LogHelper : ILogHelper
    {
        private readonly HttpResponseMessage result;
        private readonly DateTime startDate;
        private readonly DateTime endDate;
        private readonly ILog log;

        public LogHelper(HttpResponseMessage result, DateTime startDate, DateTime endDate)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }
            this.result = result;
            this.startDate = startDate;
            this.endDate = endDate;
            log = LogManager.GetLogger(typeof(LogHelper));
        }

        public void DownloadArchive()
        {
            try
            {
                log.Debug("DownloadArchive");
                using (var zip = new ZipFile())
                {
                    zip.AddFiles(EnumerateLogFiles(), true, string.Empty);
                    result.Content = new PushStreamContent((stream, content, context) =>
                    {
                        try
                        {
                            // workaround of hanging of threads
                            zip.ParallelDeflateThreshold = -1;
                            zip.Save(stream);
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Error on creating Save stream to zipFile. {0} {1}",
                                ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                        }
                        finally
                        {
                            stream.Close();
                        }
                    }, "application/zip");
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = GetArchiveName()
                    };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on DownloadArchive. {0} {1} {2}",
                    ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
        }

        private string GetArchiveName()
        {
            return string.Format("onlyoffice_logs_{0:yyyy-MM-dd}_{1:yyyy-MM-dd}.zip", startDate, endDate);
        }

        private IEnumerable<string> EnumerateLogFiles()
        {
            return GetLogFolders()
                .SelectMany(logFolder => Directory.EnumerateFiles(logFolder, "*.log", SearchOption.AllDirectories)
                .Where(logFile => IsLogMatchingDate(logFile)));
        }

        private IEnumerable<string> GetLogFolders()
        {
            var paths = HealthCheckCfgSectionHandler.Instance.LogFolders;

            return paths.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(path =>
                        {
                            if (!Path.IsPathRooted(path))
                            {
                                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                            }
                            return path;
                        })
                        .Where(Directory.Exists);
        }

        private bool IsLogMatchingDate(string logFile)
        {
            var fileInfo = new FileInfo(logFile);
            return fileInfo.LastWriteTimeUtc.Date >= startDate && fileInfo.LastWriteTimeUtc.Date <= endDate;
        }
    }
}