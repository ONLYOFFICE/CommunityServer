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

namespace ASC.Core.Billing
{
    [Serializable]
    public class PaymentInfo
    {
        public int ID { get; set; }

        public int Status { get; set; }

        public string PaymentType { get; set; }

        public double ExchangeRate { get; set; }

        public double GrossSum { get; set; }

        public string CartId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public DateTime Date { get; set; }

        public Decimal Price { get; set; }

        public string Currency { get; set; }

        public string Method { get; set; }

        public int QuotaId { get; set; }

        public string ProductId { get; set; }

        public string TenantID { get; set; }

        public string Country { get; set; }

        public Decimal DiscountSum { get; set; }
    }
}
