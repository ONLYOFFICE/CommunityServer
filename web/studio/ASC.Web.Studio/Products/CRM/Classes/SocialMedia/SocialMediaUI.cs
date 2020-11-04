/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Thrdparty;
using ASC.Thrdparty.Twitter;
using ASC.Web.CRM.Classes.SocialMedia;
using ASC.Web.CRM.Resources;


namespace ASC.Web.CRM.SocialMedia
{
    public class SocialMediaUI
    {
        private ILog _logger = LogManager.GetLogger("ASC");
        private DaoFactory DaoFactory { get; set; }

        public SocialMediaUI(DaoFactory factory)
        {
            DaoFactory = factory;
        }

        public List<SocialMediaImageDescription> GetContactSMImages(int contactID)
        {
            var contact = DaoFactory.ContactDao.GetByID(contactID);

            var images = new List<SocialMediaImageDescription>();


            var socialNetworks = DaoFactory.ContactInfoDao.GetList(contact.ID, null, null, null);

            var twitterAccounts = socialNetworks.Where(sn => sn.InfoType == ContactInfoType.Twitter).Select(sn => sn.Data.Trim()).ToList();

            Func<List<String>, Tenant, List<SocialMediaImageDescription>> dlgGetTwitterImageDescriptionList = GetTwitterImageDescriptionList;

            // Parallelizing

            var waitHandles = new List<WaitHandle>();

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            var arGetAvatarsFromTwitter = dlgGetTwitterImageDescriptionList.BeginInvoke(twitterAccounts, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromTwitter.AsyncWaitHandle);

            WaitHandle.WaitAll(waitHandles.ToArray());

            images.AddRange(dlgGetTwitterImageDescriptionList.EndInvoke(arGetAvatarsFromTwitter));

            return images;
        }

        public List<SocialMediaImageDescription> GetContactSMImages(List<String> twitter)
        {
            var images = new List<SocialMediaImageDescription>();

            Func<List<String>, Tenant, List<SocialMediaImageDescription>> dlgGetTwitterImageDescriptionList = GetTwitterImageDescriptionList;

            // Parallelizing

            var waitHandles = new List<WaitHandle>();

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            var arGetAvatarsFromTwitter = dlgGetTwitterImageDescriptionList.BeginInvoke(twitter, currentTenant, null, null);
            waitHandles.Add(arGetAvatarsFromTwitter.AsyncWaitHandle);

            WaitHandle.WaitAll(waitHandles.ToArray());

            images.AddRange(dlgGetTwitterImageDescriptionList.EndInvoke(arGetAvatarsFromTwitter));

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

        public Exception ProcessError(Exception exception, string methodName)
        {
            if (exception is ConnectionFailureException)
                return new Exception(CRMSocialMediaResource.ErrorTwitterConnectionFailure);

            if (exception is RateLimitException)
                return new Exception(CRMSocialMediaResource.ErrorTwitterRateLimit);

            if (exception is ResourceNotFoundException)
                return new Exception(CRMSocialMediaResource.ErrorTwitterAccountNotFound);

            if (exception is UnauthorizedException)
                return new Exception(CRMSocialMediaResource.ErrorTwitterUnauthorized);

            if (exception is SocialMediaException)
                return new Exception(CRMSocialMediaResource.ErrorInternalServer);

            if (exception is SocialMediaAccountNotFound)
                return exception;

            var unknownErrorText = String.Format("{0} error: Unknown exception:", methodName);
            _logger.Error(unknownErrorText, exception);
            return new Exception(CRMSocialMediaResource.ErrorInternalServer);
        }
    }
}