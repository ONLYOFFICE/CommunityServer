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
using ASC.Web.CRM.Resources;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;
using ASC.Common.Threading.Progress;
using ASC.CRM.Core.Dao;

#endregion

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation
    {
        private void ImportCaseData(DaoFactory _daoFactory)
        {
            using (var CSVFileStream = _dataStore.GetReadStream("temp", _CSVFileURI))
            using (CsvReader csv = ImportFromCSV.CreateCsvReaderInstance(CSVFileStream, _importSettings))
            {
                int currentIndex = 0;

                var casesDao = _daoFactory.CasesDao;
                var customFieldDao = _daoFactory.CustomFieldDao;
                var tagDao = _daoFactory.TagDao;

                var findedTags = new Dictionary<int, List<String>>();
                var findedCustomField = new List<CustomField>();
                var findedCases = new List<ASC.CRM.Core.Entities.Cases>();
                var findedCasesMembers = new Dictionary<int, List<int>>();

                while (csv.ReadNextRecord())
                {
                    _columns = csv.GetCurrentRowFields(false);

                    var objCases = new ASC.CRM.Core.Entities.Cases();

                    objCases.ID = currentIndex;

                    objCases.Title = GetPropertyValue("title");

                    if (String.IsNullOrEmpty(objCases.Title)) continue;

                    foreach (JProperty jToken in _importSettings.ColumnMapping.Children())
                    {
                        var propertyValue = GetPropertyValue(jToken.Name);

                        if (String.IsNullOrEmpty(propertyValue)) continue;

                        if (!jToken.Name.StartsWith("customField_")) continue;

                        var fieldID = Convert.ToInt32(jToken.Name.Split(new[] { '_' })[1]);

                        var field = customFieldDao.GetFieldDescription(fieldID);

                        if (field != null)
                        {
                            findedCustomField.Add(new CustomField
                            {
                                EntityID = objCases.ID,
                                EntityType = EntityType.Case,
                                ID = fieldID,
                                Value = field.FieldType == CustomFieldType.CheckBox ? (propertyValue == "on" || propertyValue == "true" ? "true" : "false") : propertyValue
                            });
                        }

                    }

                    var tag = GetPropertyValue("tag");

                    if (!String.IsNullOrEmpty(tag))
                    {
                        var tagList = tag.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        tagList.AddRange(_importSettings.Tags);
                        tagList = tagList.Distinct().ToList();
                        findedTags.Add(objCases.ID, tagList);
                    }
                    else if (_importSettings.Tags.Count != 0)
                    {
                        findedTags.Add(objCases.ID, _importSettings.Tags);
                    }

                    var localMembersCases = new List<int>();

                    var members = GetPropertyValue("member");

                    if (!String.IsNullOrEmpty(members))
                    {
                        var membersList = members.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var item in membersList)
                        {
                            var findedMember = _daoFactory.ContactDao.GetContactsByName(item, true);

                            if (findedMember.Count > 0)
                            {
                                localMembersCases.Add(findedMember[0].ID);
                            }
                            else
                            {
                                findedMember = _daoFactory.ContactDao.GetContactsByName(item, false);
                                if (findedMember.Count > 0)
                                {
                                    localMembersCases.Add(findedMember[0].ID);
                                }
                            }

                        }
                    }

                    if (localMembersCases.Count > 0)
                        findedCasesMembers.Add(objCases.ID, localMembersCases);

                    objCases.ID = currentIndex;

                    findedCases.Add(objCases);

                    if (currentIndex + 1 > ImportFromCSV.MaxRoxCount) break;

                    currentIndex++;
                }

                Percentage = 62.5;

                if (ImportDataCache.CheckCancelFlag(EntityType.Case))
                {
                    ImportDataCache.ResetAll(EntityType.Case);

                    throw new OperationCanceledException();
                }

                ImportDataCache.Insert(EntityType.Case, (ImportDataOperation)Clone());
                              
                var newIDs = casesDao.SaveCasesList(findedCases);
                findedCases.ForEach(d => d.ID = newIDs[d.ID]);

                findedCustomField.ForEach(item => item.EntityID = newIDs[item.EntityID]);

                customFieldDao.SaveList(findedCustomField);

                Percentage += 12.5;

                if (ImportDataCache.CheckCancelFlag(EntityType.Case))
                {
                    ImportDataCache.ResetAll(EntityType.Case);

                    throw new OperationCanceledException();
                }

                ImportDataCache.Insert(EntityType.Case, (ImportDataOperation)Clone());               

                foreach (var findedCasesMemberKey in findedCasesMembers.Keys)
                {
                    _daoFactory.DealDao.SetMembers(newIDs[findedCasesMemberKey], findedCasesMembers[findedCasesMemberKey].ToArray());
                }

                Percentage += 12.5;

                if (ImportDataCache.CheckCancelFlag(EntityType.Case))
                {
                    ImportDataCache.ResetAll(EntityType.Case);

                    throw new OperationCanceledException();
                }

                ImportDataCache.Insert(EntityType.Case, (ImportDataOperation)Clone());               


                foreach (var findedTagKey in findedTags.Keys)
                {
                    tagDao.SetTagToEntity(EntityType.Case, newIDs[findedTagKey], findedTags[findedTagKey].ToArray());
                }

                if (_importSettings.IsPrivate)
                    findedCases.ForEach(dealItem => CRMSecurity.SetAccessTo(dealItem, _importSettings.AccessList));


                Percentage += 12.5;

                if (ImportDataCache.CheckCancelFlag(EntityType.Case))
                {
                    ImportDataCache.ResetAll(EntityType.Case);

                    throw new OperationCanceledException();
                }

                ImportDataCache.Insert(EntityType.Case,(ImportDataOperation)Clone());               

            }

            Complete();
        }
    }
}