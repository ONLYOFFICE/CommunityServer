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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Data.Storage;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Common.Util;

namespace ASC.Web.UserControls.Bookmarking.Util
{
    public class WebSiteThumbnailHelper : IThumbnailHelper
    {
        private readonly List<Uri> ProcessedUrls = new List<Uri>();


        private IDataStore getStore(HttpContext context)
        {
            return StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), BookmarkingRequestConstants.BookmarkingStorageManagerID);
        }

        private IDataStore getStore(HttpContext context, int tenant)
        {
            return StorageFactory.GetStorage(tenant.ToString(), BookmarkingRequestConstants.BookmarkingStorageManagerID);
        }


        public void MakeThumbnail(string url)
        {
            MakeThumbnail(url, true, true, HttpContext.Current, TenantProvider.CurrentTenantID);
        }

        public void MakeThumbnail(string url, bool async, bool notOverride, HttpContext context, int tenantID)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return;

                if (notOverride)
                {
                    var fileName = GetFileNameByUrl(HttpUtility.UrlEncode(url), BookmarkingSettings.ThumbSmallSize);
                    if (getStore(context, tenantID).IsFile(string.Empty, fileName))
                    {
                        return;
                    }
                }

                List<object> p = new List<object>();
                p.Add(url);
                p.Add(context);
                p.Add(tenantID);
                ThreadPool.QueueUserWorkItem(MakeThumbnailCallback, p);

                //if (!async) thread.Join();

            }
            catch { }
        }

        private void MakeThumbnailCallback(object p)
        {
            #region Sanity Check

            var url = string.Empty;
            var context = HttpContext.Current;
            int tenant = 0;

            try
            {
                if (p is List<Object>)
                {
                    var s = p as List<object>;
                    if (s.Count == 3)
                    {
                        if (s[0] is string)
                        {
                            url = s[0] as string;
                        }
                        if (s[1] is HttpContext)
                        {
                            context = s[1] as HttpContext;
                        }
                        if (s[2] is int)
                        {
                            tenant = (int)s[2];
                        }
                    }
                }
            }
            catch { }

            #endregion

            var outFileName = string.Empty;
            Process ps = null;
            int psid = -1;
            Uri uri = null;
            try
            {
                //Check true url
                if (!string.IsNullOrEmpty(url) && context != null && Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    lock (ProcessedUrls)
                    {
                        if (ProcessedUrls.Contains(uri)) return;//Screen ih bin processing go away!

                        ProcessedUrls.Add(uri);

                    }
                    //We got normal url
                    //Map server path
                    var appDataDir = context.Server.MapPath(VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/App_Data"));
                    var screenShoterName = Path.Combine(appDataDir, "IECapt.exe");

                    if (File.Exists(screenShoterName))
                    {
                        outFileName = Path.Combine(appDataDir, Path.Combine("screens", Guid.NewGuid() + ".png"));
                        if (!Directory.Exists(Path.GetDirectoryName(outFileName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(outFileName));
                        }
                        var userAgent = context.Request.UserAgent;
                        var arguments = BuildArguments(uri, outFileName, userAgent);
                        //Launch process
                        using (ps = new Process())
                        {
                            ps.StartInfo = new ProcessStartInfo(screenShoterName, arguments);
                            ps.StartInfo.CreateNoWindow = true;
                            ps.Start();
                            psid = ps.Id;
                            if (ps.WaitForExit(15000))//Wait 15 sec and close 
                            {
                                //Ta da. File created
                                if (File.Exists(outFileName))
                                {
                                    //Upload!
                                    //Warning! Huge memory overhead!
                                    using (Image image = Image.FromFile(outFileName))
                                    {
                                        using (
                                            Image clipImage = new Bitmap(BookmarkingSettings.ThumbSmallSize.Width,
                                                                         BookmarkingSettings.ThumbSmallSize.Height))
                                        {
                                            using (var graphics = Graphics.FromImage(clipImage))
                                            {
                                                graphics.CompositingQuality = CompositingQuality.HighQuality;
                                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                                //Bicubic is better for minimizing image.
                                                graphics.SmoothingMode = SmoothingMode.HighQuality;
                                                graphics.DrawImage(image,
                                                                   Rectangle.FromLTRB(0, 0,
                                                                                      BookmarkingSettings.ThumbSmallSize
                                                                                          .
                                                                                          Width,
                                                                                      BookmarkingSettings.ThumbSmallSize
                                                                                          .
                                                                                          Height),
                                                                   Rectangle.FromLTRB(0, 0,
                                                                                      BookmarkingSettings.BrowserSize.
                                                                                          Width,
                                                                                      BookmarkingSettings.BrowserSize.
                                                                                          Height),
                                                                   GraphicsUnit.Pixel
                                                    );
                                                using (var ms = new MemoryStream())
                                                {
                                                    clipImage.Save(ms, BookmarkingSettings.CaptureImageFormat);
                                                    ms.Position = 0;
                                                    IDataStore store = getStore(context, tenant);
                                                    var fileName = GetFileNameByUrl(HttpUtility.UrlEncode(url), BookmarkingSettings.ThumbSmallSize);
                                                    store.Save(string.Empty, fileName, ms);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Process hasn't exited
                                //finally will kill it
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ps != null)
                {
                    try
                    {
                        ps.Kill();
                    }
                    catch
                    {
                        //Don't throw
                    }
                }
                //try kill
                if (psid != -1)
                {
                    try
                    {
                        var proc = Process.GetProcessById(psid);
                        if (proc != null)
                        {
                            proc.Kill();
                        }
                    }
                    catch
                    {
                        //Don't throw
                    }
                }
                if (!string.IsNullOrEmpty(outFileName) && File.Exists(outFileName))
                {
                    File.Delete(outFileName);
                }

                lock (ProcessedUrls)
                {
                    if (uri != null && ProcessedUrls.Contains(uri))
                    {
                        ProcessedUrls.Remove(uri);
                    }
                }
            }

        }


        private static string BuildArguments(Uri uri, string outFileName, string userAgent)
        {
            return string.Format("--url=\"{0}\" --out=\"{1}\" --delay={2} --max-wait={4} --min-width={3} --silent ",
                        uri, outFileName, 1000, BookmarkingSettings.BrowserSize.Width, 10000);

        }


        public string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size)
        {
            var fileName = GetFileNameByUrl(Url, size);

            return getStore(HttpContext.Current).IsFile(string.Empty, fileName) ? getStore(HttpContext.Current).GetUri(string.Empty, fileName).ToString() : string.Empty;
        }

        public string GetThumbnailUrlForUpdate(string Url, BookmarkingThumbnailSize size)
        {
            return GetThumbnailUrl(Url, size);
        }

        #region DeleteThumbnail
        public void DeleteThumbnail(string Url)
        {
            try
            {
                var fileName = GetFileNameByUrl(Url, BookmarkingSettings.ThumbSmallSize);
                getStore(HttpContext.Current, TenantProvider.CurrentTenantID).Delete(fileName);
            }
            catch (FileNotFoundException) { }
        }

        #endregion

        private static string GetSHA256(string text)
        {
            var ue = new UnicodeEncoding();
            var message = ue.GetBytes(text);

            var hashString = new SHA256Managed();
            var hashValue = hashString.ComputeHash(message);

            var hex = new StringBuilder();
            foreach (byte x in hashValue) hex.AppendFormat("{0:x2}", x);
            return hex.ToString();
        }

        private static string GetFileNameByUrl(string url, BookmarkingThumbnailSize size)
        {
            string sizeString = size == null ? string.Empty : size.ToString();
            return string.Format("{0}{1}.{2}", GetSHA256(url), sizeString, BookmarkingSettings.CaptureImageFormat.ToString());
        }
    }
}