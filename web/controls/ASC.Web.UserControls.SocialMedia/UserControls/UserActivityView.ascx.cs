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
using System.Linq;
using System.Web;
using ASC.SocialMedia;
using ASC.Web.UserControls.SocialMedia.Resources;
using ASC.SocialMedia.Twitter;

namespace ASC.Web.UserControls.SocialMedia.UserControls
{
    public partial class UserActivityView : BaseUserControl
    {
        #region - Classes -

        public class TwitterInfo
        {
            public TwitterApiInfo ApiInfo { get; set; }
            public List<UserAccountInfo> UserAccounts { get; set; }

            public TwitterInfo()
            {
                ApiInfo = new TwitterApiInfo();
                UserAccounts = new List<UserAccountInfo>();
            }
        }

        public class UserAccountInfo
        {
            public string ScreenName { get; set; }
            public Decimal? UserID { get; set; }
        }

        #endregion

        #region - Members -

        private List<SocialNetworks> _socialNetworks = new List<SocialNetworks>();

        #endregion

        #region - Properties -

        public TwitterInfo TwitterInformation { get; set; }

        public List<SocialNetworks> SelectedSocialNetworks
        {
            get { return _socialNetworks; }
        }

        public int MessageCount { get; set; }

        public int LoadedMessageCount { get; set; }

        public const string MessageCountCookieName = "sm_msg_count";

        public Exception LastException { get; set; }

        public bool InvalidTwitterAccountExist { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckInputParameters();

            SetMessageCount();

            var ctrlMessageList = (ListActivityMessageView)LoadControl("ListActivityMessageView.ascx");

            try
            {
                ctrlMessageList.MessageList = GetUserActivity();
                LoadedMessageCount = ctrlMessageList.MessageList.Count;
            }
            catch (Exception ex)
            {
                LastException = ex;
                throw ex;
            }

            CommonContainerHolder.Controls.Add(ctrlMessageList);
        }

        #region - Methods -

        private void CheckInputParameters()
        {
            if (SelectedSocialNetworks.Contains(SocialNetworks.Twitter))
            {
                if (TwitterInformation == null)
                    throw new ArgumentException("You must set TwitterInformation object");
            }
        }

        private void SetMessageCount()
        {
            int messageCountFromCookie;
            if (HttpContext.Current.Request.Cookies[MessageCountCookieName] != null && Int32.TryParse(HttpContext.Current.Request.Cookies[MessageCountCookieName].Value, out messageCountFromCookie))
                MessageCount = messageCountFromCookie;

            _ctrlMessageCount.SelectedValue = MessageCount.ToString();
        }

        private List<Message> GetUserActivity()
        {
            if (SelectedSocialNetworks == null)
                return null;

            List<Message> messages = new List<Message>();
            if (SelectedSocialNetworks.Contains(SocialNetworks.Twitter))
            {
                TwitterDataProvider twitterProvider = new TwitterDataProvider(TwitterInformation.ApiInfo);
                foreach (UserAccountInfo accountInfo in TwitterInformation.UserAccounts)
                {
                    try
                    {
                        messages.AddRange(twitterProvider.GetUserTweets(accountInfo.UserID, accountInfo.ScreenName, MessageCount));
                    }
                    catch (ResourceNotFoundException)
                    {
                        _ctrlErrorDescription.InnerText += String.Format("{0}: {1}", SocialMediaResource.ErrorUnknownTwitterAccount, accountInfo.ScreenName);
                        _ctrlErrorDescriptionContainer.Style.Add(System.Web.UI.HtmlTextWriterStyle.Display, "block");
                        this.InvalidTwitterAccountExist = true;
                    }
                }
            }

            return messages.OrderByDescending(m => m.PostedOn).Take(MessageCount).ToList();
        }


        #endregion
    }
}