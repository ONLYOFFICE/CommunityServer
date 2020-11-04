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

namespace ASC.Core.Billing
{
    public interface ITariffService
    {
        Tariff GetTariff(int tenantId, bool withRequestToPaymentSystem = true);

        void SetTariff(int tenantId, Tariff tariff);

        void DeleteDefaultBillingInfo();

        void ClearCache(int tenantId);

        IEnumerable<PaymentInfo> GetPayments(int tenantId, DateTime from, DateTime to);

        Uri GetShoppingUri(int? tenant, int quotaId, string affiliateId, string currency = null, string language = null, string customerId = null);

        IDictionary<string, IEnumerable<Tuple<string, decimal>>> GetProductPriceInfo(params string[] productIds);

        Invoice GetInvoice(string paymentId);

        string GetButton(int tariffId, string partnerId);

        void SaveButton(int tariffId, string partnerId, string buttonUrl);
    }
}
