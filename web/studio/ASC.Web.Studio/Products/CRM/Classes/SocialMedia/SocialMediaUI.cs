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

        public List<SocialMediaImageDescription> GetContactSMImages(int contactID)
        {
            Contact contact = Global.DaoFactory.GetContactDao().GetByID(contactID);

            var images = new List<SocialMediaImageDescription>();


            var socialNetworks = Global.DaoFactory.GetContactInfoDao().GetList(contact.ID, null, null, null);

            var twitterAccounts = socialNetworks.Where(sn => sn.InfoType == ContactInfoType.Twitter).Select(sn => sn.Data.Trim()).ToList();
            var facebookAccounts = socialNetworks.Where(sn => sn.InfoType == ContactInfoType.Facebook).Select(sn => sn.Data.Trim()).ToList();

            Func<List<String>, Tenant, List<SocialMediaImageDescription>> dlgGetTwitterImageDescriptionList = GetTwitterImageDescriptionList;
            Func<List<String>, Tenant, List<SocialMediaImageDescription>> dlgGetFacebookImageDescriptionList = GetFacebookImageDescriptionList;

            // Parallelizing
            IAsyncResult arGetAvatarsFromTwitter;
            IAsyncResult arGetAvatarsFromFacebook;

            var waitHandles = new List<WaitHandle>();

            Tenant currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            arGetAvatarsFromTwitter = dlgGetTwitterImageDescriptionList.BeginInvoke(twitterAccounts, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromTwitter.AsyncWaitHandle);

            arGetAvatarsFromFacebook = dlgGetFacebookImageDescriptionList.BeginInvoke(facebookAccounts, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromFacebook.AsyncWaitHandle);

            WaitHandle.WaitAll(waitHandles.ToArray());

            images.AddRange(dlgGetTwitterImageDescriptionList.EndInvoke(arGetAvatarsFromTwitter));
            images.AddRange(dlgGetFacebookImageDescriptionList.EndInvoke(arGetAvatarsFromFacebook));

            return images;
        }

        public List<SocialMediaImageDescription> GetContactSMImages(List<String> twitter, List<String> facebook)
        {
            var images = new List<SocialMediaImageDescription>();

            Func<List<String>, Tenant, List<SocialMediaImageDescription>> dlgGetTwitterImageDescriptionList = GetTwitterImageDescriptionList;
            Func<List<String>, Tenant, List<SocialMediaImageDescription>> dlgGetFacebookImageDescriptionList = GetFacebookImageDescriptionList;

            // Parallelizing
            IAsyncResult arGetAvatarsFromTwitter;
            IAsyncResult arGetAvatarsFromFacebook;

            var waitHandles = new List<WaitHandle>();

            Tenant currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            arGetAvatarsFromTwitter = dlgGetTwitterImageDescriptionList.BeginInvoke(twitter, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromTwitter.AsyncWaitHandle);

            arGetAvatarsFromFacebook = dlgGetFacebookImageDescriptionList.BeginInvoke(facebook, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromFacebook.AsyncWaitHandle);

            WaitHandle.WaitAll(waitHandles.ToArray());

            images.AddRange(dlgGetTwitterImageDescriptionList.EndInvoke(arGetAvatarsFromTwitter));
            images.AddRange(dlgGetFacebookImageDescriptionList.EndInvoke(arGetAvatarsFromFacebook));

            return images;
        }

        private List<SocialMediaImageDescription> GetTwitterImageDescriptionList(List<String> twitterAccounts, Tenant tenant)
        {
            var images = new List<SocialMediaImageDescription>();

            if (twitterAccounts.Count == 0)
                return images;

            try
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);

                var provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());

                twitterAccounts = twitterAccounts.Distinct().ToList();
                images.AddRange(from twitterAccount in twitterAccounts
                                let imageUrl = provider.GetUrlOfUserImage(twitterAccount, TwitterDataProvider.ImageSize.Small)
                                where imageUrl != null
                                select new SocialMediaImageDescription
                                {
                                    Identity = twitterAccount,
                                    ImageUrl = imageUrl,
                                    SocialNetwork = SocialNetworks.Twitter
                                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return images;
        }

        private List<SocialMediaImageDescription> GetFacebookImageDescriptionList(List<String> facebookAccounts, Tenant tenant)
        {
            var images = new List<SocialMediaImageDescription>();

            if (facebookAccounts.Count == 0)
                return images;

            try
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);

                var provider = new FacebookDataProvider(FacebookApiHelper.GetFacebookApiInfoForCurrentUser());
                
                facebookAccounts = facebookAccounts.Distinct().ToList();
                images.AddRange(from facebookAccount in facebookAccounts
                                let imageUrl = provider.GetUrlOfUserImage(facebookAccount, FacebookDataProvider.ImageSize.Small)
                                where imageUrl != null
                                select new SocialMediaImageDescription
                                           {
                                               Identity = facebookAccount,
                                               ImageUrl = imageUrl,
                                               SocialNetwork = SocialNetworks.Facebook
                                           });

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return images;
        }

        #endregion

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
