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
using System.Collections.Generic;
using System.Linq;
using System.Security;

using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        /// Creates an opportunity stage with the parameters (title, description, success probability, etc.) specified in the request.
        /// </summary>
        /// <param name="title">Stage title</param>
        /// <param name="description">Stage description</param>
        /// <param name="color">Stage color</param>
        /// <param name="successProbability">Stage success probability</param>
        /// <param name="stageType" remark="Allowed values: 0 (Open), 1 (ClosedAndWon), 2 (ClosedAndLost)">Stage type</param>
        /// <short>Create an opportunity stage</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        /// Opportunity stage
        /// </returns>
        [Create(@"opportunity/stage")]
        public DealMilestoneWrapper CreateDealMilestone(
            string title,
            string description,
            string color,
            int successProbability,
            DealMilestoneStatus stageType)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            if (successProbability < 0) successProbability = 0;

            var dealMilestone = new DealMilestone
            {
                Title = title,
                Color = color,
                Description = description,
                Probability = successProbability,
                Status = stageType
            };

            dealMilestone.ID = DaoFactory.DealMilestoneDao.Create(dealMilestone);
            MessageService.Send(Request, MessageAction.OpportunityStageCreated, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return ToDealMilestoneWrapper(dealMilestone);
        }

        /// <summary>
        /// Updates the selected opportunity stage with the parameters (title, description, success probability, etc.) specified in the request.
        /// </summary>
        /// <param name="id">Opportunity stage ID</param>
        /// <param name="title">New stage title</param>
        /// <param name="description">New stage description</param>
        /// <param name="color">New stage color</param>
        /// <param name="successProbability">New stage success probability</param>
        /// <param name="stageType" remark="Allowed values: Open, ClosedAndWon, ClosedAndLost">New stage type</param>
        /// <short>Update an opportunity stage by parameters</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Updated opportunity stage
        /// </returns>
        [Update(@"opportunity/stage/{id:[0-9]+}")]
        public DealMilestoneWrapper UpdateDealMilestone(
            int id,
            string title,
            string description,
            string color,
            int successProbability,
            DealMilestoneStatus stageType)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            if (successProbability < 0) successProbability = 0;

            var curDealMilestoneExist = DaoFactory.DealMilestoneDao.IsExist(id);
            if (!curDealMilestoneExist) throw new ItemNotFoundException();

            var dealMilestone = new DealMilestone
            {
                Title = title,
                Color = color,
                Description = description,
                Probability = successProbability,
                Status = stageType,
                ID = id
            };

            DaoFactory.DealMilestoneDao.Edit(dealMilestone);
            MessageService.Send(Request, MessageAction.OpportunityStageUpdated, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return ToDealMilestoneWrapper(dealMilestone);
        }

        /// <summary>
        /// Updates the selected opportunity stage with the color specified in the request.
        /// </summary>
        /// <param name="id">Opportunity stage ID</param>
        /// <param name="color">New stage color</param>
        /// <short>Update an opportunity stage color</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Opportunity stage with the updated color
        /// </returns>
        [Update(@"opportunity/stage/{id:[0-9]+}/color")]
        public DealMilestoneWrapper UpdateDealMilestoneColor(int id, string color)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var dealMilestone = DaoFactory.DealMilestoneDao.GetByID(id);
            if (dealMilestone == null) throw new ItemNotFoundException();

            dealMilestone.Color = color;

            DaoFactory.DealMilestoneDao.ChangeColor(id, color);
            MessageService.Send(Request, MessageAction.OpportunityStageUpdatedColor, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return ToDealMilestoneWrapper(dealMilestone);
        }

        /// <summary>
        /// Updates the available order of opportunity stages with a list specified in the request.
        /// </summary>
        /// <short>
        /// Update the order of opportunity stages
        /// </short>
        /// <param name="ids">List of opportunity stage IDs</param>
        /// <category>Opportunities</category>
        /// <returns>
        /// Opportunity stages in the new order
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"opportunity/stage/reorder")]
        public IEnumerable<DealMilestoneWrapper> UpdateDealMilestonesOrder(IEnumerable<int> ids)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (ids == null) throw new ArgumentException();

            var idsList = ids.ToList();

            var result = idsList.Select(id => DaoFactory.DealMilestoneDao.GetByID(id)).ToList();

            DaoFactory.DealMilestoneDao.Reorder(idsList.ToArray());
            MessageService.Send(Request, MessageAction.OpportunityStagesUpdatedOrder, MessageTarget.Create(idsList), result.Select(x => x.Title));

            return result.Select(ToDealMilestoneWrapper);
        }

        /// <summary>
        /// Deletes an opportunity stage with the ID specified in the request.
        /// </summary>
        /// <short>Delete an opportunity stage</short> 
        /// <category>Opportunities</category>
        /// <param name="id">Opportunity stage ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Opportunity stage
        /// </returns>
        [Delete(@"opportunity/stage/{id:[0-9]+}")]
        public DealMilestoneWrapper DeleteDealMilestone(int id)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var dealMilestone = DaoFactory.DealMilestoneDao.GetByID(id);
            if (dealMilestone == null) throw new ItemNotFoundException();

            var result = ToDealMilestoneWrapper(dealMilestone);

            DaoFactory.DealMilestoneDao.Delete(id);
            MessageService.Send(Request, MessageAction.OpportunityStageDeleted, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return result;
        }

        /// <summary>
        /// Creates a new history category with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="title">History category title</param>
        ///<param name="description">History category description</param>
        ///<param name="sortOrder">History category order</param>
        ///<param name="imageName">Image name of the history category</param>
        ///<short>Create a history category</short> 
        /// <category>History</category>
        ///<returns>History category</returns>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"history/category")]
        public HistoryCategoryWrapper CreateHistoryCategory(string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName
            };

            listItem.ID = DaoFactory.ListItemDao.CreateItem(ListType.HistoryCategory, listItem);
            MessageService.Send(Request, MessageAction.HistoryEventCategoryCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToHistoryCategoryWrapper(listItem);
        }

        /// <summary>
        /// Updates the selected history category with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="id">History category ID</param>
        ///<param name="title">New history category title</param>
        ///<param name="description">New history category description</param>
        ///<param name="sortOrder">New history category order</param>
        ///<param name="imageName">New image name of the history category </param>
        ///<short>Update a history category</short> 
        ///<category>History</category>
        ///<returns>Updated history category</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"history/category/{id:[0-9]+}")]
        public HistoryCategoryWrapper UpdateHistoryCategory(int id, string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curHistoryCategoryExist = DaoFactory.ListItemDao.IsExist(id);
            if (!curHistoryCategoryExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName,
                ID = id
            };

            DaoFactory.ListItemDao.EditItem(ListType.HistoryCategory, listItem);
            MessageService.Send(Request, MessageAction.HistoryEventCategoryUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToHistoryCategoryWrapper(listItem);
        }

        /// <summary>
        /// Updates an icon of a history category with the ID specified in the request.
        /// </summary>
        /// <param name="id">History category ID</param>
        /// <param name="imageName">New icon name of the history category</param>
        /// <short>Update a history category icon</short> 
        /// <category>History</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// History category with the updated icon
        /// </returns>
        [Update(@"history/category/{id:[0-9]+}/icon")]
        public HistoryCategoryWrapper UpdateHistoryCategoryIcon(int id, string imageName)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var historyCategory = DaoFactory.ListItemDao.GetByID(id);
            if (historyCategory == null) throw new ItemNotFoundException();

            historyCategory.AdditionalParams = imageName;

            DaoFactory.ListItemDao.ChangePicture(id, imageName);
            MessageService.Send(Request, MessageAction.HistoryEventCategoryUpdatedIcon, MessageTarget.Create(historyCategory.ID), historyCategory.Title);

            return ToHistoryCategoryWrapper(historyCategory);
        }

        /// <summary>
        /// Updates the order of the history categories with a list specified in the request.
        /// </summary>
        /// <short>
        /// Update the order of history categories
        /// </short>
        /// <param name="titles">List of history category titles</param>
        /// <category>History</category>
        /// <returns>
        /// History categories in the new order
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"history/category/reorder")]
        public IEnumerable<HistoryCategoryWrapper> UpdateHistoryCategoriesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.ListItemDao.GetByTitle(ListType.HistoryCategory, title)).ToList();

            DaoFactory.ListItemDao.ReorderItems(ListType.HistoryCategory, titles.ToArray());
            MessageService.Send(Request, MessageAction.HistoryEventCategoriesUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToHistoryCategoryWrapper);
        }

        /// <summary>
        /// Deletes a history category with the ID specified in the request.
        /// </summary>
        /// <short>Delete a history category</short> 
        /// <category>History</category>
        /// <param name="id">History category ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <returns>History category</returns>
        [Delete(@"history/category/{id:[0-9]+}")]
        public HistoryCategoryWrapper DeleteHistoryCategory(int id)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var dao = DaoFactory.ListItemDao;
            var listItem = dao.GetByID(id);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.HistoryCategory) < 2)
            {
                throw new ArgumentException("The last history category cannot be deleted");
            }

            var result = ToHistoryCategoryWrapper(listItem);

            dao.DeleteItem(ListType.HistoryCategory, id, 0);
            MessageService.Send(Request, MessageAction.HistoryEventCategoryDeleted, MessageTarget.Create(listItem.ID), listItem.Title);

            return result;
        }

        /// <summary>
        /// Creates a new task category with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="title">Task category title</param>
        ///<param name="description">Task category description</param>
        ///<param name="sortOrder">Task category order</param>
        ///<param name="imageName">Image name of task category</param>
        ///<short>Create a task category</short> 
        ///<category>Tasks</category>
        ///<returns>Task category</returns>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"task/category")]
        public TaskCategoryWrapper CreateTaskCategory(string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName
            };

            listItem.ID = DaoFactory.ListItemDao.CreateItem(ListType.TaskCategory, listItem);
            MessageService.Send(Request, MessageAction.CrmTaskCategoryCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToTaskCategoryWrapper(listItem);
        }

        /// <summary>
        /// Updates the selected task category with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="id">Task category ID</param>
        ///<param name="title">New task category title</param>
        ///<param name="description">New task category description</param>
        ///<param name="sortOrder">New task category order</param>
        ///<param name="imageName">New image name of task category</param>
        ///<short>Update a task category</short> 
        ///<category>Tasks</category>
        ///<returns>Updated task category</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        [Update(@"task/category/{id:[0-9]+}")]
        public TaskCategoryWrapper UpdateTaskCategory(int id, string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curTaskCategoryExist = DaoFactory.ListItemDao.IsExist(id);
            if (!curTaskCategoryExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName,
                ID = id
            };

            DaoFactory.ListItemDao.EditItem(ListType.TaskCategory, listItem);
            MessageService.Send(Request, MessageAction.CrmTaskCategoryUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToTaskCategoryWrapper(listItem);
        }

        /// <summary>
        /// Updates an icon of the task category with the ID specified in the request.
        /// </summary>
        /// <param name="id">Task category ID</param>
        /// <param name="imageName">New icon name of task category</param>
        /// <short>Update a task category icon</short> 
        /// <category>Tasks</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Task category with the updated icon
        /// </returns>
        [Update(@"task/category/{id:[0-9]+}/icon")]
        public TaskCategoryWrapper UpdateTaskCategoryIcon(int id, string imageName)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var taskCategory = DaoFactory.ListItemDao.GetByID(id);
            if (taskCategory == null) throw new ItemNotFoundException();

            taskCategory.AdditionalParams = imageName;

            DaoFactory.ListItemDao.ChangePicture(id, imageName);
            MessageService.Send(Request, MessageAction.CrmTaskCategoryUpdatedIcon, MessageTarget.Create(taskCategory.ID), taskCategory.Title);

            return ToTaskCategoryWrapper(taskCategory);
        }

        /// <summary>
        /// Updates the order of the task categories with a list specified in the request.
        /// </summary>
        /// <short>
        /// Update the order of task categories
        /// </short>
        /// <param name="titles">List of task category titles</param>
        /// <category>Tasks</category>
        /// <returns>
        /// Task categories in the new order
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"task/category/reorder")]
        public IEnumerable<TaskCategoryWrapper> UpdateTaskCategoriesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.ListItemDao.GetByTitle(ListType.TaskCategory, title)).ToList();

            DaoFactory.ListItemDao.ReorderItems(ListType.TaskCategory, titles.ToArray());
            MessageService.Send(Request, MessageAction.CrmTaskCategoriesUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToTaskCategoryWrapper);
        }

        /// <summary>
        /// Deletes a task category with the ID specified in the request.
        /// </summary>
        /// <short>Delete a task category</short> 
        /// <category>Tasks</category>
        /// <param name="categoryid">Task category ID</param>
        /// <param name="newcategoryid">Task category ID to replace the deleted category in the tasks with the current task category</param>
        /// <returns>Task category</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        [Delete(@"task/category/{categoryid:[0-9]+}")]
        public TaskCategoryWrapper DeleteTaskCategory(int categoryid, int newcategoryid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (categoryid <= 0 || newcategoryid < 0) throw new ArgumentException();

            var dao = DaoFactory.ListItemDao;
            var listItem = dao.GetByID(categoryid);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.TaskCategory) < 2)
            {
                throw new ArgumentException("The last task category cannot be deleted");
            }

            dao.DeleteItem(ListType.TaskCategory, categoryid, newcategoryid);
            MessageService.Send(Request, MessageAction.CrmTaskCategoryDeleted, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToTaskCategoryWrapper(listItem);
        }

        /// <summary>
        /// Creates a new contact status with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="title">Contact status title</param>
        ///<param name="description">Contact status description</param>
        ///<param name="color">Contact status color</param>
        ///<param name="sortOrder">Contact status sort order</param>
        ///<returns>Contact status</returns>
        /// <short>Create a contact status</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"contact/status")]
        public ContactStatusWrapper CreateContactStatus(string title, string description, string color, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                Color = color,
                SortOrder = sortOrder
            };

            listItem.ID = DaoFactory.ListItemDao.CreateItem(ListType.ContactStatus, listItem);
            MessageService.Send(Request, MessageAction.ContactTemperatureLevelCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactStatusWrapper(listItem);
        }

        /// <summary>
        /// Updates the selected contact status with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="id">Contact status ID</param>
        ///<param name="title">New contact status title</param>
        ///<param name="description">New contact status description</param>
        ///<param name="color">New contact status color</param>
        ///<param name="sortOrder">New contact status sort order</param>
        ///<returns>Updated contact status</returns>
        /// <short>Update a contact status</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        [Update(@"contact/status/{id:[0-9]+}")]
        public ContactStatusWrapper UpdateContactStatus(int id, string title, string description, string color, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curListItemExist = DaoFactory.ListItemDao.IsExist(id);
            if (!curListItemExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                ID = id,
                Title = title,
                Description = description,
                Color = color,
                SortOrder = sortOrder
            };

            DaoFactory.ListItemDao.EditItem(ListType.ContactStatus, listItem);
            MessageService.Send(Request, MessageAction.ContactTemperatureLevelUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactStatusWrapper(listItem);
        }

        /// <summary>
        /// Updates a color of the selected contact status with a new color specified in the request.
        /// </summary>
        /// <param name="id">Contact status ID</param>
        /// <param name="color">New contact status color</param>
        /// <short>Update a contact status color</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Contact status with the new color
        /// </returns>
        [Update(@"contact/status/{id:[0-9]+}/color")]
        public ContactStatusWrapper UpdateContactStatusColor(int id, string color)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var contactStatus = DaoFactory.ListItemDao.GetByID(id);
            if (contactStatus == null) throw new ItemNotFoundException();

            contactStatus.Color = color;

            DaoFactory.ListItemDao.ChangeColor(id, color);
            MessageService.Send(Request, MessageAction.ContactTemperatureLevelUpdatedColor, MessageTarget.Create(contactStatus.ID), contactStatus.Title);

            return ToContactStatusWrapper(contactStatus);
        }

        /// <summary>
        /// Updates the order of the contact statuses with a list specified in the request.
        /// </summary>
        /// <short>
        /// Update the order of contact statuses
        /// </short>
        /// <param name="titles">List of contact status titles</param>
        /// <category>Contacts</category>
        /// <returns>
        /// Contact statuses in the new order
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"contact/status/reorder")]
        public IEnumerable<ContactStatusWrapper> UpdateContactStatusesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.ListItemDao.GetByTitle(ListType.ContactStatus, title)).ToList();

            DaoFactory.ListItemDao.ReorderItems(ListType.ContactStatus, titles.ToArray());
            MessageService.Send(Request, MessageAction.ContactTemperatureLevelsUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToContactStatusWrapper);
        }

        /// <summary>
        /// Deletes a contact status with the ID specified in the request.
        /// </summary>
        /// <short>Delete a contact status</short> 
        /// <category>Contacts</category>
        /// <param name="contactStatusid">Contact status ID</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        /// <returns>
        /// Contact status
        /// </returns>
        [Delete(@"contact/status/{contactStatusid:[0-9]+}")]
        public ContactStatusWrapper DeleteContactStatus(int contactStatusid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (contactStatusid <= 0) throw new ArgumentException();

            var dao = DaoFactory.ListItemDao;
            var listItem = dao.GetByID(contactStatusid);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.ContactStatus) < 2)
            {
                throw new ArgumentException("The last contact status cannot be deleted");
            }

            var contactStatus = ToContactStatusWrapper(listItem);

            dao.DeleteItem(ListType.ContactStatus, contactStatusid, 0);
            MessageService.Send(Request, MessageAction.ContactTemperatureLevelDeleted, MessageTarget.Create(contactStatus.ID), contactStatus.Title);

            return contactStatus;
        }

        /// <summary>
        /// Returns a contact status with the ID specified in the request.
        /// </summary>
        /// <param name="contactStatusid">Contact status ID</param>
        /// <returns>Contact status</returns>
        /// <short>Get a contact status</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"contact/status/{contactStatusid:[0-9]+}")]
        public ContactStatusWrapper GetContactStatusByID(int contactStatusid)
        {
            if (contactStatusid <= 0) throw new ArgumentException();

            var listItem = DaoFactory.ListItemDao.GetByID(contactStatusid);
            if (listItem == null) throw new ItemNotFoundException();

            return ToContactStatusWrapper(listItem);
        }

        /// <summary>
        /// Creates a new contact type with the parameters (title, etc.) specified in the request.
        /// </summary>
        ///<param name="title">Contact type title</param>
        ///<param name="sortOrder">Contact type sort order</param>
        ///<returns>Contact type</returns>
        /// <short>Create a contact type</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"contact/type")]
        public ContactTypeWrapper CreateContactType(string title, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var listItem = new ListItem
            {
                Title = title,
                Description = string.Empty,
                SortOrder = sortOrder
            };

            listItem.ID = DaoFactory.ListItemDao.CreateItem(ListType.ContactType, listItem);
            MessageService.Send(Request, MessageAction.ContactTypeCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactTypeWrapper(listItem);
        }

        /// <summary>
        /// Updates the selected contact type with the parameters (title, description, etc.) specified in the request.
        /// </summary>
        ///<param name="id">Contact type ID</param>
        ///<param name="title">New contact type title</param>
        ///<param name="sortOrder">New contact type order</param>
        ///<returns>Updated contact type</returns>
        /// <short>Update a contact type</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        [Update(@"contact/type/{id:[0-9]+}")]
        public ContactTypeWrapper UpdateContactType(int id, string title, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curListItemExist = DaoFactory.ListItemDao.IsExist(id);
            if (!curListItemExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                ID = id,
                Title = title,
                SortOrder = sortOrder
            };

            DaoFactory.ListItemDao.EditItem(ListType.ContactType, listItem);
            MessageService.Send(Request, MessageAction.ContactTypeUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactTypeWrapper(listItem);
        }

        /// <summary>
        /// Updates the order of the contact types with a list specified in the request.
        /// </summary>
        /// <short>
        /// Update the order of contact types
        /// </short>
        /// <param name="titles">List of contact type titles</param>
        /// <category>Contacts</category>
        /// <returns>
        /// Contact types in the new order
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"contact/type/reorder")]
        public IEnumerable<ContactTypeWrapper> UpdateContactTypesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.ListItemDao.GetByTitle(ListType.ContactType, title)).ToList();

            DaoFactory.ListItemDao.ReorderItems(ListType.ContactType, titles.ToArray());
            MessageService.Send(Request, MessageAction.ContactTypesUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToContactTypeWrapper);
        }

        /// <summary>
        /// Deletes a contact type with the ID specified in the request.
        /// </summary>
        /// <short>Delete a contact type</short> 
        /// <category>Contacts</category>
        /// <param name="contactTypeid">Contact type ID</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        /// <returns>
        /// Contact type
        /// </returns>
        [Delete(@"contact/type/{contactTypeid:[0-9]+}")]
        public ContactTypeWrapper DeleteContactType(int contactTypeid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (contactTypeid <= 0) throw new ArgumentException();
            var dao = DaoFactory.ListItemDao;

            var listItem = dao.GetByID(contactTypeid);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.ContactType) < 2)
            {
                throw new ArgumentException("The last contact type cannot be deleted");
            }

            var contactType = ToContactTypeWrapper(listItem);

            dao.DeleteItem(ListType.ContactType, contactTypeid, 0);
            MessageService.Send(Request, MessageAction.ContactTypeDeleted, MessageTarget.Create(listItem.ID), listItem.Title);

            return contactType;
        }

        /// <summary>
        /// Returns a contact type with the ID specified in the request.
        /// </summary>
        /// <param name="contactTypeid">Contact type ID</param>
        /// <returns>Contact type</returns>
        /// <short>Get a contact type</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"contact/type/{contactTypeid:[0-9]+}")]
        public ContactTypeWrapper GetContactTypeByID(int contactTypeid)
        {
            if (contactTypeid <= 0) throw new ArgumentException();

            var listItem = DaoFactory.ListItemDao.GetByID(contactTypeid);
            if (listItem == null) throw new ItemNotFoundException();

            return ToContactTypeWrapper(listItem);
        }

        /// <summary>
        /// Returns an opportunity stage with the ID specified in the request.
        /// </summary>
        /// <param name="stageid">Opportunity stage ID</param>
        /// <returns>Opportunity stage</returns>
        /// <short>Get an opportunity stage</short> 
        /// <category>Opportunities</category>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"opportunity/stage/{stageid:[0-9]+}")]
        public DealMilestoneWrapper GetDealMilestoneByID(int stageid)
        {
            if (stageid <= 0) throw new ArgumentException();

            var dealMilestone = DaoFactory.DealMilestoneDao.GetByID(stageid);
            if (dealMilestone == null) throw new ItemNotFoundException();

            return ToDealMilestoneWrapper(dealMilestone);
        }

        /// <summary>
        /// Returns a task category with the ID specified in the request.
        /// </summary>
        /// <param name="categoryid">Task category ID</param>
        /// <returns>Task category</returns>
        /// <short>Get a task category</short> 
        /// <category>Tasks</category>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"task/category/{categoryid:[0-9]+}")]
        public TaskCategoryWrapper GetTaskCategoryByID(int categoryid)
        {
            if (categoryid <= 0) throw new ArgumentException();

            var listItem = DaoFactory.ListItemDao.GetByID(categoryid);
            if (listItem == null) throw new ItemNotFoundException();

            return ToTaskCategoryWrapper(listItem);
        }

        /// <summary>
        /// Returns a list of all the history categories available on the portal.
        /// </summary>
        /// <short>Get all history categories</short> 
        /// <category>History</category>
        /// <returns>
        /// List of all the history categories
        /// </returns>
        [Read(@"history/category")]
        public IEnumerable<HistoryCategoryWrapper> GetHistoryCategoryWrapper()
        {
            var result = DaoFactory.ListItemDao.GetItems(ListType.HistoryCategory).ConvertAll(item => new HistoryCategoryWrapper(item));

            var relativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.HistoryCategory);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.ID))
                        x.RelativeItemsCount = relativeItemsCount[x.ID];
                });
            return result;
        }

        /// <summary>
        /// Returns a list of all the task categories available on the portal.
        /// </summary>
        /// <short>Get all task categories</short> 
        /// <category>Tasks</category>
        /// <returns>
        /// List of all the task categories
        /// </returns>
        [Read(@"task/category")]
        public IEnumerable<TaskCategoryWrapper> GetTaskCategories()
        {
            var result = DaoFactory.ListItemDao.GetItems(ListType.TaskCategory).ConvertAll(item => new TaskCategoryWrapper(item));

            var relativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.TaskCategory);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.ID))
                        x.RelativeItemsCount = relativeItemsCount[x.ID];
                });
            return result;
        }

        /// <summary>
        /// Returns a list of all the contact statuses available on the portal.
        /// </summary>
        /// <short>Get all contact statuses</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///List of all the contact statuses
        /// </returns>
        [Read(@"contact/status")]
        public IEnumerable<ContactStatusWrapper> GetContactStatuses()
        {
            var result = DaoFactory.ListItemDao.GetItems(ListType.ContactStatus).ConvertAll(item => new ContactStatusWrapper(item));

            var relativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.ContactStatus);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.ID))
                        x.RelativeItemsCount = relativeItemsCount[x.ID];
                });
            return result;
        }

        /// <summary>
        /// Returns a list of all the contact types available on the portal.
        /// </summary>
        /// <short>Get all contact types</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// List of all the contact types
        /// </returns>
        [Read(@"contact/type")]
        public IEnumerable<ContactTypeWrapper> GetContactTypes()
        {
            var result = DaoFactory.ListItemDao.GetItems(ListType.ContactType).ConvertAll(item => new ContactTypeWrapper(item));

            var relativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.ContactType);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.ID))
                        x.RelativeItemsCount = relativeItemsCount[x.ID];
                });

            return result;
        }

        /// <summary>
        /// Returns a list of all the opportunity stages available on the portal.
        /// </summary>
        /// <short>Get all opportunity stages</short> 
        /// <category>Opportunities</category>
        /// <returns>
        /// List of all the opportunity stages
        /// </returns>
        [Read(@"opportunity/stage")]
        public IEnumerable<DealMilestoneWrapper> GetDealMilestones()
        {
            var result = DaoFactory.DealMilestoneDao.GetAll().ConvertAll(item => new DealMilestoneWrapper(item));

            var relativeItemsCount = DaoFactory.DealMilestoneDao.GetRelativeItemsCount();

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.ID))
                        x.RelativeItemsCount = relativeItemsCount[x.ID];
                });

            return result;
        }

        public ContactStatusWrapper ToContactStatusWrapper(ListItem listItem)
        {
            var result = new ContactStatusWrapper(listItem)
            {
                RelativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.ContactStatus, listItem.ID)
            };

            return result;
        }

        public ContactTypeWrapper ToContactTypeWrapper(ListItem listItem)
        {
            var result = new ContactTypeWrapper(listItem)
            {
                RelativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.ContactType, listItem.ID)
            };

            return result;
        }

        public HistoryCategoryWrapper ToHistoryCategoryWrapper(ListItem listItem)
        {
            var result = new HistoryCategoryWrapper(listItem)
            {
                RelativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.HistoryCategory, listItem.ID)
            };
            return result;
        }

        public TaskCategoryWrapper ToTaskCategoryWrapper(ListItem listItem)
        {
            var result = new TaskCategoryWrapper(listItem)
            {
                RelativeItemsCount = DaoFactory.ListItemDao.GetRelativeItemsCount(ListType.TaskCategory, listItem.ID)
            };
            return result;
        }

        private DealMilestoneWrapper ToDealMilestoneWrapper(DealMilestone dealMilestone)
        {
            var result = new DealMilestoneWrapper(dealMilestone)
            {
                RelativeItemsCount = DaoFactory.DealMilestoneDao.GetRelativeItemsCount(dealMilestone.ID)
            };
            return result;
        }
    }
}