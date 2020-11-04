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
using System.Web;
using System.Collections.Generic;

namespace ASC.Web.UserControls.Forum.Common
{
    public class Settings
    {
        public ForumManager ForumManager { get; private set; }

        public LinkProvider LinkProvider { get; private set; }

        public Guid ID { get; private set; }

        public Guid ModuleID { get; private set; }

        public Guid ImageItemID { get; private set; }

        public int TopicCountOnPage { get; private set; }

        public int PostCountOnPage { get; private set; }

        public string UserControlsVirtualPath { get; private set; }

        public string FileStoreModuleID { get; private set; }

        public string ThreadParamName{ get; private set;}

        public string TopicParamName { get; private set; }

        public string TagParamName { get; private set; }

        public string ActionParamName { get; private set; }
        public string PostParamName { get; private set; }

        public string NewPostPageVirtualPath { get; private set; }
        public string NewPostPageAbsolutePath { get { return GetAbsolutePathWithParams(NewPostPageVirtualPath); } }

        public string SearchPageVirtualPath { get; private set; }
        public string SearchPageAbsolutePath { get { return GetAbsolutePathWithParams(SearchPageVirtualPath); } }
        
        public string StartPageVirtualPath { get; private set; }
        public string StartPageAbsolutePath { get { return GetAbsolutePathWithParams(StartPageVirtualPath); } }

        public string TopicPageVirtualPath { get; private set; }
        public string TopicPageAbsolutePath { get { return GetAbsolutePathWithParams(TopicPageVirtualPath); } }

        public string EditTopicPageVirtualPath { get; private set; }
        public string EditTopicPageAbsolutePath { get { return GetAbsolutePathWithParams(EditTopicPageVirtualPath); } }

        public string PostPageVirtualPath { get; private set; }
        public string PostPageAbsolutePath { get { return GetAbsolutePathWithParams(PostPageVirtualPath); } }

        public Settings()
        {
            ID = Guid.NewGuid();
            TopicCountOnPage = 20;
            PostCountOnPage = 20;

            ThreadParamName = "f";
            TopicParamName = "t";
            TagParamName = "tag";
            ActionParamName = "a";
            PostParamName = "m";

            ModuleID = new Guid("853B6EB9-73EE-438d-9B09-8FFEEDF36234");
            ImageItemID = new Guid("853B6EB9-73EE-438d-9B09-8FFEEDF36234");
            UserControlsVirtualPath = "~/Products/Community/Modules/Forum/UserControls";
            StartPageVirtualPath = "~/Products/Community/Modules/Forum/Default.aspx";
            TopicPageVirtualPath = "~/Products/Community/Modules/Forum/Topics.aspx";
            PostPageVirtualPath = "~/Products/Community/Modules/Forum/Posts.aspx";
            SearchPageVirtualPath = "~/Products/Community/Modules/Forum/Search.aspx";
            NewPostPageVirtualPath = "~/Products/Community/Modules/Forum/NewPost.aspx";
            EditTopicPageVirtualPath = "~/Products/Community/Modules/Forum/EditTopic.aspx";
            FileStoreModuleID = "forum";

            LinkProvider = new LinkProvider(this);
            //registry
            ForumManager = new ForumManager(this);
        }

        private static string GetAbsolutePathWithParams(string virtualPath)
        {
            if (!string.IsNullOrEmpty(virtualPath))
            {
                if(virtualPath.IndexOf("?")!=-1)
                    return VirtualPathUtility.ToAbsolute(virtualPath.Split('?')[0]) + "?" + virtualPath.Split('?')[1];
                
                return VirtualPathUtility.ToAbsolute(virtualPath) + "?";
            }

            return string.Empty;
        }

        internal string[] GetAllPageAdditionalParams()
        {
            var result = new List<string>();

            result.AddRange(GetPageAdditionalParams(PostPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(TopicPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(SearchPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(SearchPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(NewPostPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(EditTopicPageVirtualPath));

            return result.ToArray();
        }

        private static string[] GetPageAdditionalParams(string url)
        {
            if (string.IsNullOrEmpty(url) || url.IndexOf("?") == -1) return new string[0];

            var query = url.Split('?')[1];
            if (!string.IsNullOrEmpty(query))
            {
                var result = new List<string>();
                foreach (var param in query.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    result.Add(param.Split('=')[0]);
                }

                return result.ToArray();
            }

            return new string[0];
        }
    }
}
