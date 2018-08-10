/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

    public class ReportTaskState
    {
        public string Id { get; set; }
        public ReportTaskStatus Status { get; set; }
        public ReportType ReportType { get; set; }
        public int Percentage { get; set; }
        public bool IsCompleted { get; set; }
        public string ErrorText { get; set; }
        public string BuilderKey { get; set; }
        public string FileName { get; set; }
        public int FileId { get; set; }
    }
}