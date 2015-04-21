/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Linq;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Core.Enums;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;
using ASC.CRM.Core.Dao;

#endregion

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation
    {
        private void ImportContactsData()
        {
            using (var CSVFileStream = _dataStore.GetReadStream("temp", _CSVFileURI))
            using (CsvReader csv = ImportFromCSV.CreateCsvReaderInstance(CSVFileStream, _importSettings))
            {
                int currentIndex = 0;

                var personFakeIdCompanyNameHash = new Dictionary<int, String>();

                var contactDao = _daoFactory.GetContactDao();
                var contactInfoDao = _daoFactory.GetContactInfoDao();
                var customFieldDao = _daoFactory.GetCustomFieldDao();
                var tagDao = _daoFactory.GetTagDao();

                var findedContacts = new Dictionary<int, Contact>();
                var findedTags = new Dictionary<int, List<String>>();
                var findedCustomField = new List<CustomField>();
                var findedContactInfos = new List<ContactInfo>();

                while (csv.ReadNextRecord())
                {
                    _columns = csv.GetCurrentRowFields(false);
                    Contact contact = null;

                    #region Common data

                    if (!_CommonData(currentIndex, ref contact, ref personFakeIdCompanyNameHash))
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

                Percentage = 37.5;

                #region Processing duplicate rule

                _DuplicateRecordRuleProcess(ref findedContacts, ref personFakeIdCompanyNameHash, ref findedContactInfos, ref findedCustomField, ref findedTags);

                #endregion

                Percentage += 12.5;

                var findedCompanies = findedContacts.Where(x => x.Value is Company).ToDictionary(x => x.Key, y => y.Value);
                var findedPeoples = findedContacts.Where(x => x.Value is Person).ToDictionary(x => x.Key, y => y.Value);

                var fakeRealContactIdHash = contactDao.SaveContactList(findedCompanies.Values.ToList())
                                            .ToDictionary(item => item.Key, item => item.Value);

                var companyNameRealIdHash = new Dictionary<String, int>();

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

                fakeRealContactIdHash = fakeRealContactIdHash.Union(contactDao.SaveContactList(findedPeoples.Values.ToList()))
                    .ToDictionary(item => item.Key, item => item.Value);

                Percentage += 12.5;

                #region Save contact infos

                findedContactInfos.ForEach(item => item.ContactID = fakeRealContactIdHash[item.ContactID]);
                contactInfoDao.SaveList(findedContactInfos);

                #endregion

                Percentage += 12.5;

                #region Save custom fields

                findedCustomField.ForEach(item => item.EntityID = fakeRealContactIdHash[item.EntityID]);
                customFieldDao.SaveList(findedCustomField);

                #endregion

                Percentage += 12.5;

                #region Save tags
                foreach (var findedTagKey in findedTags.Keys)
                {
                    tagDao.SetTagToEntity(EntityType.Contact, fakeRealContactIdHash[findedTagKey], findedTags[findedTagKey].ToArray());
                }
                #endregion

                #region CRMSecurity set

                findedContacts.Values.ToList().ForEach(contact => CRMSecurity.SetAccessTo(contact, _importSettings.ContactManagers));

                #endregion

                Percentage += 12.5;
            }

            Complete();
        }

        private bool _CommonData(int currentIndex, ref Contact contact, ref Dictionary<int, String> personFakeIdCompanyNameHash)
        {
            var firstName = GetPropertyValue("firstName");
            var lastName = GetPropertyValue("lastName");
            var companyName = GetPropertyValue("companyName");

            if (String.IsNullOrEmpty(firstName) && String.IsNullOrEmpty(lastName) && String.IsNullOrEmpty(companyName))
                return false;

            Percentage += 1.0 * 100 / (ImportFromCSV.MaxRoxCount * 2);

            var listItemDao = _daoFactory.GetListItemDao();


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

            var category = Convert.ToInt32(nameParts[1]);

            bool isPrimary = false;

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

            findedContactInfos.Add(new ContactInfo
            {
                Category = category,
                InfoType = contactInfoType,
                Data = propertyValue,
                ContactID = contact.ID,
                IsPrimary = isPrimary
            });
        }

        private void _DuplicateRecordRuleProcess(
            ref Dictionary<int, Contact> findedContacts,
            ref Dictionary<int, String> personFakeIdCompanyNameHash,
            ref List<ContactInfo> findedContactInfos,
            ref List<CustomField> findedCustomField,
            ref Dictionary<int, List<String>> findedTags)
        {
            var contactDao = _daoFactory.GetContactDao();

            switch (_importSettings.DuplicateRecordRule)
            {
                case 1:  // Skip  
                    {
                        var emails = findedContactInfos.Where(item => item.InfoType == ContactInfoType.Email).ToList();

                        var duplicateContactsID = contactDao.FindDuplicateByEmail(emails)
                            .Select(row => Convert.ToInt32(row[0]))
                            .Distinct()
                            .ToList();

                        if (duplicateContactsID.Count == 0) break;

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
                    break;
                case 2:  // Overwrite  
                    {
                        var emailContactInfos = findedContactInfos.Where(item => item.InfoType == ContactInfoType.Email).ToList();

                        var duplicateContactsID = contactDao.FindDuplicateByEmail(emailContactInfos).Select(
                             row => Convert.ToInt32(row[2])).Distinct().ToArray();

                        contactDao.DeleteBatchContact(duplicateContactsID);

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