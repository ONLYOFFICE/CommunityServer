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
using System.Web;

namespace ASC.Web.UserControls.Forum.Common
{
    internal static class StringExtensions
    {
        internal static string ToQuery(this string url)
        {
            if (String.IsNullOrEmpty(url))
                return "";

            if (url.IndexOf('?') != -1)
                return url + "&";

            return url + "?";
        }
    }

    public class LinkProvider
    {

        public Settings Settings { get; private set; }

        private List<string> _protectedParams = null;

        public LinkProvider(Settings settings)
        {
            this.Settings = settings;
        }

        private void InitProtectedParams()
        {
            _protectedParams = new List<string>();

            _protectedParams.AddRange(new string[] {
                Settings.ActionParamName,
                Settings.PostParamName,
                Settings.TagParamName,
                Settings.ThreadParamName,
                Settings.TopicParamName,
                "p"
            });

            _protectedParams.AddRange(Settings.GetAllPageAdditionalParams());
        }


        private Dictionary<string, string> GetCurrentParams()
        {
            var result = new Dictionary<string, string>();
            if (HttpContext.Current == null && HttpContext.Current.Request == null)
                return result;

            var query = HttpContext.Current.Request.Url.Query ?? "";
            query = query.Trim('?');
            foreach (var param in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!result.ContainsKey(param.Split('=')[0]))
                    result.Add(param.Split('=')[0], param.Split('=')[1]);
            }

            return result;
        }

        private string AddParams(string url, Dictionary<string, string> additionalParams)
        {
            if (additionalParams != null)
            {
                if (_protectedParams == null)
                    InitProtectedParams();

                foreach (var param in additionalParams)
                {
                    bool isAllow = true;
                    foreach (var protectedParam in _protectedParams)
                    {
                        if (String.Equals(param.Key, protectedParam, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isAllow = false;
                            break;
                        }
                    }

                    if (isAllow)
                        url += "&" + param.Key + "=" + param.Value;
                }
            }
            return url;
        }

        public string TopicList(int threadID)
        {
            return TopicList(threadID, -1, GetCurrentParams());
        }
        public string TopicList(int threadID, int pageNumber)
        {
            return TopicList(threadID, pageNumber, GetCurrentParams());
        }

        public string TopicList(int threadID, int pageNumber, Dictionary<string, string> additionalParams)
        {
            string result = Settings.TopicPageAbsolutePath.ToQuery() + Settings.ThreadParamName + "=" + threadID.ToString();
            if (pageNumber != -1)
                result += "&p=" + pageNumber.ToString();

            return AddParams(result, additionalParams);
        }

        public string PostList(int topicID)
        {
            return PostList(topicID, -1, GetCurrentParams());
        }
        public string PostList(int topicID, int pageNumber)
        {
            return PostList(topicID, pageNumber, GetCurrentParams());
        }

        public string PostList(int topicID, int pageNumber, Dictionary<string, string> additionalParams)
        {
            //string[] topicIDSplited = topicID.ToString().Split('&');
            string result = Settings.PostPageAbsolutePath.ToQuery() + Settings.TopicParamName + "=" + topicID.ToString();
            if (pageNumber != -1)
                result += "&p=" + pageNumber.ToString();

            return AddParams(result, additionalParams);
        }

        public string NewPost(int topicID)
        {
            return NewPost(topicID, PostAction.Normal, -1, GetCurrentParams());
        }

        public string NewPost(int topicID, PostAction postAction, int postID)
        {
            return NewPost(topicID, postAction, postID, GetCurrentParams());
        }


        public string NewPost(int topicID, Dictionary<string, string> additionalParams)
        {
            return NewPost(topicID, PostAction.Normal, -1, additionalParams);
        }

        public string NewPost(int topicID, PostAction postAction, int postID, Dictionary<string, string> additionalParams)
        {
            string result = Settings.NewPostPageAbsolutePath.ToQuery() + Settings.TopicParamName + "=" + topicID.ToString();

            if (postAction == PostAction.Quote)
                result += "&" + Settings.PostParamName + "=" + postID.ToString() + "&" + Settings.ActionParamName + "=1";

            else if (postAction == PostAction.Reply)
                result += "&" + Settings.PostParamName + "=" + postID.ToString() + "&" + Settings.ActionParamName + "=2";

            else if (postAction == PostAction.Edit)
                result += "&" + Settings.PostParamName + "=" + postID.ToString() + "&" + Settings.ActionParamName + "=3";


            return AddParams(result, additionalParams);
        }

        public string NewTopic(bool isPool)
        {
            return NewTopic(-1, isPool, GetCurrentParams());
        }

        public string NewTopic(int threadID, bool isPool)
        {
            return NewTopic(threadID, isPool, GetCurrentParams());
        }

        public string NewTopic(int threadID, bool isPool, Dictionary<string, string> additionalParams)
        {
            string result = Settings.NewPostPageAbsolutePath.ToQuery();
            if (threadID == -1)
            {
                result += Settings.PostParamName + "=" + (isPool ? "1" : "0");
            }
            else
            {
                result += Settings.ThreadParamName + "=" + threadID.ToString();
                result += "&" + Settings.PostParamName + "=" + (isPool ? "1" : "0");
            }

            return AddParams(result, additionalParams);
        }


        public string RecentPost(int postID, int topicID, int postCountInTopic)
        {
            return RecentPost(postID, topicID, postCountInTopic, GetCurrentParams());
        }
        public string RecentPost(int postID, int topicID, int postCountInTopic, Dictionary<string, string> additionalParams)
        {
            int pageNum = Convert.ToInt32(Math.Ceiling(postCountInTopic / (Settings.PostCountOnPage * 1.0)));
            if (pageNum <= 0)
                pageNum = 1;

            string result = Settings.PostPageAbsolutePath.ToQuery() + Settings.TopicParamName + "=" + topicID.ToString() + "&p=" + pageNum.ToString();

            //result = AddParams(result, additionalParams);
            return result + "#" + postID.ToString();
        }

        public string Post(int postID, int topicID, int pageNumber)
        {
            return Post(postID, topicID, pageNumber, GetCurrentParams());
        }
        public string Post(int postID, int topicID, int pageNumber, Dictionary<string, string> additionalParams)
        {

            string result = Settings.PostPageAbsolutePath.ToQuery() + Settings.TopicParamName + "=" + topicID.ToString() + "&p=" + pageNumber.ToString();
            result = AddParams(result, additionalParams);
            return result + "#" + postID.ToString();
        }

        //searchByTag
        public string SearchByTag(int tagID)
        {
            return SearchByTag(tagID, GetCurrentParams());
        }
        public string SearchByTag(int tagID, Dictionary<string, string> additionalParams)
        {

            string result = Settings.SearchPageAbsolutePath.ToQuery() + Settings.TagParamName + "=" + tagID.ToString();
            return AddParams(result, additionalParams);
        }

        //edit Topic
        public string EditTopic(int topicID)
        {
            return EditTopic(topicID, GetCurrentParams());
        }
        public string EditTopic(int topicID, Dictionary<string, string> additionalParams)
        {
            string result = Settings.EditTopicPageAbsolutePath.ToQuery() + Settings.TopicParamName + "=" + topicID.ToString();
            return AddParams(result, additionalParams);
        }
    }
}
