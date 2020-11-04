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


using System.Web;
using ASC.Forum;

namespace ASC.Web.Community.Forum
{
    public static class ForumShortcutProvider
    {
        public static string GetCreateContentPageUrl()
        {
            return ValidateCreateTopicOrPoll(false) ? VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/NewPost.aspx") + "?m=0" : null;
        }

        private static bool ValidateCreateTopicOrPoll(bool isPool)
        {
            return ForumManager.Instance.ValidateAccessSecurityAction((isPool ? ForumAction.PollCreate : ForumAction.TopicCreate), null);
        }
    }
}
