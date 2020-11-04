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
using System.Web;
using ASC.Web.Studio.Core;
using AjaxPro;
using ASC.Core;
using ASC.Notify.Recipients;
using ASC.Web.Community.Wiki.Common;
using WikiNotifySource = ASC.Web.UserControls.Wiki.WikiNotifySource;

namespace ASC.Web.Community.Wiki
{
    [Flags]
    public enum WikiNavigationActionVisible
    {
        None = 0x000,
        AddNewPage = 0x001,
        UploadFile = 0x002,
        EditThePage = 0x004,
        ShowVersions = 0x008,
        PrintPage = 0x010,
        DeleteThePage = 0x020,
        SubscriptionOnNewPage = 0x040,
        SubscriptionThePage = 0x080,
        SubscriptionOnCategory = 0x100,
        CreateThePage = 0x200
    }

    [AjaxNamespace("MainWikiAjaxMaster")]
    public partial class WikiMaster : System.Web.UI.MasterPage
    {
        public delegate string GetDelUniqIdHandle();

        public event GetDelUniqIdHandle GetDelUniqId;

        public delegate WikiNavigationActionVisible GetNavigateActionsVisibleHandle();

        private static IDirectRecipient IAmAsRecipient
        {
            get { return (IDirectRecipient)WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/Products/Community/Modules/Wiki/App_Themes/default/css/wikicss.css");
            Utility.RegisterTypeForAjax(GetType(), Page);
        }

        public void PrintInfoMessage(string info, InfoType infoType)
        {
            MainWikiContainer.Options.InfoMessageText = info;
            MainWikiContainer.Options.InfoType = infoType;
        }

        protected string MasterResolveUrlLC(string url)
        {
            return this.ResolveUrlLC(url);
        }

        public string GetDeleteUniqueId()
        {
            if (GetDelUniqId == null)
                return string.Empty;

            return GetDelUniqId();
        }

        public string CurrentPageCaption
        {
            get { return MainWikiContainer.CurrentPageCaption; }
            set { MainWikiContainer.CurrentPageCaption = value; }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SubscribeOnEditPage(bool isSubscribe, string pageName)
        {
            pageName = HttpUtility.HtmlDecode(pageName);
            var subscriptionProvider = WikiNotifySource.Instance.GetSubscriptionProvider();
            if (IAmAsRecipient == null)
            {
                return false;
            }
            if (!isSubscribe)
            {

                subscriptionProvider.Subscribe(
                    Constants.EditPage,
                    pageName,
                    IAmAsRecipient
                    );
                return true;
            }
            else
            {
                subscriptionProvider.UnSubscribe(
                    Constants.EditPage,
                    pageName,
                    IAmAsRecipient
                    );
                return false;
            }
        }

    }
}