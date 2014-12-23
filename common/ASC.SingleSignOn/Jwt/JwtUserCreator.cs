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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.SingleSignOn.Common;
using log4net;
using System;
using System.Security.Claims;

namespace ASC.SingleSignOn.Jwt
{
    public class JwtUserCreator
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(JwtUserCreator));
        private const int MAX_NUMBER_OF_SYMBOLS = 64;

        public UserInfo CreateUserInfo(ClaimsPrincipal claimsPrincipal, string profile)
        {
            Claim extUserIdClaim = claimsPrincipal.FindFirst(x => x.Type == SupportedClaimTypes.ExtUserId);
            var email = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.Email).Value;
            UserInfo userInfo = null;

            if (extUserIdClaim != null && !string.IsNullOrWhiteSpace(extUserIdClaim.Value))
            {
                var wrapper = new CommonDbWrapper();
                var userId = wrapper.GetUserId(extUserIdClaim.Value, profile);
                if (userId != null)
                {
                    _log.DebugFormat("Updating new user with extUserId: {0}", extUserIdClaim.Value);
                    userInfo = CoreContext.UserManager.GetUsers(Guid.Parse(userId));

                    while (true)
                    {
                        int count = 0;
                        var userByEmail = CoreContext.UserManager.GetUserByEmail(email);
                        if (userByEmail == Constants.LostUser || userByEmail.ID == userInfo.ID)
                        {
                            break;
                        }
                        email += count++;
                    }
                }
                if (userId == null || userInfo == Constants.LostUser)
                {
                    _log.DebugFormat("Creating new user with extUserId: {0}", extUserIdClaim.Value);
                    userInfo = new UserInfo();
                    wrapper.SaveExtUserId(extUserIdClaim.Value, userInfo.ID.ToString(), profile);
                    while (true)
                    {
                        int count = 0;
                        if (CoreContext.UserManager.GetUserByEmail(email) == Constants.LostUser)
                        {
                            break;
                        }
                        email += count++;
                    }
                }
            }
            else
            {
                userInfo = CoreContext.UserManager.GetUserByEmail(email);
                if (userInfo == Constants.LostUser)
                {
                    _log.DebugFormat("Creating new user with email: {0}", email);
                    userInfo = new UserInfo();
                }
                else
                {
                    _log.DebugFormat("Updating user with email: {0}", email);
                }
            }
            Claim givenNameClaim = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.GivenName);
            Claim surNameClaim = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.Surname);
            Claim mobilePhoneClaim = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.MobilePhone);
            Claim titleClaim = claimsPrincipal.FindFirst(x => x.Type == SupportedClaimTypes.Title);
            Claim locationClaim = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.StreetAddress);
            Claim birthDateClaim = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.DateOfBirth);
            Claim sexClaim = claimsPrincipal.FindFirst(x => x.Type == SupportedClaimTypes.Sex);

            userInfo.ActivationStatus = EmployeeActivationStatus.Activated;
            userInfo.Email = email;
            userInfo.FirstName = givenNameClaim != null ? givenNameClaim.Value : string.Empty;
            userInfo.LastName = surNameClaim != null ? surNameClaim.Value : string.Empty;
            userInfo.MobilePhone = mobilePhoneClaim != null ? mobilePhoneClaim.Value : string.Empty;
            userInfo.Title = titleClaim != null ? titleClaim.Value : string.Empty;
            userInfo.Location = locationClaim != null ? locationClaim.Value : string.Empty;
            string firstName = givenNameClaim != null ? givenNameClaim.Value : string.Empty;
            string lastName = surNameClaim != null ? surNameClaim.Value : string.Empty;
            string mobilePhone = mobilePhoneClaim != null ? mobilePhoneClaim.Value : string.Empty;
            string title = titleClaim != null ? titleClaim.Value : string.Empty;
            string location = locationClaim != null ? locationClaim.Value : string.Empty;
            string birthDateString = birthDateClaim != null ? birthDateClaim.Value : null;
            string sexString = sexClaim != null ? sexClaim.Value : null;

            if (!string.IsNullOrEmpty(firstName))
            {
                if (firstName.Length > MAX_NUMBER_OF_SYMBOLS)
                {
                    firstName = firstName.Substring(0, MAX_NUMBER_OF_SYMBOLS);
                }
                userInfo.FirstName = firstName;
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                if (lastName.Length > MAX_NUMBER_OF_SYMBOLS)
                {
                    lastName = lastName.Substring(0, MAX_NUMBER_OF_SYMBOLS);
                }
                userInfo.LastName = lastName;
            }
            if (!string.IsNullOrEmpty(mobilePhone))
            {
                userInfo.MobilePhone = mobilePhone;
            }
            if (!string.IsNullOrEmpty(title))
            {
                userInfo.Title = title;
            }
            if (!string.IsNullOrEmpty(location))
            {
                userInfo.Location = location;
            }
            if (!string.IsNullOrEmpty(birthDateString))
            {
                try
                {
                    userInfo.BirthDate = DateTime.Parse(birthDateString);
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Parse birthDateString error: {0}, {1}", e, birthDateString);
                }
            }

            if (!string.IsNullOrEmpty(sexString))
            {
                try
                {
                    userInfo.Sex = Convert.ToBoolean(sexString);
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Parse sexString error: {0}, {1}", e, sexString);
                }
            }

            if (!userInfo.WorkFromDate.HasValue)
            {
                userInfo.WorkFromDate = TenantUtil.DateTimeNow();
            }

            return userInfo;
        }
    }
}