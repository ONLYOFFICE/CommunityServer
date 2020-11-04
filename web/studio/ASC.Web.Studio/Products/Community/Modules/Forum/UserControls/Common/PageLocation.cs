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

namespace ASC.Web.UserControls.Forum.Common
{   
    public enum ForumPage
    {
        Default,
        TopicList,
        PostList,
        NewPost,
        EditTopic,
        Search,
        UserProfile,
        ManagementCenter
    }

    public class PageLocation : ICloneable
    {
        public ForumPage Page{get; set;}
        public string Url { get; set;}
        
        public PageLocation(ForumPage page, string url)
        {
            this.Page = page;
            this.Url = url;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new PageLocation(this.Page, this.Url);
        }

        #endregion
    }
}
