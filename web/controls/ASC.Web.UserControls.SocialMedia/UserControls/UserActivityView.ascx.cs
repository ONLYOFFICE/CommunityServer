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