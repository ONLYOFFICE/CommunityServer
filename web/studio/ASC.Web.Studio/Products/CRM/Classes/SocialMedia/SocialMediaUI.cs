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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.SocialMedia;
using ASC.SocialMedia.Facebook;
using ASC.SocialMedia.Twitter;
using ASC.SocialMedia.LinkedIn;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Resources;
using ASC.Web.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Classes.SocialMedia;
using ASC.Web.CRM.Controls.SocialMedia;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.SocialMedia.Resources;
using ASC.Web.UserControls.SocialMedia.UserControls;
using log4net;
using Newtonsoft.Json;
using ASC.CRM.Core;

namespace ASC.Web.CRM.SocialMedia
{

    [AjaxPro.AjaxNamespace("AjaxPro.SocialMediaUI")]
    public class SocialMediaUI
    {
        private ILog _logger = LogManager.GetLogger(SocialMediaConstants.LoggerName);

        #region - GetContactActivity -

        [AjaxPro.AjaxMethod]
        public string GetContactActivity(int contactID, int messageCount)
        {
            UserActivityView ctrlUserActivity = null;
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }

                var contact = Global.DaoFactory.GetContactDao().GetByID(contactID);
                return GetContactActivityPage(ctrlUserActivity, contact, messageCount);
            }
            catch (Exception ex)
            {
                throw ProcessError(ctrlUserActivity.LastException ?? ex, "GetContactActivity");
            }
        }

        private string GetContactActivityPage(UserActivityView ctrlUserActivity, Contact contact, int messageCount)
        {
            var page = new Page();
            var form = new HtmlForm { EnableViewState = false };

            ctrlUserActivity = (UserActivityView)page.LoadControl(PathProvider.GetFileStaticRelativePath("SocialMedia/UserActivityView.ascx"));
            InitTwitter(ctrlUserActivity, contact);
            if (ctrlUserActivity.TwitterInformation.UserAccounts.Count == 0)
            {
                page.Controls.Add(new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_twitter.png", ProductEntryPoint.ID),
                    Header = CRMSocialMediaResource.EmptyContentTwitterAccountsHeader,
                    Describe = CRMSocialMediaResource.EmptyContentTwitterAccountsDescribe,
                    ButtonHTML = String.Format(@"<a class='link dotline plus' href='javascript:void(0);'
                                                    onclick='ASC.CRM.SocialMedia.FindTwitterProfiles(jq(this),""{0}"", 1, 9);'>{1}</a>",
                                               contact is Company ? "company" : "people",
                                               CRMSocialMediaResource.LinkTwitterAccount)
                });

                return RenderPage(page);
            }

            ctrlUserActivity.MessageCount = messageCount;
            form.Controls.Add(ctrlUserActivity);
            page.Controls.Add(form);

            var executedPage = RenderPage(page);

            if (ctrlUserActivity.LoadedMessageCount == 0 && ctrlUserActivity.LastException == null)
            {
                page = new Page();

                //TODO
                page.Controls.Add(new EmptyScreenControl
                {

                    Header = CRMCommonResource.NoLoadedMessages,

                });
                executedPage = RenderPage(page);
            }

            return executedPage;
        }

        /// <summary>
        /// Renders a page to html string
        /// </summary>
        /// <param name="page">Page to render</param>
        /// <returns>Page as HTML string</returns>
        private static string RenderPage(Page page)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Html32TextWriter hw = new Html32TextWriter(sw);
            StringWriter output = new StringWriter();
            HttpContext.Current.Server.Execute(page, hw, false);
            return sb.ToString();
        }

        private void InitTwitter(UserActivityView ctrlUserActivity, Contact contact)
        {
            ctrlUserActivity.TwitterInformation = new UserActivityView.TwitterInfo();

            var twitterAccounts = Global.DaoFactory.GetContactInfoDao().GetList(contact.ID, ContactInfoType.Twitter, null, null);

            if (twitterAccounts.Count == 0)
                return;


            foreach (var twitterAccount in twitterAccounts)
                ctrlUserActivity.TwitterInformation.UserAccounts.Add(new UserActivityView.UserAccountInfo { ScreenName = twitterAccount.Data });

            ctrlUserActivity.TwitterInformation.ApiInfo = TwitterApiHelper.GetTwitterApiInfoForCurrentUser();
            ctrlUserActivity.SelectedSocialNetworks.Add(SocialNetworks.Twitter);
        }

        #endregion

        #region - FindUsers -

        [AjaxPro.AjaxMethod]
        public string FindUsers(string searchText, string socialNetwork)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }
                if (socialNetwork.ToLower() == "twitter")
                {
                    TwitterDataProvider provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());
                    List<TwitterUserInfo> users = provider.FindUsers(searchText);
                    return GetTwitterUserListPage(users);
                }
                if (socialNetwork.ToLower() == "facebook")
                {
                    FacebookApiInfo apiInfo = FacebookApiHelper.GetFacebookApiInfoForCurrentUser();

                    if (apiInfo == null)
                        throw new SocialMediaAccountNotFound(SocialMediaResource.SocialMediaAccountNotFoundFacebook);

                    FacebookDataProvider facebookProvider = new FacebookDataProvider(apiInfo);

                    List<FacebookUserInfo> users = facebookProvider.FindUsers(searchText);
                    return GetFacebookUserListPage(users);
                }
                return null;

            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "FindUsers");
            }
        }

        private string GetTwitterUserListPage(List<TwitterUserInfo> users)
        {
            Page page = new Page();
            HtmlForm form = new HtmlForm();
            form.EnableViewState = false;

            ListTwitterUserInfoView ctrlUserList = (ListTwitterUserInfoView)page.LoadControl(PathProvider.GetFileStaticRelativePath("SocialMedia/ListTwitterUserInfoView.ascx"));
            ctrlUserList.UserInfoCollection = users;
            form.Controls.Add(ctrlUserList);
            page.Controls.Add(form);
            return RenderPage(page);
        }

        private string GetFacebookUserListPage(List<FacebookUserInfo> users)
        {
            Page page = new Page();
            HtmlForm form = new HtmlForm();
            form.EnableViewState = false;

            ListFacebookUserInfoView ctrlUserList = (ListFacebookUserInfoView)page.LoadControl(PathProvider.GetFileStaticRelativePath("SocialMedia/ListFacebookUserInfoView.ascx"));
            ctrlUserList.UserInfoCollection = users;
            form.Controls.Add(ctrlUserList);
            page.Controls.Add(form);
            return RenderPage(page);
        }

        [AjaxPro.AjaxMethod]
        public string FindTwitterProfiles(string searchText)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }

                TwitterDataProvider provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());
                List<TwitterUserInfo> users = provider.FindUsers(searchText);
                /*List<TwitterUserInfo> users = new List<TwitterUserInfo>();
                users.Add(new TwitterUserInfo { Description = "I'm a cool user", SmallImageUrl = "http://localhost/TeamLab/products/crm/data/0/photos/00/00/10/contact_10_50_50.jpg", UserName = "User", ScreenName = "user", UserID = 1 });
                users.Add(new TwitterUserInfo { Description = "I'm a cool user", SmallImageUrl = "http://localhost/TeamLab/products/crm/data/0/photos/00/00/10/contact_10_50_50.jpg", UserName = "User", ScreenName = "user", UserID = 1 });
                users.Add(new TwitterUserInfo { Description = "I'm a cool user", SmallImageUrl = "http://localhost/TeamLab/products/crm/data/0/photos/00/00/10/contact_10_50_50.jpg", UserName = "User", ScreenName = "user", UserID = 1 });*/
                string result = JsonConvert.SerializeObject(users);
                return result;
            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "FindTwitterProfiles");
            }
        }

        [AjaxPro.AjaxMethod]
        public string FindFacebookProfiles(string searchText)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }

                FacebookApiInfo apiInfo = FacebookApiHelper.GetFacebookApiInfoForCurrentUser();

                if (apiInfo == null)
                    throw new SocialMediaAccountNotFound(SocialMediaResource.SocialMediaAccountNotFoundFacebook);

                FacebookDataProvider facebookProvider = new FacebookDataProvider(apiInfo);

                List<FacebookUserInfo> users = facebookProvider.FindUsers(searchText);
                string result = JsonConvert.SerializeObject(users);
                return result;
            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "FindTwitterProfiles");
            }
        }

        [AjaxPro.AjaxMethod]
        public string FindLinkedInProfiles(string firstName, string lastName)
        {
            //Process authorization
            if (!ProcessAuthorization(HttpContext.Current))
            {
                AccessDenied(HttpContext.Current);
                return null;
            }

            var provider = LinkedInApiHelper.GetLinkedInDataProviderForCurrentUser();

            if (provider == null)
                throw new SocialMediaAccountNotFound(SocialMediaResource.SocialMediaAccountNotFoundLinkedIn);

            var users = provider.FindUsers(firstName, lastName);

            foreach (var user in users)
            {
                if (String.IsNullOrEmpty(user.ImageUrl))
                    user.ImageUrl = ContactPhotoManager.GetMediumSizePhoto(0, false);
            }
            string result = JsonConvert.SerializeObject(users);

            return result;

        }

        #endregion

        #region - ShowTwitterUserRelationWindow -

        [AjaxPro.AjaxMethod]
        public string ShowTwitterUserRelationWindow(string userID)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }
                TwitterDataProvider provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());
                TwitterUserInfo user = provider.LoadUserInfo(Convert.ToDecimal(userID));
                return GetTwitterUserInfoPage(user);

            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "ShowTwitterUserRelationWindow");
            }
        }

        private string GetTwitterUserInfoPage(TwitterUserInfo user)
        {
            Page page = new Page();
            HtmlForm form = new HtmlForm();
            form.EnableViewState = false;

            TwitterUserInfoView ctrl = (TwitterUserInfoView)page.LoadControl(PathProvider.GetFileStaticRelativePath("SocialMedia/TwitterUserInfoView.ascx"));
            ctrl.UserInfo = user;
            form.Controls.Add(ctrl);
            page.Controls.Add(form);
            return RenderPage(page);
        }

        [AjaxPro.AjaxMethod]
        public string ShowFacebookUserRelationWindow(string userID)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }

                FacebookApiInfo apiInfo = FacebookApiHelper.GetFacebookApiInfoForCurrentUser();

                if (apiInfo == null)
                    throw new SocialMediaAccountNotFound(SocialMediaResource.SocialMediaAccountNotFoundFacebook);

                FacebookDataProvider provider = new FacebookDataProvider(apiInfo);

                FacebookUserInfo user = provider.LoadUserInfo(userID);
                return GetFacebookUserInfoPage(user);

            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "ShowTwitterUserRelationWindow");
            }
        }

        private string GetFacebookUserInfoPage(FacebookUserInfo user)
        {
            Page page = new Page();
            HtmlForm form = new HtmlForm();
            form.EnableViewState = false;

            FacebookUserInfoView ctrl = (FacebookUserInfoView)page.LoadControl(PathProvider.GetFileStaticRelativePath("SocialMedia/FacebookUserInfoView.ascx"));
            ctrl.UserInfo = user;
            form.Controls.Add(ctrl);
            page.Controls.Add(form);
            return RenderPage(page);
        }

        #endregion

        #region - SaveContactTwitterRelation -

        [AjaxPro.AjaxMethod]
        public void SaveContactTwitterRelation(ContactRelationTwitterSettings settings)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return;
                }

                if (settings == null)
                    throw new ArgumentNullException("Settings can not be null");

                if (settings.RelateAccount == false && settings.RelateAvatar == false)
                    return;

                var contact = Global.DaoFactory.GetContactDao().GetByID(settings.ContactID);

                if (contact == null)
                    throw new Exception("Specified contact not found");

                if (settings.RelateAccount)
                {

                    var twitterAccounts = Global.DaoFactory.GetContactInfoDao().GetListData(contact.ID, ContactInfoType.Twitter);

                    if (!twitterAccounts.Contains(settings.TwitterScreenName))
                        Global.DaoFactory.GetContactInfoDao().Save(new ContactInfo
                                                                                  {
                                                                                      ContactID = contact.ID,
                                                                                      Category = (int)ContactInfoBaseCategory.Other,
                                                                                      InfoType = ContactInfoType.Twitter,
                                                                                      Data = settings.TwitterScreenName
                                                                                  });

                }

                if (settings.RelateAvatar)
                    SaveAvatar(contact.ID, settings.UserAvatarUrl);

            }
            catch (Exception ex)
            {
                _logger.Error("ASC.Web.CRM.SocialMediaUI.SaveContactSocialMediaRelation error:", ex);
                throw new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorInternalServer);
            }
        }

        [AjaxPro.AjaxMethod]
        public void AddTwitterProfileToContact(int contactId, string twitterScreenName)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return;
                }

                ContactDao dao = Global.DaoFactory.GetContactDao();
                Contact contact = dao.GetByID(contactId);

                if (contact == null)
                    throw new Exception("Specified contact not found");

                var twitterAccounts = Global.DaoFactory.GetContactInfoDao().GetListData(contact.ID, ContactInfoType.Twitter);

                if (!twitterAccounts.Contains(twitterScreenName))
                    Global.DaoFactory.GetContactInfoDao().Save(new ContactInfo
                    {
                        ContactID = contact.ID,
                        Category = (int)ContactInfoBaseCategory.Other,
                        InfoType = ContactInfoType.Twitter,
                        Data = twitterScreenName
                    });

            }
            catch (Exception ex)
            {
                _logger.Error("ASC.Web.CRM.SocialMediaUI.SaveContactSocialMediaRelation error:", ex);
                throw new Exception(SocialMediaResource.ErrorInternalServer);
            }
        }

        #endregion

        #region - SaveContactFacebookRelation -

        [AjaxPro.AjaxMethod]
        public void SaveContactFacebookRelation(ContactRelationFacebookSettings settings)
        {
            try
            {

                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return;
                }

                if (settings == null)
                    throw new ArgumentNullException("Settings can not be null");

                if (settings.RelateAvatar == false)
                    return;

                ContactDao dao = Global.DaoFactory.GetContactDao();
                Contact contact = dao.GetByID(settings.ContactID);

                if (contact == null)
                    throw new Exception("Specified contact not found");

                if (settings.RelateAvatar == true)
                    SaveAvatar(contact.ID, settings.UserAvatarUrl);
            }
            catch (Exception ex)
            {
                _logger.Error("ASC.Web.CRM.SocialMediaUI.SaveContactSocialMediaRelation error:", ex);
                throw new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorInternalServer);
            }
        }

        private string SaveAvatar(int contactID, string imageUrl)
        {
            return UploadAvatar(contactID, imageUrl, false);

        }
        private string UploadAvatar(int contactID, string imageUrl, bool uploadOnly)
        {
            if (contactID != 0)
            {
                return ContactPhotoManager.UploadPhoto(imageUrl, contactID, uploadOnly);
            }
            else
            {
                var tmpDirName = Guid.NewGuid().ToString();
                return ContactPhotoManager.UploadPhoto(imageUrl, tmpDirName);
            }
        }

        #endregion

        #region - GetList user images -


        [AjaxPro.AjaxMethod]
        public String GetContactSMImages(int contactID)
        {
            try
            {

                Contact contact = Global.DaoFactory.GetContactDao().GetByID(contactID);

                var images = new List<SocialMediaImageDescription>();

                Func<Contact, Tenant, List<SocialMediaImageDescription>> dlgGetTwitterImageDescriptionList = GetTwitterImageDescriptionList;
                Func<Contact, Tenant, List<SocialMediaImageDescription>> dlgGetFacebookImageDescriptionList = GetFacebookImageDescriptionList;
                Func<Contact, Tenant, List<SocialMediaImageDescription>> dlgGetLinkedInImageDescriptionList = GetLinkedInImageDescriptionList;

                // Parallelizing
                IAsyncResult arGetAvatarsFromTwitter;
                IAsyncResult arGetAvatarsFromFacebook;
                IAsyncResult arGetAvatarsFromLinkedIn;

                var waitHandles = new List<WaitHandle>();

                Tenant currentTenant = CoreContext.TenantManager.GetCurrentTenant();

                arGetAvatarsFromTwitter = dlgGetTwitterImageDescriptionList.BeginInvoke(contact, currentTenant, null, null);
                waitHandles.Add(arGetAvatarsFromTwitter.AsyncWaitHandle);

                arGetAvatarsFromFacebook = dlgGetFacebookImageDescriptionList.BeginInvoke(contact, currentTenant, null, null);
                waitHandles.Add(arGetAvatarsFromFacebook.AsyncWaitHandle);


                arGetAvatarsFromLinkedIn = dlgGetLinkedInImageDescriptionList.BeginInvoke(contact, currentTenant, null, null);
                waitHandles.Add(arGetAvatarsFromLinkedIn.AsyncWaitHandle);

                WaitHandle.WaitAll(waitHandles.ToArray());

                images.AddRange(dlgGetTwitterImageDescriptionList.EndInvoke(arGetAvatarsFromTwitter));
                images.AddRange(dlgGetFacebookImageDescriptionList.EndInvoke(arGetAvatarsFromFacebook));
                images.AddRange(dlgGetLinkedInImageDescriptionList.EndInvoke(arGetAvatarsFromLinkedIn));

                return JsonConvert.SerializeObject(images);
            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "GetContactSMImages");
            }
        }

        private List<SocialMediaImageDescription> GetLinkedInImageDescriptionList(Contact contact, Tenant tenant)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenant);

            var images = new List<SocialMediaImageDescription>();

            var linkedInAccounts = Global.DaoFactory.GetContactInfoDao().GetListData(contact.ID, ContactInfoType.LinkedIn);

            if (linkedInAccounts.Count == 0)
                return images;

            var provider = LinkedInApiHelper.GetLinkedInDataProviderForCurrentUser();

            if (provider == null)
                return images;

            //images.AddRange(from linkedInAccount in linkedInAccounts
            //                let imageUrl = provider.GetUrlOfUserImage(account.UserID)
            //                select new SocialMediaImageDescription
            //                           {
            //                               Identity = account.UserID, ImageUrl = imageUrl, SocialNetwork = ASC.SocialMedia.Core.SocialNetworks.LinkedIn
            //                           });

            return images;
        }

        private List<SocialMediaImageDescription> GetTwitterImageDescriptionList(Contact contact, Tenant tenant)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenant);

            var images = new List<SocialMediaImageDescription>();

            var twitterAccounts = Global.DaoFactory.GetContactInfoDao().GetListData(contact.ID, ContactInfoType.Twitter);

            if (twitterAccounts.Count == 0)
                return images;

            var provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());

            images.AddRange(from twitterAccount in twitterAccounts
                            let imageUrl = provider.GetUrlOfUserImage(twitterAccount, TwitterDataProvider.ImageSize.Small)
                            where imageUrl != null
                            select new SocialMediaImageDescription
                                       {
                                           Identity = twitterAccount,
                                           ImageUrl = imageUrl,
                                           SocialNetwork = SocialNetworks.Twitter
                                       });

            return images;
        }

        private List<SocialMediaImageDescription> GetFacebookImageDescriptionList(Contact contact, Tenant tenant)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenant);

            var images = new List<SocialMediaImageDescription>();

            var facebookAccounts = Global.DaoFactory.GetContactInfoDao().GetListData(contact.ID, ContactInfoType.Facebook);

            if (facebookAccounts.Count == 0)
                return images;

            var provider = new FacebookDataProvider(FacebookApiHelper.GetFacebookApiInfoForCurrentUser());

            images.AddRange(from facebookAccount in facebookAccounts
                            let imageUrl = provider.GetUrlOfUserImage(facebookAccount, FacebookDataProvider.ImageSize.Small)
                            where imageUrl != null
                            select new SocialMediaImageDescription
                                       {
                                           Identity = facebookAccount,
                                           ImageUrl = imageUrl,
                                           SocialNetwork = SocialNetworks.Facebook
                                       });

            return images;
        }

        #endregion

        [AjaxPro.AjaxMethod]
        public string DeleteContactAvatar(int contactId, bool uploadOnly, string type)
        {
            bool isCompany;

            if (contactId != 0)
            {
                var contact = Global.DaoFactory.GetContactDao().GetByID(contactId);
                isCompany = contact is Company;
            }
            else
            {
                isCompany = type != "people";
            }

            if (!uploadOnly)
            {
                ContactPhotoManager.DeletePhoto(contactId);
                return ContactPhotoManager.GetBigSizePhoto(0, isCompany);
            }
            return "";
        }

        [AjaxPro.AjaxMethod]
        public string UploadUserAvatarFromSocialNetwork(int contactID, SocialNetworks socialNetwork, string userIdentity, bool uploadOnly)
        {
            try
            {
                //Process authorization
                if (!ProcessAuthorization(HttpContext.Current))
                {
                    AccessDenied(HttpContext.Current);
                    return null;
                }
                if (socialNetwork == SocialNetworks.Twitter)
                {
                    TwitterDataProvider provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());
                    string imageUrl = provider.GetUrlOfUserImage(userIdentity, TwitterDataProvider.ImageSize.Original);
                    return UploadAvatar(contactID, imageUrl, uploadOnly);
                }
                if (socialNetwork == SocialNetworks.Facebook)
                {
                    FacebookDataProvider provider = new FacebookDataProvider(FacebookApiHelper.GetFacebookApiInfoForCurrentUser());
                    string imageUrl = provider.GetUrlOfUserImage(userIdentity, FacebookDataProvider.ImageSize.Original);
                    return UploadAvatar(contactID, imageUrl, uploadOnly);
                }

                if (socialNetwork == SocialNetworks.LinkedIn)
                {
                    LinkedInDataProvider provider = LinkedInApiHelper.GetLinkedInDataProviderForCurrentUser();
                    string imageUrl = provider.GetUrlOfUserImage(userIdentity);
                    return UploadAvatar(contactID, imageUrl, uploadOnly);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ProcessError(ex, "UploadUserAvatarFromSocialNetwork");
            }
        }

        private static void AccessDenied(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.StatusDescription = "Access Denied";
            string realm = String.Format("Basic Realm=\"{0}\"", context.Request.GetUrlRewriter());
            context.Response.AppendHeader("WWW-Authenticate", realm);
            context.Response.Write("401 Access Denied");
        }

        private static bool ProcessAuthorization(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                //Try studio auth
                try
                {
                    var cookiesKey = CookiesManager.GetCookies(CookiesType.AuthKey);
                    if (!SecurityContext.AuthenticateMe(cookiesKey))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                catch (Exception)
                {

                }
            }
            return SecurityContext.IsAuthenticated;
        }

        private Exception ProcessError(Exception exception, string methodName)
        {
            if (exception is ASC.SocialMedia.Twitter.ConnectionFailureException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorTwitterConnectionFailure);

            if (exception is ASC.SocialMedia.Twitter.RateLimitException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorTwitterRateLimit);

            if (exception is ASC.SocialMedia.Twitter.ResourceNotFoundException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorTwitterAccountNotFound);

            if (exception is ASC.SocialMedia.Twitter.UnauthorizedException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorTwitterUnauthorized);

            if (exception is SocialMediaException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorInternalServer);

            if (exception is ASC.SocialMedia.Facebook.OAuthException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorFacebookOAuth);

            if (exception is ASC.SocialMedia.Facebook.APILimitException)
                return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorFacebookAPILimit);

            if (exception is ASC.Web.CRM.SocialMedia.SocialMediaAccountNotFound)
                return exception;

            string unknownErrorText = String.Format("ASC.Web.CRM.SocialMediaUI.{0} error: Unknown exception:", methodName);
            _logger.Error(unknownErrorText, exception);
            return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorInternalServer);
        }


    }

}
