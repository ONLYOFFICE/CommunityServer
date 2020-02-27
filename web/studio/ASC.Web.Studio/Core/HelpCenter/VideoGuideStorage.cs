/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

            var titles = doc.DocumentNode.SelectNodes("//div[@class='MainHelpCenter PageVideo']//div");

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