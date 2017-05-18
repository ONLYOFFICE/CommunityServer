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
using System.Web;
using System.Collections.Generic;
using System.Text;
using ASC.Web.Community.News.Resources;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Studio.Core;

namespace ASC.Web.Community.News
{
    public partial class NewsMaster : System.Web.UI.MasterPage
    {
        public string SearchText { get; set; }

        public string CurrentPageCaption
        {
            get { return MainNewsContainer.CurrentPageCaption; }
            set { MainNewsContainer.CurrentPageCaption = value; }
        }

        private static IDirectRecipient IAmAsRecipient
        {
            get { return (IDirectRecipient)NewsNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()); }
        }

        protected Guid RequestedUserId
        {
            get
            {
                var result = Guid.Empty;
                try
                {
                    result = new Guid(Request["uid"]);
                }
                catch
                {
                }

                return result;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/products/community/modules/news/app_themes/default/newsstylesheet.css")
                .RegisterBodyScripts("~/products/community/modules/news/js/news.js");

            SearchText = "";
            if (!string.IsNullOrEmpty(Request["search"]))
            {
                SearchText = Request["search"];
            }
        }

        public void SetInfoMessage(string message, InfoType type)
        {
            MainNewsContainer.Options.InfoType = type;
            MainNewsContainer.Options.InfoMessageText = message;
        }
    }
}