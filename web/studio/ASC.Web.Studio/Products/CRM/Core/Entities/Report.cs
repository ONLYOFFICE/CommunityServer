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


#region Import

using System;
using System.Collections.Generic;
using ASC.VoipService;
using ASC.Web.Core.Files;

#endregion

namespace ASC.CRM.Core.Entities
{
    public class SalesByManager
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }

    public class SalesForecast
    {
        public decimal Value { get; set; }
        public decimal ValueWithProbability { get; set; }
        public DateTime Date { get; set; }
    }

    public class SalesFunnel
    {
        public DealMilestoneStatus Status { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
        public decimal Value { get; set; }
        public int Duration { get; set; }
    }

    public class WorkloadByDeals
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DealMilestoneStatus Status { get; set; }
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class WorkloadByTasks
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int Count { get; set; }
    }

    public class WorkloadByInvoices
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int SentCount { get; set; }
        public int PaidCount { get; set; }
        public int RejectedCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class WorkloadByViop
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public VoipCallStatus Status { get; set; }
        public int Count { get; set; }
        public int Duration { get; set; }
    }

    public class WorkloadByContacts
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int Count { get; set; }
        public int WithDeals { get; set; }
    }
}