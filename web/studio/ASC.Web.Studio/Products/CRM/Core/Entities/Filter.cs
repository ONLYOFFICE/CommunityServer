/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnumExtension = ASC.Web.CRM.Classes.EnumExtension;

#endregion

namespace ASC.CRM.Core.Entities
{
    public abstract class FilterObject
    {
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string FilterValue { get; set; }

        public bool IsAsc
        {
            get
            {
                return !String.IsNullOrEmpty(SortOrder) && SortOrder != "descending";
            }
        }

        public abstract ICollection GetItemsByFilter(DaoFactory daofactory);
    }

    public class CasesFilterObject : FilterObject
    {
        public bool? IsClosed { get; set; }
        public List<string> Tags { get; set; }

        public CasesFilterObject()
        {
            SortBy = "title";
            SortOrder = "ascending";
        }

        public CasesFilterObject(string base64String)
        {
            if(string.IsNullOrEmpty(base64String)) return;
            
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

            var jsonArray = json.Split(';');

            foreach (var filterItem in jsonArray)
            {
                var filterObj = JObject.Parse(filterItem);

                var paramString = filterObj.Value<string>("params");

                if (string.IsNullOrEmpty(paramString)) continue;

                var filterParam = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(paramString)));

                switch (filterObj.Value<string>("id"))
                {
                    case "sorter":
                        SortBy = filterParam.Value<string>("id");
                        SortOrder = filterParam.Value<string>("sortOrder");
                        break;

                    case "text":
                        FilterValue = filterParam.Value<string>("value");
                        break;

                    case "closed":
                    case "opened":
                        IsClosed = filterParam.Value<bool>("value");
                        break;

                    case "tags":
                        Tags = filterParam.Value<JArray>("value").ToList().ConvertAll(n => n.ToString());
                        break;
                }
            }
        }

        public override ICollection GetItemsByFilter(DaoFactory daofactory)
        {
            SortedByType sortBy;
            if (!EnumExtension.TryParse(SortBy, true, out sortBy))
            {
                sortBy = SortedByType.Title;
            }

            return daofactory.CasesDao.GetCases(
                FilterValue,
                0,
                IsClosed,
                Tags,
                0, 0,
                new OrderBy(sortBy, IsAsc));
        }
    }

    public class TaskFilterObject : FilterObject
    {
        public int CategoryId { get; set; }
        public int ContactId { get; set; }
        public Guid ResponsibleId { get; set; }
        public bool? IsClosed { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public TaskFilterObject()
        {
            IsClosed = null;
            FromDate = DateTime.MinValue;
            ToDate = DateTime.MinValue;
            SortBy = "deadline";
            SortOrder = "ascending";
        }

        public TaskFilterObject(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return;

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

            var jsonArray = json.Split(';');

            foreach (var filterItem in jsonArray)
            {
                var filterObj = JObject.Parse(filterItem);

                var paramString = filterObj.Value<string>("params");

                if (string.IsNullOrEmpty(paramString)) continue;

                var filterParam = Global.JObjectParseWithDateAsString(Encoding.UTF8.GetString(Convert.FromBase64String(paramString)));

                switch (filterObj.Value<string>("id"))
                {
                    case "sorter":
                        SortBy = filterParam.Value<string>("id");
                        SortOrder = filterParam.Value<string>("sortOrder");
                        break;

                    case "text":
                        FilterValue = filterParam.Value<string>("value");
                        break;

                    case "my":
                    case "responsibleID":
                        ResponsibleId = new Guid(filterParam.Value<string>("value"));
                        break;

                    case "overdue":
                    case "today":
                    case "theNext":
                        var valueString = filterParam.Value<string>("value");
                        var fromToArray = JsonConvert.DeserializeObject<List<string>>(valueString);
                        if (fromToArray.Count != 2) continue;
                        FromDate = !String.IsNullOrEmpty(fromToArray[0])
                                              ? Global.ApiDateTimeParse(fromToArray[0]) : DateTime.MinValue;
                        ToDate = !String.IsNullOrEmpty(fromToArray[1])
                                            ? Global.ApiDateTimeParse(fromToArray[1]) : DateTime.MinValue;
                        break;

                    case "fromToDate":
                        FromDate = filterParam.Value<DateTime>("from");
                        ToDate = (filterParam.Value<DateTime>("to")).AddDays(1).AddSeconds(-1);
                        break;

                    case "categoryID":
                        CategoryId = filterParam.Value<int>("value");
                        break;

                    case "openTask":
                    case "closedTask":
                        IsClosed = filterParam.Value<bool>("value");
                        break;

                    case "contactID":
                        ContactId = filterParam.Value<int>("id");
                        break;
                }
            }
        }

        public override ICollection GetItemsByFilter(DaoFactory daofactory)
        {
            TaskSortedByType sortBy;
            if (!EnumExtension.TryParse(SortBy, true, out sortBy))
            {
                sortBy = TaskSortedByType.DeadLine;
            }

            return daofactory.TaskDao.GetTasks(
                FilterValue,
                ResponsibleId,
                CategoryId,
                IsClosed,
                FromDate,
                ToDate,
                ContactId > 0 ? EntityType.Contact : EntityType.Any,
                ContactId,
                0, 0,
                new OrderBy(sortBy, IsAsc));
        }
    }

    public class DealFilterObject : FilterObject
    {
        public Guid ResponsibleId { get; set; }
        public String StageType { get; set; }
        public int OpportunityStageId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int ContactId { get; set; }
        public bool? ContactAlsoIsParticipant { get; set; }
        public List<string> Tags { get; set; }

        public DealFilterObject()
        {
            ContactAlsoIsParticipant = null;
            FromDate = DateTime.MinValue;
            ToDate = DateTime.MinValue;
            SortBy = "stage";
            SortOrder = "ascending";
        }

        public DealFilterObject(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return;

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

            var jsonArray = json.Split(';');

            foreach (var filterItem in jsonArray)
            {
                var filterObj = JObject.Parse(filterItem);

                var paramString = filterObj.Value<string>("params");

                if (string.IsNullOrEmpty(paramString)) continue;

                var filterParam = Global.JObjectParseWithDateAsString(Encoding.UTF8.GetString(Convert.FromBase64String(paramString)));

                switch (filterObj.Value<string>("id"))
                {
                    case "sorter":
                        SortBy = filterParam.Value<string>("id");
                        SortOrder = filterParam.Value<string>("sortOrder");
                        break;

                    case "text":
                        FilterValue = filterParam.Value<string>("value");
                        break;

                    case "my":
                    case "responsibleID":
                        ResponsibleId = new Guid(filterParam.Value<string>("value"));
                        break;

                    case "stageTypeOpen":
                    case "stageTypeClosedAndWon":
                    case "stageTypeClosedAndLost":
                        StageType = filterParam.Value<string>("value");
                        break;

                    case "opportunityStagesID":
                        OpportunityStageId = filterParam.Value<int>("value");
                        break;

                    case "lastMonth":
                    case "yesterday":
                    case "today":
                    case "thisMonth":
                        var valueString = filterParam.Value<string>("value");
                        var fromToArray = JsonConvert.DeserializeObject<List<string>>(valueString);
                        if (fromToArray.Count != 2) continue;
                        FromDate = Global.ApiDateTimeParse(fromToArray[0]);
                        ToDate = Global.ApiDateTimeParse(fromToArray[1]);
                        break;

                    case "fromToDate":
                        FromDate = Global.ApiDateTimeParse(filterParam.Value<string>("from"));
                        ToDate = Global.ApiDateTimeParse(filterParam.Value<string>("to"));
                        break;

                    case "participantID":
                        ContactId = filterParam.Value<int>("id");
                        ContactAlsoIsParticipant = true;
                        break;

                    case "contactID":
                        ContactId = filterParam.Value<int>("id");
                        ContactAlsoIsParticipant = false;
                        break;

                    case "tags":
                        Tags = filterParam.Value<JArray>("value").ToList().ConvertAll(n => n.ToString());
                        break;
                }
            }

        }

        public override ICollection GetItemsByFilter(DaoFactory daofactory)
        {
            DealSortedByType sortBy;
            EnumExtension.TryParse(SortBy, true, out sortBy);

            DealMilestoneStatus? stageType = null;
            DealMilestoneStatus stage;
            if (EnumExtension.TryParse(StageType, true, out stage))
            {
                stageType = stage;
            }

            return daofactory.DealDao.GetDeals(
                FilterValue,
                ResponsibleId,
                OpportunityStageId,
                Tags,
                ContactId,
                stageType,
                ContactAlsoIsParticipant,
                FromDate,
                ToDate,
                0, 0,
                new OrderBy(sortBy, IsAsc));
        }
    }

    public class ContactFilterObject : FilterObject
    {
        public List<string> Tags { get; set; }
        public string ContactListView { get; set; }
        public int ContactStage { get; set; }
        public int ContactType { get; set; }
        public Guid? ResponsibleId { get; set; }
        public bool? IsShared { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public ContactFilterObject()
        {
            FromDate = DateTime.MinValue;
            ToDate = DateTime.MinValue;
            ResponsibleId = null;
            ContactStage = -1;
            ContactType = -1;
            SortBy = "created";
            SortOrder = "descending";
        }

        public ContactFilterObject(string base64String)
        {
            ContactStage = -1;
            ContactType = -1;
            
            if (string.IsNullOrEmpty(base64String)) return;

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

            var jsonArray = json.Split(';');

            foreach (var filterItem in jsonArray)
            {
                var filterObj = JObject.Parse(filterItem);

                var paramString = filterObj.Value<string>("params");

                if (string.IsNullOrEmpty(paramString)) continue;

                var filterParam = Global.JObjectParseWithDateAsString(Encoding.UTF8.GetString(Convert.FromBase64String(paramString)));

                switch (filterObj.Value<string>("id"))
                {
                    case "sorter":
                        SortBy = filterParam.Value<string>("id");
                        SortOrder = filterParam.Value<string>("sortOrder");
                        break;

                    case "text":
                        FilterValue = filterParam.Value<string>("value");
                        break;

                    case "my":
                    case "responsibleID":
                    case "noresponsible":
                        ResponsibleId = new Guid(filterParam.Value<string>("value"));
                        break;

                    case "tags":
                        Tags = filterParam.Value<JArray>("value").ToList().ConvertAll(n => n.ToString());
                        break;

                    case "withopportunity":
                    case "person":
                    case "company":
                        ContactListView = filterParam.Value<string>("value");
                        break;

                    case "contactType":
                        ContactType = filterParam.Value<int>("value");
                        break;

                    case "contactStage":
                        ContactStage = filterParam.Value<int>("value");
                        break;

                    case "lastMonth":
                    case "yesterday":
                    case "today":
                    case "thisMonth":
                        var valueString = filterParam.Value<string>("value");
                        var fromToArray = JsonConvert.DeserializeObject<List<string>>(valueString);
                        if (fromToArray.Count != 2) continue;
                        FromDate = Global.ApiDateTimeParse(fromToArray[0]);
                        ToDate = Global.ApiDateTimeParse(fromToArray[1]);
                        break;

                    case "fromToDate":
                        FromDate = Global.ApiDateTimeParse(filterParam.Value<string>("from"));
                        ToDate = Global.ApiDateTimeParse(filterParam.Value<string>("to"));
                        break;

                    case "restricted":
                    case "shared":
                        IsShared = filterParam.Value<bool>("value");
                        break;
                }
            }
        }

        public override ICollection GetItemsByFilter(DaoFactory daofactory)
        {
            ContactSortedByType sortBy;
            if (!EnumExtension.TryParse(SortBy, true, out sortBy))
            {
                sortBy = ContactSortedByType.Created;
            }

            ContactListViewType contactListViewType;
            EnumExtension.TryParse(ContactListView, true, out contactListViewType);

            return daofactory.ContactDao.GetContacts(
                FilterValue,
                Tags,
                ContactStage,
                ContactType,
                contactListViewType,
                FromDate,
                ToDate,
                0,
                0,
                new OrderBy(sortBy, IsAsc),
                ResponsibleId,
                IsShared);
        }
    };

    public class InvoiceItemFilterObject : FilterObject
    {
        public bool? InventoryStock { get; set; }

        public InvoiceItemFilterObject()
        {
            InventoryStock = null;
            SortBy = "name";
            SortOrder = "ascending";
        }

        public InvoiceItemFilterObject(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return;

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

            var jsonArray = json.Split(';');

            foreach (var filterItem in jsonArray)
            {
                var filterObj = JObject.Parse(filterItem);

                var paramString = filterObj.Value<string>("params");

                if(string.IsNullOrEmpty(paramString)) continue;

                var filterParam = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(paramString)));

                switch (filterObj.Value<string>("id"))
                {
                    case "sorter":
                        SortBy = filterParam.Value<string>("id");
                        SortOrder = filterParam.Value<string>("sortOrder");
                        break;

                    case "text":
                        FilterValue = filterParam.Value<string>("value");
                        break;

                    case "withInventoryStock":
                    case "withoutInventoryStock":
                        InventoryStock = filterParam.Value<bool>("value");
                        break;
                }
            }

        }

        public override ICollection GetItemsByFilter(DaoFactory daofactory)
        {
            InvoiceItemSortedByType sortBy;
            EnumExtension.TryParse(SortBy, true, out sortBy);

            return daofactory.InvoiceItemDao.GetInvoiceItems(
                FilterValue,
                0,
                InventoryStock,
                0, 0,
                new OrderBy(sortBy, IsAsc));
        }
    }
}