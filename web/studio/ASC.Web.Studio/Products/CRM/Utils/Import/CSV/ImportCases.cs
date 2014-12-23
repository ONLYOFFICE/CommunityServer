/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

#endregion

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation
    {
        private void ImportCaseData()
        {
            using (var CSVFileStream = _dataStore.GetReadStream("temp", _CSVFileURI))
            using (CsvReader csv = ImportFromCSV.CreateCsvReaderInstance(CSVFileStream, _importSettings))
            {
                int currentIndex = 0;

                var casesDao = _daoFactory.GetCasesDao();
                var customFieldDao = _daoFactory.GetCustomFieldDao();
                var tagDao = _daoFactory.GetTagDao();

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
                            var findedMember = _daoFactory.GetContactDao().GetContactsByName(item);

                            if (findedMember.Count > 0)
                                localMembersCases.Add(findedMember[0].ID);
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

                var newIDs = casesDao.SaveCasesList(findedCases);
                findedCases.ForEach(d => d.ID = newIDs[d.ID]);

                findedCustomField.ForEach(item => item.EntityID = newIDs[item.EntityID]);

                customFieldDao.SaveList(findedCustomField);

                Percentage += 12.5;

                foreach (var findedCasesMemberKey in findedCasesMembers.Keys)
                {
                    _daoFactory.GetDealDao().SetMembers(newIDs[findedCasesMemberKey], findedCasesMembers[findedCasesMemberKey].ToArray());
                }

                Percentage += 12.5;

                foreach (var findedTagKey in findedTags.Keys)
                {
                    tagDao.SetTagToEntity(EntityType.Case, newIDs[findedTagKey], findedTags[findedTagKey].ToArray());
                }

                if (_importSettings.IsPrivate)
                    findedCases.ForEach(dealItem => CRMSecurity.SetAccessTo(dealItem, _importSettings.AccessList));


                Percentage += 12.5;
            }

            Complete();
        }
    }
}