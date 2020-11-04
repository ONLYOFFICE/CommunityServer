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
using ASC.Web.CRM.Core.Enums;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;
using ASC.CRM.Core.Dao;
using ASC.Common.Threading.Progress;

#endregion

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation
    {
        private Int32 DaoIterationStep = 200;

        private void ImportContactsData(DaoFactory _daoFactory)
        {
            var index = 0;

            var personFakeIdCompanyNameHash = new Dictionary<int, String>();

            var contactDao = _daoFactory.ContactDao;
            var contactInfoDao = _daoFactory.ContactInfoDao;
            var customFieldDao = _daoFactory.CustomFieldDao;
            var tagDao = _daoFactory.TagDao;

            var findedContacts = new Dictionary<int, Contact>();
            var findedTags = new Dictionary<int, List<String>>();
            var findedCustomField = new List<CustomField>();
            var findedContactInfos = new List<ContactInfo>();

            #region Read csv
            using (var CSVFileStream = _dataStore.GetReadStream("temp", _CSVFileURI))
            using (CsvReader csv = ImportFromCSV.CreateCsvReaderInstance(CSVFileStream, _importSettings))
            {
                int currentIndex = 0;

                while (csv.ReadNextRecord())
                {
                    _columns = csv.GetCurrentRowFields(false);
                    Contact contact = null;

                    #region Common data

                    if (!_CommonData(currentIndex, _daoFactory, ref contact, ref personFakeIdCompanyNameHash))
                        continue;

                    findedContacts.Add(contact.ID, contact);

                    #endregion

                    #region Read tags

                    _ReadTags(ref findedTags, contact);

                    #endregion

                    #region Custom fields Y contact infos

                    var primaryFields = new List<int>();

                    foreach (JProperty jToken in _importSettings.ColumnMapping.Children())
                    {
                        var propertyValue = GetPropertyValue(jToken.Name);
                        if (String.IsNullOrEmpty(propertyValue)) continue;

                        if (jToken.Name.StartsWith("customField_"))
                        {
                            _ReadCustomField(jToken, propertyValue, contact, ref findedCustomField, customFieldDao);
                        }
                        else if (jToken.Name.StartsWith("contactInfo_"))
                        {
                            var addressTemplate = new JObject();
                            foreach (String addressPartName in Enum.GetNames(typeof(AddressPart)))
                                addressTemplate.Add(addressPartName.ToLower(), "");

                            var addressTemplateStr = addressTemplate.ToString();

                            _ReadContactInfo(jToken, propertyValue, contact, ref findedContactInfos, ref primaryFields, addressTemplateStr);
                        }
                    }
                    #endregion

                    if (currentIndex + 1 > ImportFromCSV.MaxRoxCount) break;
                    currentIndex++;
                }
            }
            _log.InfoFormat("ImportContactsData. Reading {0} findedContacts complete", findedContacts.Count);

            #endregion

            Percentage = 37.5;

            #region Check Cancel flag | Insert Operation InCache
            if (ImportDataCache.CheckCancelFlag(EntityType.Contact))
            {
                ImportDataCache.ResetAll(EntityType.Contact);

                throw new OperationCanceledException();
            }

            ImportDataCache.Insert(EntityType.Contact, (ImportDataOperation)Clone()); 
            #endregion

            #region Processing duplicate rule

            _DuplicateRecordRuleProcess(_daoFactory, ref findedContacts, ref personFakeIdCompanyNameHash, ref findedContactInfos, ref findedCustomField, ref findedTags);

            _log.Info("ImportContactsData. _DuplicateRecordRuleProcess. End");

            if (IsCompleted) {
                return;
            }

            #endregion

            Percentage += 12.5;

            #region Check Cancel flag | Insert Operation InCache
            if (ImportDataCache.CheckCancelFlag(EntityType.Contact))
            {
                ImportDataCache.ResetAll(EntityType.Contact);

                throw new OperationCanceledException();
            }

            ImportDataCache.Insert(EntityType.Contact,(ImportDataOperation)Clone());
            #endregion

            #region Manipulation for saving Companies for persons + CRMSecurity

            var findedCompanies = findedContacts.Where(x => x.Value is Company).ToDictionary(x => x.Key, y => y.Value);
            var findedPeoples = findedContacts.Where(x => x.Value is Person).ToDictionary(x => x.Key, y => y.Value);

            var fakeRealContactIdHash = new Dictionary<int, int>();
            var companyNameRealIdHash = new Dictionary<String, int>();


            var findedCompaniesList = findedCompanies.Values.ToList();
            if (findedCompaniesList.Count != 0)
            {
                index = 0;
                while (index < findedCompaniesList.Count)
                {
                    var portion = findedCompaniesList.Skip(index).Take(DaoIterationStep).ToList();// Get next step

                    fakeRealContactIdHash = fakeRealContactIdHash.Union(
                        contactDao.SaveContactList(portion))
                        .ToDictionary(item => item.Key, item => item.Value);


            #region CRMSecurity set -by every item-

                    portion.ForEach(ct => CRMSecurity.SetAccessTo(ct, _importSettings.ContactManagers));

            #endregion


                    index += DaoIterationStep;
                    if (index > findedCompaniesList.Count)
                    {
                        index = findedCompaniesList.Count;
                    }
                }
            }


            foreach (Company item in findedCompanies.Values)
            {
                if (companyNameRealIdHash.ContainsKey(item.CompanyName)) continue;

                companyNameRealIdHash.Add(item.CompanyName, item.ID);
            }

            foreach (var item in personFakeIdCompanyNameHash)
            {
                var person = (Person)findedPeoples[item.Key];

                if (companyNameRealIdHash.ContainsKey(item.Value))
                {
                    person.CompanyID = companyNameRealIdHash[item.Value];
                }
                else
                {
                    var findedCompany = contactDao.GetContactsByName(item.Value, true).FirstOrDefault();

                    // Why ???
                    if (findedCompany == null)
                    {
                        #region create COMPANY for person in csv
                            
                        findedCompany = new Company
                        {
                            CompanyName = item.Value,
                            ShareType = _importSettings.ShareType
                        };
                        findedCompany.ID = contactDao.SaveContact(findedCompany);

                        person.CompanyID = findedCompany.ID;
                        CRMSecurity.SetAccessTo(findedCompany, _importSettings.ContactManagers);

                        if (_importSettings.Tags.Count != 0)
                        {
                            tagDao.SetTagToEntity(EntityType.Contact, person.CompanyID, _importSettings.Tags.ToArray());
                        }

                        #endregion
                    }
                    else
                    {
                        person.CompanyID = findedCompany.ID;
                    }

                    companyNameRealIdHash.Add(item.Value, person.CompanyID);
                }
            }
            #endregion

            #region Saving People common data -by portions- + CRMSecurity

            var findedPeopleList = findedPeoples.Values.ToList();
            if (findedPeopleList.Count != 0)
            {
                index = 0;
                while (index < findedPeopleList.Count)
                {
                    var portion = findedPeopleList.Skip(index).Take(DaoIterationStep).ToList();// Get next step

                    fakeRealContactIdHash = fakeRealContactIdHash.Union(
                        contactDao.SaveContactList(portion))
                        .ToDictionary(item => item.Key, item => item.Value);

                #region CRMSecurity set -by every item-

                    portion.ForEach(ct => CRMSecurity.SetAccessTo(ct, _importSettings.ContactManagers));

                #endregion


                    index += DaoIterationStep;
                    if (index > findedPeopleList.Count)
                    {
                        index = findedPeopleList.Count;
                    }
                }
            }
            _log.Info("ImportContactsData. Contacts common data saved");
            #endregion

            Percentage += 12.5;

            #region Check Cancel flag | Insert Operation InCache
            if (ImportDataCache.CheckCancelFlag(EntityType.Contact))
            {
                ImportDataCache.ResetAll(EntityType.Contact);

                throw new OperationCanceledException();
            }

            ImportDataCache.Insert(EntityType.Contact, (ImportDataOperation)Clone());  
            #endregion

            #region Save contact infos -by portions-

            if (findedContactInfos.Count != 0)
            {
                findedContactInfos.ForEach(item => item.ContactID = fakeRealContactIdHash[item.ContactID]);

                index = 0;
                while (index < findedContactInfos.Count)
                {
                    var portion = findedContactInfos.Skip(index).Take(DaoIterationStep).ToList();// Get next step

                    contactInfoDao.SaveList(portion);

                    index += DaoIterationStep;
                    if (index > findedContactInfos.Count)
                    {
                        index = findedContactInfos.Count;
                    }
                }
            }
            _log.Info("ImportContactsData. Contacts infos saved");
            #endregion

            Percentage += 12.5;

            #region Check Cancel flag | Insert Operation InCache
            if (ImportDataCache.CheckCancelFlag(EntityType.Contact))
            {
                ImportDataCache.ResetAll(EntityType.Contact);

                throw new OperationCanceledException();
            }

            ImportDataCache.Insert(EntityType.Contact, (ImportDataOperation)Clone());
            #endregion

            #region Save custom fields -by portions-

            if (findedCustomField.Count != 0)
            {
                findedCustomField.ForEach(item => item.EntityID = fakeRealContactIdHash[item.EntityID]);
                
                index = 0;
                while (index < findedCustomField.Count)
                {
                    var portion = findedCustomField.Skip(index).Take(DaoIterationStep).ToList();// Get next step

                    customFieldDao.SaveList(portion);

                    index += DaoIterationStep;
                    if (index > findedCustomField.Count)
                    {
                        index = findedCustomField.Count;
                    }
                }
            }
            _log.Info("ImportContactsData. Custom fields saved");
            #endregion

            Percentage += 12.5;

            #region Check Cancel flag | Insert Operation InCache
            if (ImportDataCache.CheckCancelFlag(EntityType.Contact))
            {
                ImportDataCache.ResetAll(EntityType.Contact);

                throw new OperationCanceledException();
            }

            ImportDataCache.Insert(EntityType.Contact, (ImportDataOperation)Clone()); 
            #endregion

            #region Save tags

            var findedTagsValues = new List<string>();

            findedTags.Values.ToList().ForEach(t => { findedTagsValues.AddRange(t); });
            findedTagsValues = findedTagsValues.Distinct().ToList();

            var allTagsForImport = tagDao.GetAndAddTags(EntityType.Contact, findedTagsValues.Distinct().ToArray());

            foreach (var findedTagKey in findedTags.Keys)
            {
                var curTagNames = findedTags[findedTagKey];
                var curTagIds = curTagNames.ConvertAll(n => allTagsForImport.ContainsKey(n) ? allTagsForImport[n] : 0).Where(id => id != 0).ToArray();

                tagDao.AddTagToEntity(EntityType.Contact, fakeRealContactIdHash[findedTagKey], curTagIds);
            }
            _log.Info("ImportContactsData. Tags saved");
            #endregion

            Percentage += 12.5;

            #region Check Cancel flag | Insert Operation InCache
            if (ImportDataCache.CheckCancelFlag(EntityType.Contact))
            {
                ImportDataCache.ResetAll(EntityType.Contact);

                throw new OperationCanceledException();
            }

            ImportDataCache.Insert(EntityType.Contact, (ImportDataOperation)Clone());
            #endregion

            Complete();
        }

        private bool _CommonData(int currentIndex, DaoFactory _daoFactory, ref Contact contact, ref Dictionary<int, String> personFakeIdCompanyNameHash)
        {
            var firstName = GetPropertyValue("firstName");
            var lastName = GetPropertyValue("lastName");
            var companyName = GetPropertyValue("companyName");

            if (String.IsNullOrEmpty(firstName) && String.IsNullOrEmpty(lastName) && String.IsNullOrEmpty(companyName))
                return false;

            Percentage += 1.0 * 100 / (ImportFromCSV.MaxRoxCount * 3);

            var listItemDao = _daoFactory.ListItemDao;


            if (!String.IsNullOrEmpty(firstName) || !String.IsNullOrEmpty(lastName))
            {
                var person = new Person
                {
                    ID = currentIndex,
                    FirstName = !String.IsNullOrEmpty(firstName) ? firstName : lastName,
                    LastName = !String.IsNullOrEmpty(firstName) ? lastName : String.Empty,
                    JobTitle = GetPropertyValue("title")
                };

                if (!(String.IsNullOrEmpty(companyName)))
                    personFakeIdCompanyNameHash.Add(person.ID, companyName);

                contact = person;
            }
            else
            {
                contact = new Company
                {
                    ID = currentIndex
                };

                ((Company)contact).CompanyName = companyName;
            }

            contact.About = GetPropertyValue("notes");
            contact.ShareType = _importSettings.ShareType;

            var contactStageName = GetPropertyValue("contactStage");
            var contactTypeName = GetPropertyValue("contactType");
            if (!String.IsNullOrEmpty(contactStageName))
            {
                var contactStage = listItemDao.GetByTitle(ListType.ContactStatus, contactStageName);
                if (contactStage != null)
                {
                    contact.StatusID = contactStage.ID;
                }
                else
                {
                    contact.StatusID = listItemDao.CreateItem(ListType.ContactStatus, new ListItem(){
                        Title = contactStageName,
                        Color = "#77cf9a",
                        Description = ""
                    });
                }
            }

            if (!String.IsNullOrEmpty(contactTypeName))
            {
                var contactType = listItemDao.GetByTitle(ListType.ContactType, contactTypeName);
                if (contactType != null)
                {
                    contact.ContactTypeID = contactType.ID;
                }
                else
                {
                    contact.ContactTypeID = listItemDao.CreateItem(ListType.ContactType, new ListItem(){
                        Title = contactTypeName,
                        Description = ""
                    });
                }
            }

            return true;
        }

        #region Read methods

        private void _ReadTags(ref Dictionary<int, List<String>> findedTags, Contact contact)
        {
            var tag = GetPropertyValue("tag");

            if (!String.IsNullOrEmpty(tag))
            {
                var tagList = tag.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                tagList.AddRange(_importSettings.Tags);
                tagList = tagList.Distinct().ToList();
                findedTags.Add(contact.ID, tagList);
            }
            else if (_importSettings.Tags.Count != 0)
            {
                findedTags.Add(contact.ID, _importSettings.Tags);
            }
        }

        private void _ReadCustomField(JProperty jToken, String propertyValue, Contact contact, ref List<CustomField> findedCustomField, CustomFieldDao customFieldDao)
        {
            var fieldID = Convert.ToInt32(jToken.Name.Split(new[] { '_' })[1]);
            var field = customFieldDao.GetFieldDescription(fieldID);

            if (field != null)
            {
                findedCustomField.Add(new CustomField
                {
                    EntityID = contact.ID,
                    EntityType = contact is Person ? EntityType.Person : EntityType.Company,
                    ID = fieldID,
                    Value = field.FieldType == CustomFieldType.CheckBox ? (propertyValue == "on" || propertyValue == "true" ? "true" : "false") : propertyValue
                });
            }
        }

        private void _ReadContactInfo(JProperty jToken, String propertyValue, Contact contact, ref List<ContactInfo> findedContactInfos, ref List<int> primaryFields, String addressTemplateStr)
        {
            var nameParts = jToken.Name.Split(new[] { '_' }).Skip(1).ToList();
            var contactInfoType =
                (ContactInfoType)Enum.Parse(typeof(ContactInfoType), nameParts[0]);

            if (contactInfoType == ContactInfoType.Email)
            {
                var validEmails = propertyValue
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Where(email => email.TestEmailRegex()).ToArray();

                if(!validEmails.Any())
                    return;

                propertyValue = string.Join(",", validEmails);
            }

            var category = Convert.ToInt32(nameParts[1]);

            var isPrimary = false;

            if ((contactInfoType == ContactInfoType.Email ||
                contactInfoType == ContactInfoType.Phone ||
                contactInfoType == ContactInfoType.Address) &&
                (!primaryFields.Contains((int)contactInfoType)))
            {
                isPrimary = true;

                primaryFields.Add((int)contactInfoType);
            }

            if (contactInfoType == ContactInfoType.Address)
            {
                var addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), nameParts[2]);

                var findedAddress =
                    findedContactInfos.Find(
                        item =>
                        (category == item.Category) && (item.InfoType == ContactInfoType.Address) &&
                        (item.ContactID == contact.ID));

                if (findedAddress == null)
                {
                    findedAddress = new ContactInfo
                    {
                        Category = category,
                        InfoType = contactInfoType,
                        Data = addressTemplateStr,
                        ContactID = contact.ID,
                        IsPrimary = isPrimary
                    };

                    findedContactInfos.Add(findedAddress);
                }

                var addressParts = JObject.Parse(findedAddress.Data);

                addressParts[addressPart.ToString().ToLower()] = propertyValue;

                findedAddress.Data = addressParts.ToString();

                return;
            }

            var items = propertyValue.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                findedContactInfos.Add(new ContactInfo
                    {
                        Category = category,
                        InfoType = contactInfoType,
                        Data = item,
                        ContactID = contact.ID,
                        IsPrimary = isPrimary
                    });

                isPrimary = false;
            }
        }
        
        #endregion

        private void _DuplicateRecordRuleProcess(DaoFactory _daoFactory,
            ref Dictionary<int, Contact> findedContacts,
            ref Dictionary<int, String> personFakeIdCompanyNameHash,
            ref List<ContactInfo> findedContactInfos,
            ref List<CustomField> findedCustomField,
            ref Dictionary<int, List<String>> findedTags)
        {
            var contactDao = _daoFactory.ContactDao;

            _log.Info("_DuplicateRecordRuleProcess. Start");

            switch (_importSettings.DuplicateRecordRule)
            {
                case 1:  // Skip  
                    {
                        var emails = findedContactInfos.Where(item => item.InfoType == ContactInfoType.Email).ToList();

                        if (emails.Count == 0) break;

                        var index = 0;
                        while (index < emails.Count)
                        {
                            var emailsIteration = emails.Skip(index).Take(DaoIterationStep).ToList();// Get next step

                            var duplicateContactsID = contactDao.FindDuplicateByEmail(emailsIteration, false)
                                                        .Distinct()
                                                        .ToList();

                            if (duplicateContactsID.Count != 0)
                            {
                                findedContacts = findedContacts.Where(item => !duplicateContactsID.Contains(item.Key)).ToDictionary(x => x.Key, y => y.Value);

                                personFakeIdCompanyNameHash = personFakeIdCompanyNameHash.Where(item => !duplicateContactsID.Contains(item.Key)).ToDictionary(x => x.Key, y => y.Value);

                                if (findedContacts.Count == 0)
                                {
                                    Complete();
                                    return;
                                }

                                findedContactInfos = findedContactInfos.Where(item => !duplicateContactsID.Contains(item.ContactID)).ToList();
                                findedCustomField = findedCustomField.Where(item => !duplicateContactsID.Contains(item.EntityID)).ToList();

                                foreach (var exceptID in duplicateContactsID)
                                {
                                    if (findedTags.ContainsKey(exceptID)) findedTags.Remove(exceptID);
                                }
                            }

                            index += DaoIterationStep;
                            if (index > emails.Count)
                            {
                                index = emails.Count;
                            }
                        }
                    }
                    break;
                case 2:  // Overwrite  
                    {
                        var emailContactInfos = findedContactInfos.Where(item => item.InfoType == ContactInfoType.Email).ToList();
                        if (emailContactInfos.Count == 0) break;

                        _log.InfoFormat("_DuplicateRecordRuleProcess. Overwrite. Start. All emeails count = {0}", emailContactInfos.Count);

                        var index = 0;
                        while (index < emailContactInfos.Count)
                        {
                            var emailsIteration = emailContactInfos.Skip(index).Take(DaoIterationStep).ToList();// Get next step

                            _log.InfoFormat("_DuplicateRecordRuleProcess. Overwrite. Portion from index = {0}. count = {1}", index, emailsIteration.Count);
                            var duplicateContactsID = contactDao.FindDuplicateByEmail(emailsIteration, true)
                                                        .Distinct()
                                                        .ToArray();

                            _log.InfoFormat("_DuplicateRecordRuleProcess. Overwrite. FindDuplicateByEmail result count = {0}", duplicateContactsID.Length);
                            var deleted = contactDao.DeleteBatchContact(duplicateContactsID);

                            _log.InfoFormat("_DuplicateRecordRuleProcess. Overwrite. DeleteBatchContact. Was deleted {0} contacts", deleted != null ? deleted.Count : 0);

                            index += DaoIterationStep;
                            if (index > emailContactInfos.Count)
                            {
                                index = emailContactInfos.Count;
                            }
                        }

                        break;
                    }
                case 3: // Clone
                    break;
                default:
                    break;
            }
        }
    
    }
}