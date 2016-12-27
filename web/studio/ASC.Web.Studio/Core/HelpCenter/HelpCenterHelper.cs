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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Web.Core.Client;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using HtmlAgilityPack;
using log4net;

namespace ASC.Web.Studio.Core.HelpCenter
{
    public class HelpCenterHelper
    {
        private static ILog _log = LogManager.GetLogger("ASC.Web.HelpCenter");

        #region VideoGuide

        public static List<VideoGuideItem> GetVideoGuides()
        {
            var data = GetVideoGuidesAll();
            var wathced = UserVideoSettings.GetUserVideoGuide();

            data.RemoveAll(r => r != null && wathced.Contains(r.Id));
            if (!UserHelpTourHelper.IsNewUser)
            {
                data.RemoveAll(r => r.Status == "default");
            }
            return data;
        }

        private static List<VideoGuideItem> GetVideoGuidesAll()
        {
            var url = CommonLinkUtility.GetHelpLink();
            if (string.IsNullOrEmpty(url))
            {
                return new List<VideoGuideItem>();
            }

            url += "/video.aspx";

            var storageData = VideoGuideStorage.GetVideoGuide();
            if (storageData == null)
            {
                return new List<VideoGuideItem>();
            }

            VideoGuideData videoGuideData = null;
            if (storageData.ContainsKey(url))
            {
                videoGuideData = storageData[url];
            }
            if (videoGuideData != null && string.CompareOrdinal(videoGuideData.ResetCacheKey, ClientSettings.ResetCacheKey) != 0)
            {
                videoGuideData = null;
            }

            if (videoGuideData == null)
            {
                var html = SendRequest(url);
                var data = ParseVideoGuideHtml(html);

                videoGuideData = new VideoGuideData { ListItems = new List<VideoGuideItem>() };
                //if (data.Any())
                {
                    videoGuideData.ListItems = data;
                    videoGuideData.ResetCacheKey = ClientSettings.ResetCacheKey;

                    storageData[url] = videoGuideData;
                    VideoGuideStorage.UpdateVideoGuide(storageData);
                }
            }

            return videoGuideData.ListItems;
        }

        private static List<VideoGuideItem> ParseVideoGuideHtml(string html)
        {
            var data = new List<VideoGuideItem>();
            if (string.IsNullOrEmpty(html)) return data;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var titles = doc.DocumentNode.SelectNodes("//div[@class='MainHelpCenter PageVideo']//div");

                if (titles == null || titles.Count(a => a.Attributes["id"] != null) != titles.Count() || !titles.Elements("a").Any()) return data;

                var helpLinkBlock = CommonLinkUtility.GetHelpLink(false);

                var needTitles = titles.Where(x =>
                                              x.Attributes["data-status"] != null
                                              && (x.Attributes["data-status"].Value == "new" || x.Attributes["data-status"].Value == "default")).ToList();

                foreach (var needTitle in needTitles)
                {
                    var title = needTitle.SelectSingleNode(".//span[@class='link_to_video']").InnerText;
                    var id = needTitle.Attributes["id"].Value;
                    var link = helpLinkBlock + needTitle.Element("a").Attributes["href"].Value.Substring(1);
                    var status = needTitle.Attributes["data-status"].Value;

                    data.Add(new VideoGuideItem { Title = title, Id = id, Link = link, Status = status });
                }
            }
            catch (Exception e)
            {
                _log.Error("Error parse video guide html", e);
            }
            return data;
        }

        #endregion

        #region GettingStarted

        public static List<HelpCenterItem> GetHelpCenter(string module, string helpLinkBlock)
        {
            var url = CommonLinkUtility.GetHelpLink();
            if (string.IsNullOrEmpty(url))
            {
                return new List<HelpCenterItem>();
            }

            url += "/gettingstarted/" + module;

            var storageData = HelpCenterStorage.GetHelpCenter();
            if (storageData == null)
            {
                return new List<HelpCenterItem>();
            }

            HelpCenterData helpCenterData = null;
            if (storageData.ContainsKey(url))
            {
                helpCenterData = storageData[url];
            }
            if (helpCenterData != null && String.CompareOrdinal(helpCenterData.ResetCacheKey, ClientSettings.ResetCacheKey) != 0)
            {
                helpCenterData = null;
            }

            if (helpCenterData == null)
            {
                var html = SendRequest(url);
                var data = ParseHelpCenterHtml(html, helpLinkBlock);

                helpCenterData = new HelpCenterData();
                //if (data.Any())
                {
                    helpCenterData.ListItems = data;
                    helpCenterData.ResetCacheKey = ClientSettings.ResetCacheKey;

                    storageData[url] = helpCenterData;
                    HelpCenterStorage.UpdateHelpCenter(storageData);
                }
            }

            return helpCenterData.ListItems ?? new List<HelpCenterItem>();
        }

        private static List<HelpCenterItem> ParseHelpCenterHtml(string html, string helpLinkBlock)
        {
            var helpCenterItems = new List<HelpCenterItem>();

            if (string.IsNullOrEmpty(html)) return helpCenterItems;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var urlHelp = CommonLinkUtility.GetHelpLink(false);
                var mainContent = doc.DocumentNode.SelectSingleNode("//div[@class='MainHelpCenter GettingStarted']");

                if (mainContent == null) return helpCenterItems;

                var blocks = (mainContent.SelectNodes(".//div[@class='gs_content']"))
                    .Where(r => r.Attributes["id"] != null)
                    .Select(x => x.Attributes["id"].Value).ToList();

                var i = 0;
                foreach (var block in mainContent.SelectNodes(".//div[@class='gs_content']"))
                {
                    try
                    {
                        i++;
                    var hrefs = block.SelectNodes(".//a[@href]");

                    if (hrefs != null)
                    {
                        foreach (var href in hrefs.Where(r =>
                            {
                                var value = r.Attributes["href"].Value;
                                return r.Attributes["href"] != null
                                       && !string.IsNullOrEmpty(value)
                                       && !value.StartsWith("mailto:")
                                       && !value.StartsWith("http");
                            }))
                        {
                            var value = href.Attributes["href"].Value;

                            if (value.IndexOf("#", StringComparison.Ordinal) != 0 && value.Length > 1)
                            {
                                href.Attributes["href"].Value = urlHelp + value.Substring(1);
                                href.SetAttributeValue("target", "_blank");
                            }
                            else
                            {
                                if (!blocks.Contains(value.Substring(1))) continue;

                                href.Attributes["href"].Value = helpLinkBlock +
                                                                blocks.IndexOf(value.Substring(1))
                                                                      .ToString(CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    var images = block.SelectNodes(".//img");
                    if (images != null)
                    {
                        foreach (var img in images.Where(img => img.Attributes["src"] != null))
                        {
                            img.Attributes["src"].Value = GetInternalLink(urlHelp + img.Attributes["src"].Value);
                        }

                        foreach (var screenPhoto in images.Where(img =>
                                                                 img.Attributes["class"] != null && img.Attributes["class"].Value.Contains("screenphoto")
                                                                 && img.Attributes["target"] != null && img.ParentNode != null))
                        {
                            var bigphotoScreenId = screenPhoto.Attributes["target"].Value;

                            var bigphotoScreen = images.FirstOrDefault(img =>
                                                                       img.Attributes["id"] != null && img.Attributes["id"].Value == bigphotoScreenId
                                                                       && img.Attributes["class"] != null && img.Attributes["class"].Value.Contains("bigphoto_screen")
                                                                       && img.Attributes["src"] != null);
                            if (bigphotoScreen == null) continue;

                            var hrefNode = doc.CreateElement("a");
                            var hrefAttribute = doc.CreateAttribute("href");
                            hrefAttribute.Value = bigphotoScreen.Attributes["src"].Value;
                            hrefNode.Attributes.Append(hrefAttribute);

                            hrefAttribute = doc.CreateAttribute("class");
                            hrefAttribute.Value = "screenzoom";
                            hrefNode.Attributes.Append(hrefAttribute);

                            string title = null;
                            var titleAttribute = bigphotoScreen.Attributes["title"];
                            if (titleAttribute != null)
                            {
                                title = titleAttribute.Value;
                            }
                            else
                            {
                                var altAttribute = bigphotoScreen.Attributes["alt"];
                                if (altAttribute != null)
                                {
                                    title = altAttribute.Value;
                                }
                            }
                            if (!string.IsNullOrEmpty(title))
                            {
                                hrefAttribute = doc.CreateAttribute("title");
                                hrefAttribute.Value = title;
                                hrefNode.Attributes.Append(hrefAttribute);
                            }

                            hrefAttribute = doc.CreateAttribute("rel");
                            hrefAttribute.Value = "imageHelpCenter";
                            hrefNode.Attributes.Append(hrefAttribute);

                            screenPhoto.ParentNode.ReplaceChild(hrefNode, screenPhoto);
                            hrefNode.AppendChild(screenPhoto);
                        }
                    }

                    var titles = block.SelectSingleNode(".//h2");
                    var contents = block.SelectSingleNode(".//div[@class='PortalHelp']");

                    if (titles != null && contents != null)
                    {
                        helpCenterItems.Add(new HelpCenterItem { Title = titles.InnerText, Content = contents.InnerHtml });
                    }
                    }
                    catch(Exception e)
                    {
                        _log.Error(string.Format("Error parse help html in {0} block: {1}. Culture {2}", i, helpLinkBlock, CultureInfo.CurrentCulture.TwoLetterISOLanguageName), e);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(string.Format("Error parse help html: {0}. Culture {1}", helpLinkBlock, CultureInfo.CurrentCulture.TwoLetterISOLanguageName), e);
            }
            return helpCenterItems;
        }

        private static string GetInternalLink(string externalUrl)
        {
            if ((WebConfigurationManager.AppSettings["web.help-center.internal-uri"] ?? "false") != "true")
                return externalUrl;

            try
            {
                externalUrl = externalUrl.ToLower().Trim();
                var imagePath = externalUrl.GetHashCode().ToString(CultureInfo.InvariantCulture) + FileUtility.GetFileExtension(externalUrl);

                var storage = StorageFactory.GetStorage(Tenant.DEFAULT_TENANT.ToString(CultureInfo.InvariantCulture), "static_helpcenter");

                if (storage.IsFile(imagePath))
                    return storage.GetUri(imagePath).ToString();

                var req = (HttpWebRequest)WebRequest.Create(externalUrl);
                using (var response = req.GetResponse())
                using (var fileStream = response.GetResponseStream())
                {
                    return storage.Save(imagePath, fileStream).ToString();
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("GetInternalLink {0}: {1}", externalUrl, e.Message);
            }
            return externalUrl;
        }

        #endregion

        private static bool _stopRequesting;

        private static String SendRequest(string url)
        {
            if (_stopRequesting) return string.Empty;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                httpWebRequest.AllowAutoRedirect = false;
                httpWebRequest.Timeout = 15000;
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers["Accept-Language"] = "en"; // get correct en lang

                var countTry = 0;
                const int maxTry = 3;
                while (countTry < maxTry)
                {
                    try
                    {
                        countTry++;
                        using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                        using (var stream = httpWebResponse.GetResponseStream())
                        using (var reader = new StreamReader(stream, Encoding.GetEncoding(httpWebResponse.CharacterSet)))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status != WebExceptionStatus.Timeout)
                        {
                            throw;
                        }
                    }
                }

                _stopRequesting = true;
                throw new WebException("Timeout " + maxTry, WebExceptionStatus.Timeout);
            }
            catch (Exception e)
            {
                _log.Error(string.Format("HelpCenter is not avaliable by url {0}", url), e);
            }
            return string.Empty;
        }
    }
}