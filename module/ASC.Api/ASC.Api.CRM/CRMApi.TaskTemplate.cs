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
using ASC.Api.Employee;
using ASC.Api.Exceptions;
using ASC.CRM.Core.Entities;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        /// Creates a new task template container with the type and title specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="entityType">Entity type</param>
        /// <param type="System.String, System" name="title">Template container title</param>
        /// <short>Create a task template container</short> 
        /// <category>Task templates</category>
        /// <returns>
        /// Task template container
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/crm/{entityType}/tasktemplatecontainer</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create(@"{entityType:(contact|person|company|opportunity|case)}/tasktemplatecontainer")]
        public TaskTemplateContainerWrapper CreateTaskTemplateContainer(string entityType, string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            var taskTemplateContainer = new TaskTemplateContainer
            {
                EntityType = ToEntityType(entityType),
                Title = title
            };

            taskTemplateContainer.ID = DaoFactory.TaskTemplateContainerDao.SaveOrUpdate(taskTemplateContainer);
            return ToTaskTemplateContainerWrapper(taskTemplateContainer);
        }

        /// <summary>
        /// Returns a complete list of all the task template containers available on the portal.
        /// </summary>
        /// <param type="System.String, System" name="entityType">Entity type</param>
        /// <short>Get task template containers</short> 
        /// <category>Task templates</category>
        /// <returns>
        /// List of task template containers
        /// </returns>
        /// <path>api/2.0/crm/{entityType}/tasktemplatecontainer</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Read(@"{entityType:(contact|person|company|opportunity|case)}/tasktemplatecontainer")]
        public IEnumerable<TaskTemplateContainerWrapper> GetTaskTemplateContainers(string entityType)
        {
            return ToTaskListTemplateContainerWrapper(DaoFactory.TaskTemplateContainerDao.GetItems(ToEntityType(entityType)));
        }

        /// <summary>
        /// Deletes a task template container with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="containerid">Task template container ID</param>
        /// <short>Delete a task template container</short> 
        /// <category>Task templates</category>
        /// <returns>
        /// Deleted task template container
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/{containerid}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <visible>false</visible>
        [Delete(@"tasktemplatecontainer/{containerid:[0-9]+}")]
        public TaskTemplateContainerWrapper DeleteTaskTemplateContainer(int containerid)
        {
            if (containerid <= 0) throw new ArgumentException();

            var result = ToTaskTemplateContainerWrapper(DaoFactory.TaskTemplateContainerDao.GetByID(containerid));
            if (result == null) throw new ItemNotFoundException();

            DaoFactory.TaskTemplateContainerDao.Delete(containerid);

            return result;
        }

        /// <summary>
        /// Updates a task template container with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="containerid">Task template container ID</param>
        /// <param type="System.String, System" name="title">New template container title</param>
        /// <short>Update a task template container</short> 
        /// <category>Task templates</category>
        /// <returns>
        /// Updated task template container
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/{containerid}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"tasktemplatecontainer/{containerid:[0-9]+}")]
        public TaskTemplateContainerWrapper UpdateTaskTemplateContainer(int containerid, string title)
        {
            if (containerid <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var result = DaoFactory.TaskTemplateContainerDao.GetByID(containerid);
            if (result == null) throw new ItemNotFoundException();

            result.Title = title;

            DaoFactory.TaskTemplateContainerDao.SaveOrUpdate(result);

            return ToTaskTemplateContainerWrapper(result);
        }

        /// <summary>
        /// Returns the detailed information on the task template container with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="containerid">Task template container ID</param>
        /// <short>Get a task template container by ID</short> 
        /// <category>Task templates</category>
        /// <returns>
        /// Task template container
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/{containerid}</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"tasktemplatecontainer/{containerid:[0-9]+}")]
        public TaskTemplateContainerWrapper GetTaskTemplateContainerByID(int containerid)
        {
            if (containerid <= 0) throw new ArgumentException();

            var item = DaoFactory.TaskTemplateContainerDao.GetByID(containerid);
            if (item == null) throw new ItemNotFoundException();

            return ToTaskTemplateContainerWrapper(item);
        }

        /// <summary>
        /// Returns a list of all the tasks in the container with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="containerid">Task template container ID</param>
        /// <short>Get container tasks</short> 
        /// <category>Task templates</category>
        /// <returns>
        /// List of task templates
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/{containerid}/tasktemplate</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Read(@"tasktemplatecontainer/{containerid:[0-9]+}/tasktemplate")]
        public IEnumerable<TaskTemplateWrapper> GetTaskTemplates(int containerid)
        {
            if (containerid <= 0) throw new ArgumentException();

            var container = DaoFactory.TaskTemplateContainerDao.GetByID(containerid);
            if (container == null) throw new ItemNotFoundException();

            return DaoFactory.TaskTemplateDao.GetList(containerid).ConvertAll(ToTaskTemplateWrapper);
        }

        /// <summary>
        /// Creates a new task template with the parameters specified in the request in the container with the selected ID.
        /// </summary>
        /// <param type="System.Int32, System" name="containerid">Task template container ID</param>
        /// <param type="System.String, System" name="title">Task template title</param>
        /// <param type="System.String, System" name="description">Task template description</param>
        /// <param type="System.Guid, System" name="responsibleid">Task template responsible ID</param>
        /// <param type="System.Int32, System" name="categoryid">Task template category ID</param>
        /// <param type="System.Boolean, System" name="isNotify">Responsible notification: notifies or not</param>
        /// <param type="System.Int64, System" name="offsetTicks">Tick offset</param>
        /// <param type="System.Boolean, System" name="deadLineIsFixed">Defines if the deadline is fixed or not</param>
        /// <short>Create a task template</short> 
        /// <category>Task templates</category>
        /// <returns>Task template</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/{containerid}/tasktemplate</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create(@"tasktemplatecontainer/{containerid:[0-9]+}/tasktemplate")]
        public TaskTemplateWrapper CreateTaskTemplate(
            int containerid,
            string title,
            string description,
            Guid responsibleid,
            int categoryid,
            bool isNotify,
            long offsetTicks,
            bool deadLineIsFixed
            )
        {
            if (containerid <= 0 || string.IsNullOrEmpty(title) || categoryid <= 0) throw new ArgumentException();

            var container = DaoFactory.TaskTemplateContainerDao.GetByID(containerid);
            if (container == null) throw new ItemNotFoundException();

            var item = new TaskTemplate
            {
                CategoryID = categoryid,
                ContainerID = containerid,
                DeadLineIsFixed = deadLineIsFixed,
                Description = description,
                isNotify = isNotify,
                ResponsibleID = responsibleid,
                Title = title,
                Offset = TimeSpan.FromTicks(offsetTicks)
            };

            item.ID = DaoFactory.TaskTemplateDao.SaveOrUpdate(item);

            return ToTaskTemplateWrapper(item);
        }

        /// <summary>
        /// Updates the selected task template with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Task template ID</param>
        /// <param type="System.Int32, System" name="containerid">New task template container ID</param>
        /// <param type="System.String, System" name="title">New task template title</param>
        /// <param type="System.String, System" name="description">New task template description</param>
        /// <param type="System.Guid, System" name="responsibleid">New task template responsible ID</param>
        /// <param type="System.Int32, System" name="categoryid">New task template category ID</param>
        /// <param type="System.Boolean, System" name="isNotify">Responsible notification: notifies or not</param>
        /// <param type="System.Int64, System" name="offsetTicks">New tick offset</param>
        /// <param type="System.Boolean, System" name="deadLineIsFixed">Defines if the deadline is fixed or not</param>
        /// <short>Update a task template</short> 
        /// <category>Task templates</category>
        /// <returns>Task template</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/{containerid}/tasktemplate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"tasktemplatecontainer/{containerid:[0-9]+}/tasktemplate")]
        public TaskTemplateWrapper UpdateTaskTemplate(
            int id,
            int containerid,
            string title,
            string description,
            Guid responsibleid,
            int categoryid,
            bool isNotify,
            long offsetTicks,
            bool deadLineIsFixed
            )
        {
            if (containerid <= 0 || string.IsNullOrEmpty(title) || categoryid <= 0) throw new ArgumentException();

            var updatingItem = DaoFactory.TaskTemplateDao.GetByID(id);
            if (updatingItem == null) throw new ItemNotFoundException();

            var container = DaoFactory.TaskTemplateContainerDao.GetByID(containerid);
            if (container == null) throw new ItemNotFoundException();

            var item = new TaskTemplate
            {
                CategoryID = categoryid,
                ContainerID = containerid,
                DeadLineIsFixed = deadLineIsFixed,
                Description = description,
                isNotify = isNotify,
                ResponsibleID = responsibleid,
                Title = title,
                ID = id,
                Offset = TimeSpan.FromTicks(offsetTicks)
            };

            item.ID = DaoFactory.TaskTemplateDao.SaveOrUpdate(item);

            return ToTaskTemplateWrapper(item);
        }

        /// <summary>
        /// Deletes a task template with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Task template ID</param>
        /// <short>Delete a task template</short> 
        /// <category>Task templates</category>
        /// <returns>Task template</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/tasktemplate/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <visible>false</visible>
        [Delete(@"tasktemplatecontainer/tasktemplate/{id:[0-9]+}")]
        public TaskTemplateWrapper DeleteTaskTemplate(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var taskTemplate = DaoFactory.TaskTemplateDao.GetByID(id);
            if (taskTemplate == null) throw new ItemNotFoundException();

            var result = ToTaskTemplateWrapper(taskTemplate);

            DaoFactory.TaskTemplateDao.Delete(id);

            return result;
        }

        /// <summary>
        /// Returns a task template with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Task template ID</param>
        /// <short>Get a task template by ID</short> 
        /// <category>Task templates</category>
        /// <returns>Task template</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/tasktemplatecontainer/tasktemplate/{id}</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"tasktemplatecontainer/tasktemplate/{id:[0-9]+}")]
        public TaskTemplateWrapper GetTaskTemplateByID(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var taskTemplate = DaoFactory.TaskTemplateDao.GetByID(id);
            if (taskTemplate == null) throw new ItemNotFoundException();

            return ToTaskTemplateWrapper(taskTemplate);
        }

        protected TaskTemplateWrapper ToTaskTemplateWrapper(TaskTemplate taskTemplate)
        {
            return new TaskTemplateWrapper
            {
                Category = GetTaskCategoryByID(taskTemplate.CategoryID),
                ContainerID = taskTemplate.ContainerID,
                DeadLineIsFixed = taskTemplate.DeadLineIsFixed,
                Description = taskTemplate.Description,
                ID = taskTemplate.ID,
                isNotify = taskTemplate.isNotify,
                Title = taskTemplate.Title,
                OffsetTicks = taskTemplate.Offset.Ticks,
                Responsible = EmployeeWraper.Get(taskTemplate.ResponsibleID)
            };
        }

        protected IEnumerable<TaskTemplateContainerWrapper> ToTaskListTemplateContainerWrapper(IEnumerable<TaskTemplateContainer> items)
        {
            var result = new List<TaskTemplateContainerWrapper>();

            var taskTemplateDictionary = DaoFactory.TaskTemplateDao.GetAll()
                                                   .GroupBy(item => item.ContainerID)
                                                   .ToDictionary(x => x.Key, y => y.Select(ToTaskTemplateWrapper));

            foreach (var item in items)
            {
                var taskTemplateContainer = new TaskTemplateContainerWrapper
                {
                    Title = item.Title,
                    EntityType = item.EntityType.ToString(),
                    ID = item.ID
                };

                if (taskTemplateDictionary.ContainsKey(taskTemplateContainer.ID))
                {
                    taskTemplateContainer.Items = taskTemplateDictionary[taskTemplateContainer.ID];
                }

                result.Add(taskTemplateContainer);
            }

            return result;
        }

        protected TaskTemplateContainerWrapper ToTaskTemplateContainerWrapper(TaskTemplateContainer item)
        {
            return ToTaskListTemplateContainerWrapper(new List<TaskTemplateContainer>
                {
                    item
                }).FirstOrDefault();
        }
    }
}