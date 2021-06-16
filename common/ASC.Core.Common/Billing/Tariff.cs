/*
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
using System.Diagnostics;

using ASC.Core.Tenants;

namespace ASC.Core.Billing
{
    [DebuggerDisplay("{QuotaId} ({State} before {DueDate})")]
    [Serializable]
    public class Tariff
    {
        public int QuotaId { get; set; }

        public TariffState State { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime DelayDueDate { get; set; }

        public DateTime LicenseDate { get; set; }

        public bool Autorenewal { get; set; }

        public bool Prolongable { get; set; }

        public int Quantity { get; set; }


        public static Tariff CreateDefault()
        {
            return new Tariff
            {
                QuotaId = Tenant.DEFAULT_TENANT,
                State = TariffState.Paid,
                DueDate = DateTime.MaxValue,
                DelayDueDate = DateTime.MaxValue,
                LicenseDate = DateTime.MaxValue,
                Quantity = 1
            };
        }


        public override int GetHashCode()
        {
            return QuotaId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var t = obj as Tariff;
            return t != null && t.QuotaId == QuotaId;
        }

        public bool EqualsByParams(Tariff t)
        {
            return t != null
                && t.QuotaId == QuotaId
                && t.DueDate == DueDate
                && t.Quantity == Quantity;
        }
    }
}