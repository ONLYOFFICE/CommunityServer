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
using System.Web;
using System.Collections.Generic;
using System.Text;
using ASC.Web.Community.News.Resources;
using AjaxPro;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Studio.Core;

namespace ASC.Web.Community.News
{
    [AjaxNamespace("MainAjaxMaster")]
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
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/community/modules/news/app_themes/default/newsstylesheet.css"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/products/community/modules/news/js/news.js"));

            Utility.RegisterTypeForAjax(GetType(), Page);

            SearchText = "";
            if (!string.IsNullOrEmpty(Request["search"]))
            {
                SearchText = Request["search"];
            }

            RegisterClientScript();
        }

        public void SetInfoMessage(string message, InfoType type)
        {
            MainNewsContainer.Options.InfoType = type;
            MainNewsContainer.Options.InfoMessageText = message;
        }

        private void RegisterClientScript()
        {
            var sb = new StringBuilder();
            var subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();

            var userList = new List<string>();
            if (IAmAsRecipient != null)
            {
                userList = new List<string>(
                    subscriptionProvider.GetSubscriptions(
                        NewsConst.NewFeed,
                        IAmAsRecipient)
                    );
            }

            var subscribed = userList.Contains(null);
            var feedID = Request.QueryString["docid"] ?? string.Empty;
            var isSubsribedOnComments = subscriptionProvider.IsSubscribed(NewsConst.NewComment, IAmAsRecipient, feedID);
            var displaySubscribeOnComments = !string.IsNullOrEmpty(feedID);

            const string subscribeFunc = @"
                var NotifyNewsUploads={2};
                var SubscribeOnNews = function() {{
                            AjaxPro.onLoading = function(b) {{
                                if (b) jq('#subscribeOnNews').parent().block();
                                else jq('#subscribeOnNews').parent().unblock();
                            }}
                            MainAjaxMaster.SubscribeOnNews(NotifyNewsUploads,
				            function(result) {{
					            NotifyNewsUploads = result.value;
					            if(!NotifyNewsUploads){{
						            jq('#subscribeOnNews').html('{0}');
					            }} else {{
						            jq('#subscribeOnNews').html('{1}');
					            }}
				            }}
			            );
		            }};";

            sb.AppendFormat(subscribeFunc,
                            NewsResource.NotifyOnUploadsMessage.ReplaceSingleQuote(),
                            NewsResource.UnNotifyOnUploadsMessage.ReplaceSingleQuote(),
                            subscribed.ToString().ToLower());


            if (displaySubscribeOnComments)
            {
                sb.AppendFormat(@"
                            var NotifyNewsComments = {0};
                            var SubscribeOnComments = function(feedId){{
                                AjaxPro.onLoading = function(b) {{
                                    if (b) jq('.eventsHeaderBlock').block();
                                    else jq('.eventsHeaderBlock').unblock();
                                }}
                                MainAjaxMaster.SubscribeOnComments(NotifyNewsComments, feedId,
		                            function(result){{
			                            NotifyNewsComments = result.value;
			                            if(!NotifyNewsComments){{
                                            jq('#statusSubscribe').removeClass('subscribed').addClass('unsubscribed');
                                            jq('#statusSubscribe').attr('title', '{1}');
			                            }} else {{
                                            jq('#statusSubscribe').removeClass('unsubscribed').addClass('subscribed');
                                            jq('#statusSubscribe').attr('title', '{2}');
			                            }}
		                            }});
                                jq('#eventsActionsMenuPanel').hide();
                                jq('.menu-small').removeClass('active');
                            }};",
                                isSubsribedOnComments.ToString().ToLower(),
                                NewsResource.SubscribeOnNewComments.ReplaceSingleQuote(),
                                NewsResource.UnsubscribeFromNewComments.ReplaceSingleQuote()

                    );
            }

            Page.RegisterInlineScript(sb.ToString(), onReady: false);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SubscribeOnNews(bool isSubscribe)
        {

            var subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();
            if (IAmAsRecipient == null)
            {
                return false;
            }
            if (!isSubscribe)
            {
                subscriptionProvider.Subscribe(NewsConst.NewFeed, null, IAmAsRecipient);
                return true;
            }
            else
            {
                subscriptionProvider.UnSubscribe(NewsConst.NewFeed, null, IAmAsRecipient);
                return false;
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SubscribeOnComments(bool isSubscribe, string commentID)
        {

            var subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();
            if (IAmAsRecipient == null)
            {
                return false;
            }
            if (!isSubscribe)
            {
                subscriptionProvider.Subscribe(NewsConst.NewComment, commentID, IAmAsRecipient);
                return true;
            }
            else
            {
                subscriptionProvider.UnSubscribe(NewsConst.NewComment, commentID, IAmAsRecipient);
                return false;
            }
        }

    }
}