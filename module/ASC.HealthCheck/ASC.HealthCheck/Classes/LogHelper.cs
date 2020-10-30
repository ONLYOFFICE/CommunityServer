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
                                path = Path.Combine(Environment.CurrentDirectory, path);
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