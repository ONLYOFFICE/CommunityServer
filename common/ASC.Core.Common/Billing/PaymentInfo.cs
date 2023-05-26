/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

        public int PaymentSystemId { get; set; }

        public string CartId { get; set; }

        public string FName { get; set; }

        public string LName { get; set; }

        public string Email { get; set; }

        public DateTime PaymentDate { get; set; }

        public Decimal Price { get; set; }

        public int Qty { get; set; }

        public string PaymentCurrency { get; set; }

        public string PaymentMethod { get; set; }

        public int QuotaId { get; set; }

        public string ProductRef { get; set; }

        public string CustomerId { get; set; }
    }
}
