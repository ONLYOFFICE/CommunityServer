/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ASC.Common.Logging;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Services.NotifyService;
using Newtonsoft.Json.Linq;
using ASC.Web.CRM.Resources;
using ASC.Web.Core;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Core.Enums;
using Autofac;

namespace ASC.Web.CRM.HttpHandlers
{
    public class WebToLeadFromHandler : IHttpHandler
    {
        private HttpContext _context;

        private String GetValue(String propertyName)
        {
            return _context.Request.Form[propertyName];
        }

        private bool CheckPermission()
        {
            try
            {
                var webFromKey = GetValue("web_form_key");

                if (String.IsNullOrEmpty(webFromKey))
                    return false;

                var webFromKeyAsGuid = new Guid(webFromKey);

                return Global.TenantSettings.WebFormKey == webFromKeyAsGuid;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    _context = context;

                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                    if (!CheckPermission())
                    {
                        throw new Exception(CRMSettingResource.WebToLeadsForm_InvalidKeyException);
                    }

                    var productInfo = WebItemSecurity.GetSecurityInfo(ProductEntryPoint.ID.ToString());
                    if (!productInfo.Enabled)
                    {
                        throw new Exception(CRMCommonResource.CRMProductIsDisabled);
                    }

                    Contact contact;

                    var fieldCollector = new NameValueCollection();

                    var addressTemplate = new JObject();
                    foreach (String addressPartName in Enum.GetNames(typeof(AddressPart)))
                        addressTemplate.Add(addressPartName.ToLower(), "");
                    var addressTemplateStr = addressTemplate.ToString();

                    var isCompany = false;

                    var isCompanyString = GetValue("is_company");
                    var firstName = GetValue("firstName");
                    var lastName = GetValue("lastName");
                    var companyName = GetValue("companyName");

                    if (!String.IsNullOrEmpty(isCompanyString))
                    {
                        if (!Boolean.TryParse(isCompanyString, out isCompany))
                        {
                            throw new ArgumentException();
                        }
                    }
                    else //old scheme
                    {
                        if (!String.IsNullOrEmpty(firstName))
                        {
                            isCompany = false;
                        }
                        else if (!String.IsNullOrEmpty(companyName))
                        {
                            isCompany = true;
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }


                    if (isCompany)
                    {
                        contact = new Company();

                        ((Company) contact).CompanyName = companyName;

                        fieldCollector.Add(CRMContactResource.CompanyName, companyName);
                    }
                    else
                    {
                        contact = new Person();

                        ((Person) contact).FirstName = firstName;
                        ((Person) contact).LastName = lastName;
                        ((Person) contact).JobTitle = GetValue("jobTitle");

                        fieldCollector.Add(CRMContactResource.FirstName, firstName);
                        fieldCollector.Add(CRMContactResource.LastName, lastName);

                        if (!String.IsNullOrEmpty(GetValue("jobTitle")))
                            fieldCollector.Add(CRMContactResource.JobTitle, ((Person) contact).JobTitle);
                    }

                    contact.About = GetValue("about");

                    if (!String.IsNullOrEmpty(contact.About))
                        fieldCollector.Add(CRMContactResource.About, contact.About);

                    if (!String.IsNullOrEmpty(GetValue("is_shared")))
                    {
                        contact.ShareType = Convert.ToBoolean(GetValue("is_shared"))
                            ? ShareType.ReadWrite
                            : ShareType.None;
                    }
                    else
                    {
                        contact.ShareType = (ShareType) (Convert.ToInt32(GetValue("share_type")));
                    }

                    contact.ID = daoFactory.ContactDao.SaveContact(contact);

                    var messageAction = contact is Company
                        ? MessageAction.CompanyCreatedWithWebForm
                        : MessageAction.PersonCreatedWithWebForm;
                    MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, messageAction,
                        MessageTarget.Create(contact.ID), contact.GetTitle());

                    var contactInfos = new List<ContactInfo>();

                    foreach (var key in _context.Request.Form.AllKeys)
                    {
                        if (key.StartsWith("customField_"))
                        {
                            var fieldID = Convert.ToInt32(key.Split(new[] {'_'})[1]);
                            String fieldValue = GetValue(key);

                            if (String.IsNullOrEmpty(fieldValue)) continue;

                            var customField = daoFactory.CustomFieldDao.GetFieldDescription(fieldID);

                            if (customField == null ||
                                !(customField.EntityType == EntityType.Contact ||
                                  customField.EntityType == EntityType.Company && isCompany ||
                                  customField.EntityType == EntityType.Person && !isCompany)) continue;

                            if (customField.FieldType == CustomFieldType.CheckBox)
                            {
                                fieldValue = fieldValue == "on" || fieldValue == "true" ? "true" : "false";
                            }
                            fieldCollector.Add(customField.Label, fieldValue);

                            daoFactory.CustomFieldDao.SetFieldValue(isCompany ? EntityType.Company : EntityType.Person, contact.ID, fieldID, fieldValue);
                        }
                        else if (key.StartsWith("contactInfo_"))
                        {
                            var nameParts = key.Split(new[] {'_'}).Skip(1).ToList();
                            var contactInfoType = (ContactInfoType) Enum.Parse(typeof(ContactInfoType), nameParts[0]);
                            var category = Convert.ToInt32(nameParts[1]);

                            bool categoryIsExists = Enum.GetValues(ContactInfo.GetCategory(contactInfoType))
                                .Cast<object>()
                                .Any(categoryEnum => (int) categoryEnum == category);
                            if (!categoryIsExists)
                                throw new ArgumentException(String.Format("Category for {0} not found", nameParts[0]));

                            if (contactInfoType == ContactInfoType.Address)
                            {
                                var addressPart = (AddressPart) Enum.Parse(typeof(AddressPart), nameParts[2]);

                                var findedAddress =
                                    contactInfos.Find(
                                        item =>
                                            (category == item.Category) && (item.InfoType == ContactInfoType.Address));

                                if (findedAddress == null)
                                {
                                    findedAddress = new ContactInfo
                                    {
                                        Category = category,
                                        InfoType = contactInfoType,
                                        Data = addressTemplateStr,
                                        ContactID = contact.ID
                                    };

                                    contactInfos.Add(findedAddress);
                                }

                                var addressParts = JObject.Parse(findedAddress.Data);

                                addressParts[addressPart.ToString().ToLower()] = GetValue(key);

                                findedAddress.Data = addressParts.ToString();

                                continue;
                            }

                            var fieldValue = GetValue(key);

                            if (String.IsNullOrEmpty(fieldValue)) continue;

                            contactInfos.Add(new ContactInfo
                            {
                                Category = category,
                                InfoType = contactInfoType,
                                Data = fieldValue,
                                ContactID = contact.ID,
                                IsPrimary = true
                            });
                        }
                        else if (String.Compare(key, "tag", true) == 0)
                        {
                            var tags = _context.Request.Form.GetValues("tag");

                            daoFactory.TagDao.SetTagToEntity(EntityType.Contact, contact.ID, tags);
                        }
                    }

                    contactInfos.ForEach(
                        item =>
                            fieldCollector[item.InfoType.ToLocalizedString()] =
                                PrepareteDataToView(item.InfoType, item.Data));

                    daoFactory.ContactInfoDao.SaveList(contactInfos, contact);

                    var notifyList = GetValue("notify_list");

                    if (!String.IsNullOrEmpty(notifyList))
                        NotifyClient.Instance.SendAboutCreateNewContact(
                            notifyList
                                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(item => new Guid(item)).ToList(), contact.ID, contact.GetTitle(), fieldCollector);

                    var managersList = GetValue("managers_list");
                    SetPermission(contact, managersList);

                    if (contact is Person && !String.IsNullOrEmpty(companyName))
                        AssignPersonToCompany((Person)contact, companyName, managersList, daoFactory);

                    if (contact is Company && !String.IsNullOrEmpty(firstName) && !String.IsNullOrEmpty(lastName))
                        AssignCompanyToPerson((Company)contact, firstName, lastName, managersList, daoFactory);

                    SecurityContext.Logout();

                    var newURL = new UriBuilder(GetValue("return_url")).Uri.AbsoluteUri;
                    context.Response.Buffer = true;
                    context.Response.Status = "302 Object moved";
                    context.Response.AddHeader("Location", newURL);
                    context.Response.Write("<HTML><Head>");
                    context.Response.Write(String.Format("<META HTTP-EQUIV=Refresh CONTENT=\"0;URL={0}\">", newURL));
                    context.Response.Write(String.Format("<Script>window.location='{0}';</Script>", newURL));
                    context.Response.Write("</Head>");
                    context.Response.Write("</HTML>");
                }
            }
            catch(Exception error)
            {
                LogManager.GetLogger("ASC.CRM").Error(error);
                context.Response.StatusCode = 400;
                context.Response.Write(HttpUtility.HtmlEncode(error.Message));
            }
        }

        private String PrepareteDataToView(ContactInfoType contactInfoType, String data)
        {
            if (contactInfoType != ContactInfoType.Address) return data;

            var addressParts = JObject.Parse(data);

            var address = new StringBuilder();

            foreach (AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart)))
                address.Append(addressParts[addressPartEnum.ToString().ToLower()] + " ");

            return address.ToString();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        protected void SetPermission(Contact contact, String privateList)
        {
            if (String.IsNullOrEmpty(privateList)) return;

            var selectedUsers = privateList
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => new Guid(item)).ToList();

            CRMSecurity.SetAccessTo(contact, selectedUsers);
        }

        protected void AssignCompanyToPerson(Company company, String firstName, String lastName, String privateList, DaoFactory daoFactory)
        {
            var person = new Person
                {
                    FirstName = firstName,
                    LastName = lastName,
                    CompanyID = company.ID
                };
            person.ID = daoFactory.ContactDao.SaveContact(person);
            SetPermission(person, privateList);
        }


        protected void AssignPersonToCompany(Person person, String companyName, String privateList, DaoFactory daoFactory)
        {
            Company company;

            var findedCompanies = daoFactory.ContactDao.GetContactsByName(companyName, true).ToList();

            if (findedCompanies.Count == 0)
            {
                company = new Company
                    {
                        CompanyName = companyName
                    };

                company.ID = daoFactory.ContactDao.SaveContact(company);

                SetPermission(company, privateList);
            }
            else
            {
                company = (Company)findedCompanies[0];
            }

            daoFactory.ContactDao.AddMember(person.ID, company.ID);
        }
    }
}