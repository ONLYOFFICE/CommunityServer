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
using System.Collections.Generic;
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Services.NotifyService;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        /// Returns the detailed information about a task with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">Task</returns>
        /// <short>Get task by ID</short> 
        /// <category>Tasks</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<path>api/2.0/crm/task/{taskid}</path>
        ///<httpMethod>GET</httpMethod>
        [Read(@"task/{taskid:[0-9]+}")]
        public TaskWrapper GetTaskByID(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            var task = DaoFactory.TaskDao.GetByID(taskid);
            if (task == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(task))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            return ToTaskWrapper(task);
        }

        /// <summary>
        /// Returns a list of tasks matching the parameters specified in the request.
        /// </summary>
        /// <param type="System.Guid, System" method="url" optional="true" name="responsibleid">Task responsible ID</param>
        /// <param type="System.Int32, System" method="url" optional="true" name="categoryid">Task category ID</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="isClosed">Task status</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" optional="true" name="fromDate">Earliest task due date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" optional="true" name="toDate">Latest task due date</param>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: opportunity, contact, or case">Related entity type</param>
        /// <param type="System.Int32, System" method="url" name="entityid">Related entity ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Get tasks</short> 
        /// <category>Tasks</category>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">
        /// List of all tasks
        /// </returns>
        /// <path>api/2.0/crm/task/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"task/filter")]
        public IEnumerable<TaskWrapper> GetAllTasks(
            Guid responsibleid,
            int categoryid,
            bool? isClosed,
            ApiDateTime fromDate,
            ApiDateTime toDate,
            string entityType,
            int entityid)
        {
            TaskSortedByType taskSortedByType;

            if (!string.IsNullOrEmpty(entityType) &&
                !(
                     string.Compare(entityType, "contact", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0)
                )
                throw new ArgumentException();

            var searchText = _context.FilterValue;

            IEnumerable<TaskWrapper> result;

            OrderBy taskOrderBy;

            if (Web.CRM.Classes.EnumExtension.TryParse(_context.SortBy, true, out taskSortedByType))
            {
                taskOrderBy = new OrderBy(taskSortedByType, !_context.SortDescending);
            }
            else if (string.IsNullOrEmpty(_context.SortBy))
            {
                taskOrderBy = new OrderBy(TaskSortedByType.DeadLine, true);
            }
            else
            {
                taskOrderBy = null;
            }

            var fromIndex = (int)_context.StartIndex;
            var count = (int)_context.Count;

            if (taskOrderBy != null)
            {
                result = ToTaskListWrapper(
                    DaoFactory
                        .TaskDao
                        .GetTasks(
                            searchText,
                            responsibleid,
                            categoryid,
                            isClosed,
                            fromDate,
                            toDate,
                            ToEntityType(entityType),
                            entityid,
                            fromIndex,
                            count,
                            taskOrderBy)).ToList();

                _context.SetDataPaginated();
                _context.SetDataFiltered();
                _context.SetDataSorted();
            }
            else
                result = ToTaskListWrapper(
                    DaoFactory
                        .TaskDao
                        .GetTasks(
                            searchText,
                            responsibleid,
                            categoryid,
                            isClosed,
                            fromDate,
                            toDate,
                            ToEntityType(entityType),
                            entityid,
                            0,
                            0, null)).ToList();


            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory
                    .TaskDao
                    .GetTasksCount(
                        searchText,
                        responsibleid,
                        categoryid,
                        isClosed,
                        fromDate,
                        toDate,
                        ToEntityType(entityType),
                        entityid);
            }

            _context.SetTotalCount(totalCount);

            return result;
        }

        /// <summary>
        /// Reopens a task with the ID specified in the request.
        /// </summary>
        /// <short>Reopen a task</short> 
        /// <category>Tasks</category>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">
        /// Task
        /// </returns>
        /// <path>api/2.0/crm/task/{taskid}/reopen</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"task/{taskid:[0-9]+}/reopen")]
        public TaskWrapper ReOpenTask(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            DaoFactory.TaskDao.OpenTask(taskid);

            var task = DaoFactory.TaskDao.GetByID(taskid);
            MessageService.Send(Request, MessageAction.CrmTaskOpened, MessageTarget.Create(task.ID), task.Title);

            return ToTaskWrapper(task);
        }

        /// <summary>
        /// Closes a task with the ID specified in the request.
        /// </summary>
        /// <short>Close a task</short> 
        /// <category>Tasks</category>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">
        /// Task
        /// </returns>
        /// <path>api/2.0/crm/task/{taskid}/close</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"task/{taskid:[0-9]+}/close")]
        public TaskWrapper CloseTask(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            DaoFactory.TaskDao.CloseTask(taskid);

            var task = DaoFactory.TaskDao.GetByID(taskid);
            MessageService.Send(Request, MessageAction.CrmTaskClosed, MessageTarget.Create(task.ID), task.Title);

            return ToTaskWrapper(task);
        }

        /// <summary>
        /// Deletes a task with the ID specified in the request.
        /// </summary>
        /// <short>Delete a task</short> 
        /// <category>Tasks</category>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">
        ///  Deleted task
        /// </returns>
        /// <path>api/2.0/crm/task/{taskid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"task/{taskid:[0-9]+}")]
        public TaskWrapper DeleteTask(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            var task = DaoFactory.TaskDao.GetByID(taskid);
            if (task == null) throw new ItemNotFoundException();

            DaoFactory.TaskDao.DeleteTask(taskid);
            MessageService.Send(Request, MessageAction.CrmTaskDeleted, MessageTarget.Create(task.ID), task.Title);

            return ToTaskWrapper(task);
        }

        /// <summary>
        ///  Creates a task with the parameters (title, description, due date, etc.) specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="title">Task title</param>
        /// <param type="System.String, System" optional="true"  name="description">Task description</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="deadline">Task due date</param>
        /// <param type="System.Guid System" name="responsibleId">Task responsible ID</param>
        /// <param type="System.Int32, System" name="categoryId">Task category ID</param>
        /// <param type="System.Int32, System" optional="true"  name="contactId">Contact ID</param>
        /// <param type="System.String, System" optional="true"  name="entityType" remark="Allowed values: opportunity or case">Related entity type</param>
        /// <param type="System.Int32, System" optional="true"  name="entityId">Related entity ID</param>
        /// <param type="System.Boolean, System" optional="true"  name="isNotify">Notifies the responsible about the task or not</param>
        /// <param type="System.Int32, System" optional="true"  name="alertValue">Time period in minutes to remind the responsible of the task</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Create a task</short> 
        /// <category>Tasks</category>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">Task</returns>
        /// <path>api/2.0/crm/task</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"task")]
        public TaskWrapper CreateTask(
            string title,
            string description,
            ApiDateTime deadline,
            Guid responsibleId,
            int categoryId,
            int contactId,
            string entityType,
            int entityId,
            bool isNotify,
            int alertValue
            )
        {
            if (!string.IsNullOrEmpty(entityType) &&
                !(
                     string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0
                 )
                || categoryId <= 0)
                throw new ArgumentException();

            var listItem = DaoFactory.ListItemDao.GetByID(categoryId);
            if (listItem == null) throw new ItemNotFoundException(CRMErrorsResource.TaskCategoryNotFound);

            var task = new Task
            {
                Title = title,
                Description = description,
                ResponsibleID = responsibleId,
                CategoryID = categoryId,
                DeadLine = deadline,
                ContactID = contactId,
                EntityType = ToEntityType(entityType),
                EntityID = entityId,
                IsClosed = false,
                AlertValue = alertValue
            };

            task = DaoFactory.TaskDao.SaveOrUpdateTask(task);

            if (isNotify)
            {
                Contact taskContact = null;
                Cases taskCase = null;
                Deal taskDeal = null;

                if (task.ContactID > 0)
                {
                    taskContact = DaoFactory.ContactDao.GetByID(task.ContactID);
                }

                if (task.EntityID > 0)
                {
                    switch (task.EntityType)
                    {
                        case EntityType.Case:
                            taskCase = DaoFactory.CasesDao.GetByID(task.EntityID);
                            break;
                        case EntityType.Opportunity:
                            taskDeal = DaoFactory.DealDao.GetByID(task.EntityID);
                            break;
                    }
                }

                NotifyClient.Instance.SendAboutResponsibleByTask(task, listItem.Title, taskContact, taskCase, taskDeal, null);
            }

            MessageService.Send(Request, MessageAction.CrmTaskCreated, MessageTarget.Create(task.ID), task.Title);

            return ToTaskWrapper(task);
        }

        /// <summary>
        ///  Creates a group of the same task with the parameters (title, description, due date, etc.) specified in the request for several contacts.
        /// </summary>
        /// <param type="System.String, System" name="title">Task title</param>
        /// <param type="System.String, System" optional="true"  name="description">Task description</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="deadline">Task due date</param>
        /// <param type="System.Guid, System" name="responsibleId">Task responsible ID</param>
        /// <param type="System.Int32, System" name="categoryId">Task category ID</param>
        /// <param type="System.Int32[], System" name="contactId">List of contact IDs</param>
        /// <param type="System.String, System" optional="true"  name="entityType" remark="Allowed values: opportunity or case">Related entity type</param>
        /// <param type="System.Int32, System" optional="true"  name="entityId">Related entity ID</param>
        /// <param type="System.Boolean, System" optional="true" name="isNotify">Notifies the responsible about the task or not</param>
        /// <param type="System.Int32, System" optional="true" name="alertValue">Time period in minutes to remind the responsible of the task</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Create a task group</short> 
        /// <category>Tasks</category>
        /// <returns>Tasks</returns>
        /// <path>api/2.0/crm/contact/task/group</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Create(@"contact/task/group")]
        public IEnumerable<TaskWrapper> CreateTaskGroup(
            string title,
            string description,
            ApiDateTime deadline,
            Guid responsibleId,
            int categoryId,
            int[] contactId,
            string entityType,
            int entityId,
            bool isNotify,
            int alertValue)
        {
            var tasks = new List<Task>();

            if (
                !string.IsNullOrEmpty(entityType) &&
                !(string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                  string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0)
                )
                throw new ArgumentException();

            foreach (var cid in contactId)
            {
                tasks.Add(new Task
                {
                    Title = title,
                    Description = description,
                    ResponsibleID = responsibleId,
                    CategoryID = categoryId,
                    DeadLine = deadline,
                    ContactID = cid,
                    EntityType = ToEntityType(entityType),
                    EntityID = entityId,
                    IsClosed = false,
                    AlertValue = alertValue
                });
            }

            tasks = DaoFactory.TaskDao.SaveOrUpdateTaskList(tasks).ToList();

            string taskCategory = null;
            if (isNotify)
            {
                if (categoryId > 0)
                {
                    var listItem = DaoFactory.ListItemDao.GetByID(categoryId);
                    if (listItem == null) throw new ItemNotFoundException();

                    taskCategory = listItem.Title;
                }
            }

            for (var i = 0; i < tasks.Count; i++)
            {
                if (!isNotify) continue;

                Contact taskContact = null;
                Cases taskCase = null;
                Deal taskDeal = null;

                if (tasks[i].ContactID > 0)
                {
                    taskContact = DaoFactory.ContactDao.GetByID(tasks[i].ContactID);
                }

                if (tasks[i].EntityID > 0)
                {
                    switch (tasks[i].EntityType)
                    {
                        case EntityType.Case:
                            taskCase = DaoFactory.CasesDao.GetByID(tasks[i].EntityID);
                            break;
                        case EntityType.Opportunity:
                            taskDeal = DaoFactory.DealDao.GetByID(tasks[i].EntityID);
                            break;
                    }
                }

                NotifyClient.Instance.SendAboutResponsibleByTask(tasks[i], taskCategory, taskContact, taskCase, taskDeal, null);
            }

            if (tasks.Any())
            {
                var contacts = DaoFactory.ContactDao.GetContacts(contactId);
                var task = tasks.First();
                MessageService.Send(Request, MessageAction.ContactsCreatedCrmTasks, MessageTarget.Create(tasks.Select(x => x.ID)), contacts.Select(x => x.GetTitle()), task.Title);
            }

            return ToTaskListWrapper(tasks);
        }


        /// <summary>
        ///  Updates the selected task with the parameters (title, description, due date, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="taskid">Task ID</param>
        /// <param type="System.String, System" name="title">New task title</param>
        /// <param type="System.String, System" name="description">New task description</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="deadline">New task due date</param>
        /// <param type="System.Guid, System" name="responsibleid">New task responsible ID</param>
        /// <param type="System.Int32, System" name="categoryid">New task category ID</param>
        /// <param type="System.Int32, System" name="contactid">New contact ID</param>
        /// <param type="System.String, System" name="entityType" remark="Allowed values: opportunity or case">New related entity type</param>
        /// <param type="System.Int32, System" name="entityid">New related entity ID</param>
        /// <param type="System.Boolean, System" name="isNotify">Notifies the responsible about the task or not</param>
        /// <param type="System.Int32, System" optional="true"  name="alertValue">New time period in minutes to remind the responsible of the task</param>
        /// <short>Update a task</short> 
        /// <category>Tasks</category>
        /// <returns type="ASC.Api.CRM.Wrappers.TaskWrapper, ASC.Api.CRM">Task</returns>
        /// <path>api/2.0/crm/task/{taskid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"task/{taskid:[0-9]+}")]
        public TaskWrapper UpdateTask(
            int taskid,
            string title,
            string description,
            ApiDateTime deadline,
            Guid responsibleid,
            int categoryid,
            int contactid,
            string entityType,
            int entityid,
            bool isNotify,
            int alertValue)
        {
            if (!string.IsNullOrEmpty(entityType) &&
                !(string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                  string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0
                 ) || categoryid <= 0)
                throw new ArgumentException();

            var listItem = DaoFactory.ListItemDao.GetByID(categoryid);
            if (listItem == null) throw new ItemNotFoundException(CRMErrorsResource.TaskCategoryNotFound);

            var task = new Task
            {
                ID = taskid,
                Title = title,
                Description = description,
                DeadLine = deadline,
                AlertValue = alertValue,
                ResponsibleID = responsibleid,
                CategoryID = categoryid,
                ContactID = contactid,
                EntityID = entityid,
                EntityType = ToEntityType(entityType)
            };


            task = DaoFactory.TaskDao.SaveOrUpdateTask(task);

            if (isNotify)
            {
                Contact taskContact = null;
                Cases taskCase = null;
                Deal taskDeal = null;

                if (task.ContactID > 0)
                {
                    taskContact = DaoFactory.ContactDao.GetByID(task.ContactID);
                }

                if (task.EntityID > 0)
                {
                    switch (task.EntityType)
                    {
                        case EntityType.Case:
                            taskCase = DaoFactory.CasesDao.GetByID(task.EntityID);
                            break;
                        case EntityType.Opportunity:
                            taskDeal = DaoFactory.DealDao.GetByID(task.EntityID);
                            break;
                    }
                }

                NotifyClient.Instance.SendAboutResponsibleByTask(task, listItem.Title, taskContact, taskCase, taskDeal, null);
            }

            MessageService.Send(Request, MessageAction.CrmTaskUpdated, MessageTarget.Create(task.ID), task.Title);

            return ToTaskWrapper(task);
        }

        /// <summary>
        ///  Sets the creation date to the task with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="taskId">Task ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="creationDate">Task creation date</param>
        /// <short>Set the task creation date</short> 
        /// <category>Tasks</category>
        /// <path>api/2.0/crm/task/{taskid}/creationdate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"task/{taskid:[0-9]+}/creationdate")]
        public void SetTaskCreationDate(int taskId, ApiDateTime creationDate)
        {
            var dao = DaoFactory.TaskDao;
            var task = dao.GetByID(taskId);

            if (task == null || !CRMSecurity.CanAccessTo(task))
                throw new ItemNotFoundException();

            dao.SetTaskCreationDate(taskId, creationDate);
        }

        /// <summary>
        ///  Sets the last modified date to the task with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="taskId">Task ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="lastModifedDate">Task last modified date</param>
        /// <short>Set the task last modified date</short> 
        /// <category>Tasks</category>
        /// <path>api/2.0/crm/task/{taskid}/lastmodifeddate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"task/{taskid:[0-9]+}/lastmodifeddate")]
        public void SetTaskLastModifedDate(int taskId, ApiDateTime lastModifedDate)
        {
            var dao = DaoFactory.TaskDao;
            var task = dao.GetByID(taskId);

            if (task == null || !CRMSecurity.CanAccessTo(task))
                throw new ItemNotFoundException();

            dao.SetTaskLastModifedDate(taskId, lastModifedDate);
        }

        private IEnumerable<TaskWrapper> ToTaskListWrapper(IEnumerable<Task> itemList)
        {
            var result = new List<TaskWrapper>();

            var contactIDs = new List<int>();
            var taskIDs = new List<int>();
            var categoryIDs = new List<int>();
            var entityWrappersIDs = new Dictionary<EntityType, List<int>>();

            foreach (var item in itemList)
            {
                taskIDs.Add(item.ID);

                if (!categoryIDs.Contains(item.CategoryID))
                {
                    categoryIDs.Add(item.CategoryID);
                }

                if (item.ContactID > 0 && !contactIDs.Contains(item.ContactID))
                {
                    contactIDs.Add(item.ContactID);
                }

                if (item.EntityID > 0)
                {
                    if (item.EntityType != EntityType.Opportunity && item.EntityType != EntityType.Case) continue;

                    if (!entityWrappersIDs.ContainsKey(item.EntityType))
                    {
                        entityWrappersIDs.Add(item.EntityType, new List<int>
                            {
                                item.EntityID
                            });
                    }
                    else if (!entityWrappersIDs[item.EntityType].Contains(item.EntityID))
                    {
                        entityWrappersIDs[item.EntityType].Add(item.EntityID);
                    }
                }
            }

            var entityWrappers = new Dictionary<string, EntityWrapper>();

            foreach (var entityType in entityWrappersIDs.Keys)
            {
                switch (entityType)
                {
                    case EntityType.Opportunity:
                        DaoFactory.DealDao.GetDeals(entityWrappersIDs[entityType].Distinct().ToArray())
                                  .ForEach(item =>
                                      {
                                          if (item == null) return;

                                          entityWrappers.Add(
                                              string.Format("{0}_{1}", (int)entityType, item.ID),
                                              new EntityWrapper
                                              {
                                                  EntityId = item.ID,
                                                  EntityTitle = item.Title,
                                                  EntityType = "opportunity"
                                              });
                                      });
                        break;
                    case EntityType.Case:
                        DaoFactory.CasesDao.GetByID(entityWrappersIDs[entityType].ToArray())
                                  .ForEach(item =>
                                      {
                                          if (item == null) return;

                                          entityWrappers.Add(
                                              string.Format("{0}_{1}", (int)entityType, item.ID),
                                              new EntityWrapper
                                              {
                                                  EntityId = item.ID,
                                                  EntityTitle = item.Title,
                                                  EntityType = "case"
                                              });
                                      });
                        break;
                }
            }

            var categories = DaoFactory.ListItemDao.GetItems(categoryIDs.ToArray()).ToDictionary(x => x.ID, x => new TaskCategoryBaseWrapper(x));
            var contacts = DaoFactory.ContactDao.GetContacts(contactIDs.ToArray()).ToDictionary(item => item.ID, ToContactBaseWithEmailWrapper);
            var restrictedContacts = DaoFactory.ContactDao.GetRestrictedContacts(contactIDs.ToArray()).ToDictionary(item => item.ID, ToContactBaseWithEmailWrapper);



            foreach (var item in itemList)
            {
                var taskWrapper = new TaskWrapper(item) { CanEdit = CRMSecurity.CanEdit(item) };

                if (contacts.ContainsKey(item.ContactID))
                {
                    taskWrapper.Contact = contacts[item.ContactID];
                }
                if (restrictedContacts.ContainsKey(item.ContactID))
                {
                    taskWrapper.Contact = restrictedContacts[item.ContactID];
                    /*Hide some fields. Should be refactored! */
                    taskWrapper.Contact.Currency = null;
                    taskWrapper.Contact.Email = null;
                    taskWrapper.Contact.AccessList = null;
                }

                if (item.EntityID > 0)
                {
                    var entityStrKey = string.Format("{0}_{1}", (int)item.EntityType, item.EntityID);

                    if (entityWrappers.ContainsKey(entityStrKey))
                    {
                        taskWrapper.Entity = entityWrappers[entityStrKey];
                    }
                }

                if (categories.ContainsKey(item.CategoryID))
                {
                    taskWrapper.Category = categories[item.CategoryID];
                }

                result.Add(taskWrapper);
            }

            return result;
        }


        private TaskWrapper ToTaskWrapper(Task task)
        {
            var result = new TaskWrapper(task);

            if (task.CategoryID > 0)
            {
                result.Category = GetTaskCategoryByID(task.CategoryID);
            }

            if (task.ContactID > 0)
            {
                result.Contact = ToContactBaseWithEmailWrapper(DaoFactory.ContactDao.GetByID(task.ContactID));
            }

            if (task.EntityID > 0)
            {
                result.Entity = ToEntityWrapper(task.EntityType, task.EntityID);
            }

            result.CanEdit = CRMSecurity.CanEdit(task);

            return result;
        }
    }
}