/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
            Page.RegisterStyle("~/products/community/modules/wiki/app_themes/default/css/wikicss.css");
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