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
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/app_themes/default/css/wikicss.css"));
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
        public bool SubscribeOnNewPage(bool isSubscribe)
        {
            var subscriptionProvider = WikiNotifySource.Instance.GetSubscriptionProvider();
            if (IAmAsRecipient == null)
            {
                return false;
            }
            if (!isSubscribe)
            {

                subscriptionProvider.Subscribe(
                    Constants.NewPage,
                    null,
                    IAmAsRecipient
                    );
                return true;
            }
            else
            {
                subscriptionProvider.UnSubscribe(
                    Constants.NewPage,
                    null,
                    IAmAsRecipient
                    );
                return false;
            }
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

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SubscribeOnPageToCat(bool isSubscribe, string catName)
        {

            var subscriptionProvider = WikiNotifySource.Instance.GetSubscriptionProvider();
            if (IAmAsRecipient == null)
            {
                return false;
            }
            if (!isSubscribe)
            {

                subscriptionProvider.Subscribe(
                    Constants.AddPageToCat,
                    catName,
                    IAmAsRecipient
                    );
                return true;
            }
            else
            {
                subscriptionProvider.UnSubscribe(
                    Constants.AddPageToCat,
                    catName,
                    IAmAsRecipient
                    );
                return false;
            }
        }
    }
}