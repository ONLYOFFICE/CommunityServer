/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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