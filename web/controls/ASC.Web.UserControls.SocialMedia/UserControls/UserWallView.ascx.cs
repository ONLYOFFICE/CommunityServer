/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.SocialMedia.Twitter;
using ASC.SocialMedia;
using ASC.SocialMedia.Facebook;
using DotNetOpenAuth.OAuth.ChannelElements;
using ASC.Core;
using System.Threading;
using ASC.Core.Tenants;
using ASC.Web.UserControls.SocialMedia.Resources;
using ASC.SocialMedia.LinkedIn;

namespace ASC.Web.UserControls.SocialMedia.UserControls
{
    public partial class UserWallView : System.Web.UI.UserControl
    {
        #region - Classes -

        public class TwitterInfo
        {
            public TwitterApiInfo ApiInfo { get; set; }
            public String UserName { get; set; }
            public Decimal? UserID { get; set; }

            public TwitterInfo()
            {
                ApiInfo = new TwitterApiInfo();
            }
        }

        public class FacebookInfo
        {
            public string AccessToken { get; set; }
            public string UserID { get; set; }
        }

        public class LinkedInInfo
        {
            public string AccessToken { get; internal set; }
            public IConsumerTokenManager TokenManager { get; internal set; }

            public LinkedInInfo(IConsumerTokenManager tokenManager, string accessToken)
            {
                this.AccessToken = accessToken;
                this.TokenManager = tokenManager;
            }
        }

        #endregion

        #region - Members -

        private List<SocialNetworks> _socialNetworks = new List<SocialNetworks>();

        private List<Exception> _thrownExceptions = new List<Exception>();

        public const string MessageCountCookieName = "sm_msg_count";

        #endregion

        #region - Properties -

        public TwitterInfo TwitterInformation { get; set; }

        public FacebookInfo FacebookInformation { get; set; }

        public LinkedInInfo LinkedInInformation { get; set; }

        public List<SocialNetworks> SelectedSocialNetworks
        {
            get { return _socialNetworks; }
        }

        public int MessageCount { get; set; }

        public int LoadedMessageCount { get; set; }

        public List<Exception> ThrownExceptions
        {
            get { return _thrownExceptions; }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckInputParameters();

            var ctrlMessageList = (ListActivityMessageView)LoadControl("ListActivityMessageView.ascx");

            ctrlMessageList.MessageList = GetUserActivity();
            LoadedMessageCount = ctrlMessageList.MessageList.Count;

            CommonContainerHolder.Controls.Add(ctrlMessageList);

            _ctrlMessageCount.SelectedValue = MessageCount.ToString();

            if (ThrownExceptions.Count > 0 && _socialNetworks.Count == 1)
            {
                _ctrlErrorDescription.InnerText = GetErrorDescription(_thrownExceptions[0]);
                _ctrlErrorDescriptionContainer.Style.Add(HtmlTextWriterStyle.Display, "block");
                _ctrlMessageNumberContainer.Visible = false;
            }
            /*if (ThrownExceptions.Count > 0 && _socialNetworks.Count > 1)
            {
                _ctrlErrorDescription.InnerText = GetErrorDescription(_thrownExceptions[0]);
                _ctrlErrorDescriptionContainer.Style.Add(HtmlTextWriterStyle.Display, "block");
            }*/

        }

        #region - Methods -

        private void CheckInputParameters()
        {
            if (SelectedSocialNetworks.Contains(SocialNetworks.Twitter))
            {
                if (TwitterInformation == null)
                    throw new ArgumentException("You must set TwitterInformation object");
            }

            if (SelectedSocialNetworks.Contains(SocialNetworks.Facebook))
            {
                if (FacebookInformation == null)
                    throw new ArgumentException("You must set FacebookInformation object");
            }

            if (SelectedSocialNetworks.Contains(SocialNetworks.LinkedIn))
            {
                if (LinkedInInformation == null)
                    throw new ArgumentException("You must set LinkedInInformation object");
            }
        }

        private List<Message> GetUserActivity()
        {
            if (SelectedSocialNetworks == null || SelectedSocialNetworks.Count == 0)
                return null;

            List<Message> messageList = new List<Message>(MessageCount);

            Func<List<Message>> dlgGetWallTwitter = GetUserWallTwitter;
            Func<List<Message>> dlgGetWallFacebook = GetUserWallFacebook;
            Func<Tenant, List<Message>> dlgGetWallLinkedIn = GetUserWallLinkedIn;

            IAsyncResult arGetWallTwitter = null;
            IAsyncResult arGetWallFacebook = null;
            IAsyncResult arGetWallLinkedIn = null;

            List<WaitHandle> waitHandles = new List<WaitHandle>();

            if (SelectedSocialNetworks.Contains(SocialNetworks.Twitter))
            {
                arGetWallTwitter = dlgGetWallTwitter.BeginInvoke(null, null);
                waitHandles.Add(arGetWallTwitter.AsyncWaitHandle);
            }

            if (SelectedSocialNetworks.Contains(SocialNetworks.Facebook))
            {
                arGetWallFacebook = dlgGetWallFacebook.BeginInvoke(null, null);
                waitHandles.Add(arGetWallFacebook.AsyncWaitHandle);
            }

            if (SelectedSocialNetworks.Contains(SocialNetworks.LinkedIn))
            {
                Tenant currentTenant = CoreContext.TenantManager.GetCurrentTenant();
                arGetWallLinkedIn = dlgGetWallLinkedIn.BeginInvoke(currentTenant, null, null);
                waitHandles.Add(arGetWallLinkedIn.AsyncWaitHandle);
            }

            WaitHandle.WaitAll(waitHandles.ToArray());

            if (SelectedSocialNetworks.Contains(SocialNetworks.Twitter))
                messageList.AddRange(dlgGetWallTwitter.EndInvoke(arGetWallTwitter));

            if (SelectedSocialNetworks.Contains(SocialNetworks.Facebook))
                messageList.AddRange(dlgGetWallFacebook.EndInvoke(arGetWallFacebook));

            if (SelectedSocialNetworks.Contains(SocialNetworks.LinkedIn))
                messageList.AddRange(dlgGetWallLinkedIn.EndInvoke(arGetWallLinkedIn));

            return messageList.OrderByDescending(m => m.PostedOn).Take(MessageCount).ToList();
        }

        private List<Message> GetUserWallTwitter()
        {
            List<Message> messageList = new List<Message>();
            try
            {
                TwitterDataProvider twitterProvider = new TwitterDataProvider(TwitterInformation.ApiInfo);
                messageList.AddRange(twitterProvider.GetUserHomeTimeLine(MessageCount));
            }
            catch (Exception ex)
            {
                ThrownExceptions.Add(ex);
            }
            return messageList;
        }

        private List<Message> GetUserWallFacebook()
        {
            List<Message> messageList = new List<Message>();
            try
            {
                FacebookApiInfo apiInfo = new FacebookApiInfo { AccessToken = FacebookInformation.AccessToken };
                FacebookDataProvider facebookProvider = new FacebookDataProvider(apiInfo);
                messageList.AddRange(facebookProvider.LoadUserWall(FacebookInformation.UserID, MessageCount));
            }
            catch (Exception ex)
            {
                ThrownExceptions.Add(ex);
            }
            return messageList;
        }

        private List<Message> GetUserWallLinkedIn(object tenant)
        {
            CoreContext.TenantManager.SetCurrentTenant((Tenant)tenant);

            List<Message> messageList = new List<Message>();
            try
            {
                LinkedInDataProvider provider = new LinkedInDataProvider(LinkedInInformation.TokenManager, LinkedInInformation.AccessToken);
                messageList.AddRange(provider.GetCurrentUserNetworkUpdates(MessageCount));
            }
            catch (Exception ex)
            {
                ThrownExceptions.Add(ex);
            }
            return messageList;
        }

        private string GetErrorDescription(Exception ex)
        {
            if (ex is ASC.SocialMedia.Twitter.ConnectionFailureException)
                return SocialMediaResource.ErrorTwitterConnectionFailure;

            if (ex is ASC.SocialMedia.Twitter.RateLimitException)
                return SocialMediaResource.ErrorTwitterRateLimit;

            if (ex is ASC.SocialMedia.Twitter.ResourceNotFoundException)
                return SocialMediaResource.ErrorTwitterAccountNotFound;

            if (ex is ASC.SocialMedia.Twitter.UnauthorizedException)
                return SocialMediaResource.ErrorTwitterUnauthorized;

            if (ex is ASC.SocialMedia.Facebook.OAuthException)
                return SocialMediaResource.ErrorFacebookOAuth;

            if (ex is ASC.SocialMedia.Facebook.APILimitException)
                return SocialMediaResource.ErrorFacebookAPILimit;

            if (ex is ASC.SocialMedia.LinkedIn.UnauthorizedException)
                return SocialMediaResource.ErrorLinkedInUnauthorized;

            return SocialMediaResource.ErrorInternalServer;
        }

        #endregion
    }
}