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
using System.Globalization;
using ASC.Core.Users;

namespace ASC.Core.Tenants
{
    public class TenantRegistrationInfo
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public CultureInfo Culture { get; set; }

        public TimeZoneInfo TimeZoneInfo { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string MobilePhone { get; set; }

        public string PasswordHash { get; set; }

        public EmployeeActivationStatus ActivationStatus { get; set; }

        public string HostedRegion { get; set; }

        public string PartnerId { get; set; }

        public string AffiliateId { get; set; }

        public TenantIndustry Industry { get; set; }

        public bool Spam { get; set; }

        public bool Calls { get; set; }

        public bool Analytics { get; set; }

        public string Campaign { get; set; }

        public bool LimitedControlPanel { get; set; }


        public TenantRegistrationInfo()
        {
            Culture = CultureInfo.CurrentCulture;
            TimeZoneInfo = TimeZoneInfo.Local;
        }
    }
}
