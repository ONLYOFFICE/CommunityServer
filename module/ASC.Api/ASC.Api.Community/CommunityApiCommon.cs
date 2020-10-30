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
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Web.Community.Birthdays;
using ASC.Web.Studio.Utility.HtmlUtility;


namespace ASC.Api.Community
{
    public partial class CommunityApi
    {
        /// <summary>
        /// Subscribe or unsubscribe on birthday of user with the ID specified
        /// </summary>
        /// <short>Subscribe/unsubscribe on birthday</short>
        /// <param name="userid">user ID</param>
        /// <param name="onRemind">should be subscribed or unsubscribed</param>
        /// <returns>onRemind value</returns>
        /// <category>Birthday</category>
        [Create("birthday")]
        public bool RemindAboutBirthday(Guid userid, bool onRemind)
        {
            BirthdaysNotifyClient.Instance.SetSubscription(SecurityContext.CurrentAccount.ID, userid, onRemind);
            return onRemind;
        }

        [Create("preview")]
        public object GetPreview(string title, string content)
        {
            return new {
                title = HttpUtility.HtmlEncode(title),
                content = HtmlUtility.GetFull(content)
            };
        }

    }
}
