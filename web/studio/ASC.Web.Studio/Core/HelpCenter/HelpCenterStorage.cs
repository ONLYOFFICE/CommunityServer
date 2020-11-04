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
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

using HtmlAgilityPack;

namespace ASC.Web.Studio.Core.HelpCenter
{
    [Serializable]
    [DataContract(Name = "HelpCenterItem", Namespace = "")]
    public class HelpCenterItem
    {
        [DataMember(Name = "Title")] public string Title;

        [DataMember(Name = "Content")] public string Content;
    }

    [Serializable]
    [DataContract(Name = "HelpCenterData", Namespace = "")]
    public class HelpCenterData : BaseHelpCenterData
    {
        [DataMember(Name = "ListItems")]
        public List<HelpCenterItem> ListItems { get; set; }

        public HelpCenterData()
        {
            ListItems = new List<HelpCenterItem>();
        }

        public override void Init(string html, string helpLinkBlock, string baseUrl)
        {
            if (string.IsNullOrEmpty(html)) return;

            try
            {
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl = baseUrl + "/";
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var mainContent = doc.DocumentNode.SelectSingleNode("//div[@class='MainHelpCenter GettingStarted']");

                if (mainContent == null) return;

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
                                    href.Attributes["href"].Value = baseUrl + value.TrimStart('/');
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
                                var val = img.Attributes["src"].Value;
                                if (val.StartsWith("data:image") || val.StartsWith("http")) continue;
                                img.Attributes["src"].Value = GetBase64(baseUrl + val.TrimStart('/'));
                            }

                            foreach (var screenPhoto in images.Where(img =>
                                img.Attributes["class"] != null && img.Attributes["class"].Value.Contains("screenphoto")
                                && img.Attributes["target"] != null && img.ParentNode != null))
                            {
                                var bigphotoScreenId = screenPhoto.Attributes["target"].Value;

                                var bigphotoScreen = images.FirstOrDefault(img =>
                                    img.Attributes["id"] != null && img.Attributes["id"].Value == bigphotoScreenId
                                    && img.Attributes["class"] != null &&
                                    img.Attributes["class"].Value.Contains("bigphoto_screen")
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
                            ListItems.Add(new HelpCenterItem
                            {
                                Title = titles.InnerText,
                                Content = contents.InnerHtml
                            });
                        }
                    }
                    catch (Exception)
                    {
                        //_log.Error(string.Format("Error parse help html in {0} block: {1}. Culture {2}", i, helpLinkBlock, CultureInfo.CurrentCulture.TwoLetterISOLanguageName), e);
                    }
                }
            }
            catch (Exception)
            {
                //_log.Error(string.Format("Error parse help html: {0}. Culture {1}", helpLinkBlock, CultureInfo.CurrentCulture.TwoLetterISOLanguageName), e);
            }
        }

        private static string GetBase64(string externalUrl)
        {
            if ((ConfigurationManagerExtension.AppSettings["web.help-center.internal-uri"] ?? "false") != "true")
                return externalUrl;

            try
            {
                var req = (HttpWebRequest)WebRequest.Create(externalUrl);
                using (var response = req.GetResponse())
                using (var image = Image.FromStream(response.GetResponseStream()))
                using (var m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    var imageBytes = m.ToArray();

                    var base64 = Convert.ToBase64String(imageBytes);

                    return "data:image;base64," + base64;
                }
            }
            catch (Exception)
            {
                //_log.ErrorFormat("GetInternalLink {0}: {1}", externalUrl, e.Message);
            }
            return externalUrl;
        }
    }
}