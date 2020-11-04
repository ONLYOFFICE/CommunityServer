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
using System.Linq;
using System.Runtime.Serialization;
using HtmlAgilityPack;

namespace ASC.Web.Studio.Core.HelpCenter
{
    [Serializable]
    [DataContract(Name = "VideoGuideItem", Namespace = "")]
    public class VideoGuideItem
    {
        [DataMember(Name = "Title")]
        public string Title;

        [DataMember(Name = "Id")]
        public string Id;

        [DataMember(Name = "Link")]
        public string Link;

        [DataMember(Name = "Status")]
        public string Status;
    }


    [Serializable]
    [DataContract(Name = "VideoGuideStorageItem", Namespace = "")]
    public class VideoGuideData : BaseHelpCenterData
    {
        [DataMember(Name = "ListItems")]
        public List<VideoGuideItem> ListItems { get; set; }

        public VideoGuideData()
        {
            ListItems = new List<VideoGuideItem>();
        }

        public override void Init(string html, string helpLinkBlock, string baseUrl)
        {
            if (string.IsNullOrEmpty(html)) return;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var titles = doc.DocumentNode.SelectNodes("//div[@id='totalVideoList']//div");

            if (titles == null || titles.Count(a => a.Attributes["id"] != null) != titles.Count() || !titles.Elements("a").Any()) return;

            var needTitles = titles.Where(x =>
                                            x.Attributes["data-status"] != null
                                            && (x.Attributes["data-status"].Value == "new" || x.Attributes["data-status"].Value == "default")).ToList();

            foreach (var needTitle in needTitles)
            {
                var title = needTitle.SelectSingleNode(".//span[@class='link_to_video']").InnerText;
                var id = needTitle.Attributes["id"].Value;
                var link = string.Format("{0}/{1}", helpLinkBlock.TrimEnd('/'), needTitle.Element("a").Attributes["href"].Value.TrimStart('/'));
                var status = needTitle.Attributes["data-status"].Value;

                ListItems.Add(new VideoGuideItem { Title = title, Id = id, Link = link, Status = status });
            }
        }
    }
}