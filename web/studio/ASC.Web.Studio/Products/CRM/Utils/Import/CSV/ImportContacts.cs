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
using ASC.Web.CRM.Core.Enums;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;

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

                var countactCount = ImportFromCSV.MaxRoxCount;

                var addressTemplate = new JObject();

                foreach (String addressPartName in Enum.GetNames(typeof(AddressPart)))
                    addressTemplate.Add(addressPartName.ToLower(), "");

                var addressTemplateStr = addressTemplate.ToString();

                var personFakeIdCompanyNameHash = new Dictionary<int, String>();

                var contactDao = _daoFactory.GetContactDao();
                var contactInfoDao = _daoFactory.GetContactInfoDao();
                var customFieldDao = _daoFactory.GetCustomFieldDao();
                var tagDao = _daoFactory.GetTagDao();
                var listItemDao = _daoFactory.GetListItemDao();

                var findedContacts = new Dictionary<int, Contact>();
                var findedTags = new Dictionary<int, List<String>>();
                var findedCustomField = new List<CustomField>();
                var findedContactInfos = new List<ContactInfo>();

                while (csv.ReadNextRecord())
                {
                    _columns = csv.GetCurrentRowFields(false);

                    var firstName = GetPropertyValue("firstName");
                    var lastName = GetPropertyValue("lastName");
                    var companyName = GetPropertyValue("companyName");

                    if ((String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName)) &&
                        String.IsNullOrEmpty(companyName))
                        continue;

                    Percentage += 1.0 * 100 / (countactCount * 2);

                    Contact contact;

                    if (!(String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName)))
                    {
                        var person = new Person
                        {
                            ID = currentIndex,
                            FirstName = firstName,
                            LastName = lastName,
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


                    findedContacts.Add(contact.ID, contact);


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

                    var primaryFields = new List<int>();

                    foreach (JProperty jToken in _importSettings.ColumnMapping.Children())
                    {
                        var propertyValue = GetPropertyValue(jToken.Name);

                        if (String.IsNullOrEmpty(propertyValue)) continue;

                        if (jToken.Name.StartsWith("customField_"))
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
                        else if (jToken.Name.StartsWith("contactInfo_"))
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

                                continue;
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
                    }


                    if (currentIndex + 1 > ImportFromCSV.MaxRoxCount) break;

                    currentIndex++;

                }

                Percentage = 37.5;

                switch (_importSettings.DuplicateRecordRule)
                {
                    case 1:  // Skip  
                        {
                            var emails = findedContactInfos.Where(item => item.InfoType == ContactInfoType.Email).ToList();

                            var duplicateContactsID = contactDao.FindDuplicateByEmail(emails).Select(
                                   row => Convert.ToInt32(row[0])).Distinct().ToList();

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
                        var findedCompany = contactDao.GetContactsByName(item.Value).FirstOrDefault(x => x is Company);

                        // Why ???
                        if (findedCompany == null)
                        {
                            findedCompany = new Company
                            {
                                CompanyName = item.Value,
                                ShareType = _importSettings.ShareType
                            };
                            findedCompany.ID = contactDao.SaveContact(findedCompany);

                            person.CompanyID = findedCompany.ID;
                            CRMSecurity.SetAccessTo(findedCompany, _importSettings.ContactManagers);
                        }
                        else
                        {
                            person.CompanyID = findedCompany.ID;

                        }

                        companyNameRealIdHash.Add(item.Value, person.CompanyID);
                    }
                }

                fakeRealContactIdHash = fakeRealContactIdHash.Union(contactDao.SaveContactList(findedPeoples.Values.ToList())).ToDictionary(item => item.Key, item => item.Value);

                Percentage += 12.5;

                findedContactInfos.ForEach(item => item.ContactID = fakeRealContactIdHash[item.ContactID]);

                contactInfoDao.SaveList(findedContactInfos);

                Percentage += 12.5;

                findedCustomField.ForEach(item => item.EntityID = fakeRealContactIdHash[item.EntityID]);

                customFieldDao.SaveList(findedCustomField);

                Percentage += 12.5;

                foreach (var findedTagKey in findedTags.Keys)
                {
                    tagDao.SetTagToEntity(EntityType.Contact, fakeRealContactIdHash[findedTagKey], findedTags[findedTagKey].ToArray());
                }

                findedContacts.Values.ToList().ForEach(contact => CRMSecurity.SetAccessTo(contact, _importSettings.ContactManagers));

                Percentage += 12.5;
            }

            Complete();

        }

    }
}