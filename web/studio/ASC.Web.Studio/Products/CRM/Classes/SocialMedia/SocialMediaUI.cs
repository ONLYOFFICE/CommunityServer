/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
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
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.SocialMedia.Resources;
using ASC.Web.UserControls.SocialMedia.UserControls;
using log4net;
using Newtonsoft.Json;
using ASC.CRM.Core;
using ASC.Thrdparty.Configuration;

namespace ASC.Web.CRM.SocialMedia
{
    public class SocialMediaUI
    {
        private ILog _logger = LogManager.GetLogger(SocialMediaConstants.LoggerName);

        #region - GetList user images -

        public String GetContactSMImages(int contactID)
        {
            Contact contact = Global.DaoFactory.GetContactDao().GetByID(contactID);

            var images = new List<SocialMediaImageDescription>();

            Func<Contact, Tenant, List<SocialMediaImageDescription>> dlgGetTwitterImageDescriptionList = GetTwitterImageDescriptionList;
            Func<Contact, Tenant, List<SocialMediaImageDescription>> dlgGetFacebookImageDescriptionList = GetFacebookImageDescriptionList;
            Func<Contact, Tenant, List<SocialMediaImageDescription>> dlgGetLinkedInImageDescriptionList = GetLinkedInImageDescriptionList;

            // Parallelizing
            IAsyncResult arGetAvatarsFromTwitter;
            IAsyncResult arGetAvatarsFromFacebook;
            //IAsyncResult arGetAvatarsFromLinkedIn;

            var waitHandles = new List<WaitHandle>();

            Tenant currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            arGetAvatarsFromTwitter = dlgGetTwitterImageDescriptionList.BeginInvoke(contact, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromTwitter.AsyncWaitHandle);

            arGetAvatarsFromFacebook = dlgGetFacebookImageDescriptionList.BeginInvoke(contact, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromFacebook.AsyncWaitHandle);


            //arGetAvatarsFromLinkedIn = dlgGetLinkedInImageDescriptionList.BeginInvoke(contact, currentTenant, null, null);
            //waitHandles.Add(arGetAvatarsFromLinkedIn.AsyncWaitHandle);

            WaitHandle.WaitAll(waitHandles.ToArray());

            images.AddRange(dlgGetTwitterImageDescriptionList.EndInvoke(arGetAvatarsFromTwitter));
            images.AddRange(dlgGetFacebookImageDescriptionList.EndInvoke(arGetAvatarsFromFacebook));
            //images.AddRange(dlgGetLinkedInImageDescriptionList.EndInvoke(arGetAvatarsFromLinkedIn));

            return JsonConvert.SerializeObject(images);
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


        public string FindContactByName(string searchUrl, string contactNamespace)
        {
            var crunchBaseKey = KeyStorage.Get("crunchBaseKey");

            if (!string.IsNullOrEmpty(crunchBaseKey))
            {
                crunchBaseKey = string.Format("user_key={0}", crunchBaseKey);
                searchUrl += "&" + crunchBaseKey;
            }

            var findGet = System.Net.WebRequest.Create(searchUrl);
            var findResp = findGet.GetResponse();

            if (findResp != null)
            {
                var findStream = findResp.GetResponseStream();
                if (findStream != null)
                {
                    var sr = new System.IO.StreamReader(findStream);
                    var s = sr.ReadToEnd();
                    var permalink = Newtonsoft.Json.Linq.JObject.Parse(s)["permalink"].ToString().HtmlEncode();

                    searchUrl = @"http://api.crunchbase.com/v/2/" + contactNamespace + "/" + permalink + ".js";
                    if (!string.IsNullOrEmpty(crunchBaseKey))
                    {
                        searchUrl += "?" + crunchBaseKey;
                    }

                    var infoGet = System.Net.WebRequest.Create(searchUrl);
                    var infoResp = infoGet.GetResponse();

                    if (infoResp != null)
                    {
                        var infoStream = infoResp.GetResponseStream();
                        if (infoStream != null)
                        {
                            sr = new System.IO.StreamReader(infoStream);
                            s = sr.ReadToEnd();
                            return s;
                        }
                    }
                    s = sr.ReadToEnd();

                    return s;
                }
            }
            return string.Empty;
        }

        public Exception ProcessError(Exception exception, string methodName)
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

            string unknownErrorText = String.Format("{0} error: Unknown exception:", methodName);
            _logger.Error(unknownErrorText, exception);
            return new Exception(ASC.Web.UserControls.SocialMedia.Resources.SocialMediaResource.ErrorInternalServer);
        }

    }

}
