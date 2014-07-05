/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Core.Tenants;
using ASC.Core.Users;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation
    {
        private void ImportTaskData()
        {
            using (var CSVFileStream = _dataStore.GetReadStream("temp", _CSVFileURI))
            using (CsvReader csv = ImportFromCSV.CreateCsvReaderInstance(CSVFileStream, _importSettings))
            {
                int currentIndex = 0;

                var contactDao = _daoFactory.GetContactDao();
                var listItemDao = _daoFactory.GetListItemDao();
                var taskDao = _daoFactory.GetTaskDao();

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
                        var contacts = contactDao.GetContactsByName(contactName);

                        if (contacts.Count > 0)
                            obj.ContactID = contacts[0].ID;
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

                var newIDs = taskDao.SaveTaskList(findedTasks);

                Percentage += 12.5;

                Complete();

            }

        }

    }
}