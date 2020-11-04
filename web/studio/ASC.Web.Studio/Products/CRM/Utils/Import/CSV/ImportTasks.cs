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
using System.Linq;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.CRM.Core.Dao;
using LumenWorks.Framework.IO.Csv;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation
    {
        private void ImportTaskData(DaoFactory _daoFactory)
        {
            using (var CSVFileStream = _dataStore.GetReadStream("temp", _CSVFileURI))
            using (CsvReader csv = ImportFromCSV.CreateCsvReaderInstance(CSVFileStream, _importSettings))
            {
                int currentIndex = 0;

                var contactDao = _daoFactory.ContactDao;
                var listItemDao = _daoFactory.ListItemDao;
                var taskDao = _daoFactory.TaskDao;

                var findedTasks = new List<Task>();
                var taskCategories = listItemDao.GetItems(ListType.TaskCategory);

                var allUsers = ASC.Core.CoreContext.UserManager.GetUsers(EmployeeStatus.All).ToList();

                while (csv.ReadNextRecord())
                {
                    _columns = csv.GetCurrentRowFields(false);

                    var obj = new Task();

                    obj.ID = currentIndex;

                    obj.Title = GetPropertyValue("title");

                    if (String.IsNullOrEmpty(obj.Title)) continue;

                    obj.Description = GetPropertyValue("description");

                    DateTime deadline;

                    if (DateTime.TryParse(GetPropertyValue("due_date"), out deadline))
                        obj.DeadLine = deadline;
                    else
                        obj.DeadLine = TenantUtil.DateTimeNow();

                    var csvResponsibleValue = GetPropertyValue("responsible");
                    var responsible = allUsers.Where(n => n.DisplayUserName().Equals(csvResponsibleValue)).FirstOrDefault();

                    if (responsible != null)
                        obj.ResponsibleID = responsible.ID;
                    else
                        obj.ResponsibleID = Constants.LostUser.ID;

                    var categoryTitle = GetPropertyValue("taskCategory");

                    if (!String.IsNullOrEmpty(categoryTitle))
                    {
                        var findedCategory = taskCategories.Find(item => String.Compare(item.Title, categoryTitle) == 0);

                        if (findedCategory == null)
                        {
                            obj.CategoryID = taskCategories[0].ID;
                        }
                        else
                            obj.CategoryID = findedCategory.ID;
                    }
                    else
                        obj.CategoryID = taskCategories[0].ID;

                    var contactName = GetPropertyValue("contact");

                    if (!String.IsNullOrEmpty(contactName))
                    {
                        var contacts = contactDao.GetContactsByName(contactName, true);

                        if (contacts.Count > 0)
                        {
                            obj.ContactID = contacts[0].ID;
                        }
                        else
                        {
                            contacts = contactDao.GetContactsByName(contactName, false);
                            if (contacts.Count > 0)
                            {
                                obj.ContactID = contacts[0].ID;
                            }
                        }
                    }

                    obj.IsClosed = false;

                    var taskStatus = GetPropertyValue("status");

                    if (!String.IsNullOrEmpty(taskStatus))
                    {
                        if (String.Compare(taskStatus, CRMTaskResource.TaskStatus_Closed, true) == 0)
                            obj.IsClosed = true;

                    }

                    var alertValue = GetPropertyValue("alertValue");
                    int alertIntVal = 0;

                    if (Int32.TryParse(alertValue, out alertIntVal))
                        obj.AlertValue = alertIntVal;
                    else
                        obj.AlertValue = 0;


                    findedTasks.Add(obj);

                    if ((currentIndex + 1) > ImportFromCSV.MaxRoxCount) break;

                    currentIndex++;

                }

                Percentage = 50;

                taskDao.SaveTaskList(findedTasks);

                Percentage += 12.5;

                Complete();

            }

        }

    }
}