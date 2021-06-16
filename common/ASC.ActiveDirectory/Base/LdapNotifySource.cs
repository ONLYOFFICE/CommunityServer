﻿/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Core.Tenants;
using ASC.Notify.Model;

namespace ASC.ActiveDirectory.Base
{
    class LdapNotifySource : INotifySource
    {
        public Tenant Tenant { get; set; }

        public string ID
        {
            get { return "asc.activedirectory." + Tenant.TenantId; }
        }

        public LdapNotifySource(Tenant tenant)
        {
            Tenant = tenant;
        }

        public void AutoSync(DateTime date)
        {
            LdapNotifyHelper.AutoSync(Tenant);
        }

        public IActionProvider GetActionProvider()
        {
            throw new NotImplementedException();
        }

        public Notify.Patterns.IPatternProvider GetPatternProvider()
        {
            throw new NotImplementedException();
        }

        public Notify.Recipients.IRecipientProvider GetRecipientsProvider()
        {
            throw new NotImplementedException();
        }

        public ISubscriptionProvider GetSubscriptionProvider()
        {
            throw new NotImplementedException();
        }
    }
}
