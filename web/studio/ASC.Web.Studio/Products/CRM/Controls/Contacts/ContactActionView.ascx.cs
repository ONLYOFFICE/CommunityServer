/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Api;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;


namespace ASC.Web.CRM.Controls.Contacts
{
    public partial class ContactActionView : BaseUserControl
    {
        #region Properies

        public static string Location { get { return PathProvider.GetFileStaticRelativePath("Contacts/ContactActionView.ascx"); } }
        public Contact TargetContact { get; set; }
        public string TypeAddedContact { get; set; }
        public int LinkMessageId { get; set; }

        public String SaveContactButtonText { get; set; }
        public String SaveAndCreateContactButtonText { get; set; }

        public String AjaxProgressText { get; set; }

        protected List<Int32> OtherCompaniesID { get; set; }

        protected List<ContactInfoType> ContactInfoTypes { get; set; }

        private const string ErrorCookieKey = "save_contact_error";

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            ContactInfoTypes = (from ContactInfoType item in Enum.GetValues(typeof(ContactInfoType))
                                where (item != ContactInfoType.Address && item != ContactInfoType.Email &&
                                item != ContactInfoType.Phone)
                                select item).ToList();

            LinkMessageId = UrlParameters.LinkMessageId;

            saveContactButton.Text = SaveContactButtonText;
            saveContactButton.OnClientClick = String.Format("return ASC.CRM.ContactActionView.submitForm('{0}');", saveContactButton.UniqueID);

            saveAndCreateContactButton.Text = SaveAndCreateContactButtonText;
            saveAndCreateContactButton.OnClientClick = String.Format("return ASC.CRM.ContactActionView.submitForm('{0}');", saveAndCreateContactButton.UniqueID);


            List<CustomField> data;
            var networks = new List<ContactInfo>();
            if (TargetContact == null)
            {
                data = Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(UrlParameters.Type != "people" ? EntityType.Company : EntityType.Person);

                var URLEmail = UrlParameters.Email;
                if (!String.IsNullOrEmpty(URLEmail))
                    networks.Add(new ContactInfo()
                            {
                               Category = (int) ContactInfoBaseCategory.Work,
                               ContactID = 0,
                               Data = URLEmail.HtmlEncode(),
                               ID = 0,
                               InfoType = ContactInfoType.Email,
                               IsPrimary = true
                            });
                if (UrlParameters.Type != "people") {
                    //init ListContactView
                    RegisterClientScriptHelper.DataListContactTab(Page, 0, EntityType.Company);
                }
            }
            else
            {
                data = Global.DaoFactory.GetCustomFieldDao().GetEnityFields(
                    TargetContact is Person ? EntityType.Person : EntityType.Company,
                    TargetContact.ID, true);

                networks = Global.DaoFactory.GetContactInfoDao().GetList(TargetContact.ID, null, null, null).ConvertAll(
                n => new ContactInfo()
                                    {
                                        Category = n.Category,
                                        ContactID = n.ContactID,
                                        Data = n.Data.HtmlEncode(),
                                        ID = n.ID,
                                        InfoType = n.InfoType,
                                        IsPrimary = n.IsPrimary
                                    });
                if (TargetContact is Company) {
                    //init ListContactView
                    RegisterClientScriptHelper.DataListContactTab(Page, TargetContact.ID, EntityType.Company);
                }
            }

            RegisterClientScriptHelper.DataContactActionView(Page, TargetContact, data, networks);


            if (TargetContact != null)
            {
                cancelButton.Attributes.Add("href", String.Format("default.aspx?{0}={1}{2}", UrlConstant.ID, TargetContact.ID,
                    !String.IsNullOrEmpty(UrlParameters.Type) ?
                    String.Format("&{0}={1}", UrlConstant.Type, UrlParameters.Type) :
                    String.Empty));
            }
            else
            {
                cancelButton.Attributes.Add("href",
                             Request.UrlReferrer != null && Request.Url != null && String.Compare(Request.UrlReferrer.PathAndQuery, Request.Url.PathAndQuery) != 0
                                 ? Request.UrlReferrer.OriginalString
                                 : "default.aspx");
            }

            InitContactManagerSelector();

            RegisterScript();
        }

        #endregion

        #region Public Methods

        public String GetTitle()
        {
            if (TargetContact != null && TargetContact is Person)
                return ((Person)TargetContact).JobTitle.HtmlEncode();
            return String.Empty;
        }

        public String GetFirstName()
        {
            if (TargetContact != null && TargetContact is Person)
                return ((Person)TargetContact).FirstName.HtmlEncode();

            var URLFullName = UrlParameters.FullName;
            if (!String.IsNullOrEmpty(URLFullName))
            {
                var parts = URLFullName.Split(' ');
                return parts.Length < 2 ? URLFullName : parts[0].HtmlEncode();
            }

            return String.Empty;
        }

        public String GetLastName()
        {
            if (TargetContact != null && TargetContact is Person)
                return ((Person)TargetContact).LastName.HtmlEncode();

            var URLFullName = UrlParameters.FullName;
            if (!String.IsNullOrEmpty(URLFullName))
            {
                var parts = URLFullName.Split(' ');
                return (parts.Length < 2 ? String.Empty : URLFullName.Remove(0, parts[0].Length)).HtmlEncode();
            }
            return String.Empty;
        }

        public String GetCompanyName()
        {
            if (TargetContact != null && TargetContact is Company)
                return ((Company)TargetContact).CompanyName.HtmlEncode();
            return UrlParameters.FullName.HtmlEncode();
        }

        public String GetCompanyIDforPerson()
        {
            if ((TargetContact != null && TargetContact is Person))
                return ((Person)TargetContact).CompanyID.ToString();
            return String.Empty;
        }

        #endregion

        #region Methods

        protected void InitContactManagerSelector()
        {
            Dictionary<Guid,String> SelectedUsers = null;
            if (TargetContact != null)
            {
                var AccessSubjectTo = CRMSecurity.GetAccessSubjectTo(TargetContact, EmployeeStatus.Active).ToList();
                SelectedUsers = new Dictionary<Guid, String>();
                if (AccessSubjectTo.Count != 0)
                {
                    foreach (var item in AccessSubjectTo)
                    {
                        SelectedUsers.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                SelectedUsers = new Dictionary<Guid,String>();
                SelectedUsers.Add(SecurityContext.CurrentAccount.ID, SecurityContext.CurrentAccount.Name.HtmlEncode());
            }
            RegisterClientScriptHelper.DataUserSelectorListView(Page, "_ContactManager", SelectedUsers);
        }

        protected void SaveOrUpdateContact(object sender, CommandEventArgs e)
        {
            try
            {
                var dao = Global.DaoFactory;
                Contact contact;
                List<Contact> contactsForSetManager = new List<Contact>();

                var typeAddedContact = Request["typeAddedContact"];


                #region Rights part #1

                ShareType shareType = ShareType.None;// 0 - NotShared, 1 - ReadWriting, 2 - Reading

                if (!String.IsNullOrEmpty(Request["isPublicContact"]))
                {
                    try
                    {
                        shareType = (ShareType)(Convert.ToInt32(Request["isPublicContact"]));
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException();
                    }
                }


                #endregion

                #region BaseInfo

                var companyID = 0;

                if (!String.IsNullOrEmpty(Request["baseInfo_compID"]))
                {
                    companyID = Convert.ToInt32(Request["baseInfo_compID"]);
                }
                else if (!String.IsNullOrEmpty(Request["baseInfo_compName"]))
                {
                    var peopleCompany = new Company
                    {
                        CompanyName = Request["baseInfo_compName"].Trim(),
                        ShareType = shareType
                    };

                    peopleCompany.ID = dao.GetContactDao().SaveContact(peopleCompany);

                    companyID = peopleCompany.ID;
                    contactsForSetManager.Add(peopleCompany);
                }


                if (typeAddedContact.Equals("people"))
                {
                    contact = new Person
                                  {
                                      FirstName = Request["baseInfo_firstName"].Trim(),
                                      LastName = Request["baseInfo_lastName"].Trim(),
                                      JobTitle = Request["baseInfo_personPosition"].Trim(),
                                      CompanyID = companyID
                                  };
                }
                else
                {
                    contact = new Company
                                  {
                                      CompanyName = Request["baseInfo_companyName"].Trim()
                                  };
                }


                contact.About = !String.IsNullOrEmpty(Request["baseInfo_contactOverview"]) ? Request["baseInfo_contactOverview"].Trim() : null;
                contact.ShareType = shareType;

                #endregion

                #region ContactType and Currency

                contact.ContactTypeID = Convert.ToInt32(Request["baseInfo_contactType"]);
                if (contact.ContactTypeID != 0)
                {
                    var listItem = dao.GetListItemDao().GetByID(contact.ContactTypeID);
                    if (listItem == null)
                        throw new Exception(CRMErrorsResource.ContactTypeNotFound);
                }

                contact.Currency = Convert.ToString(Request["baseInfo_currency"]);
                if (!String.IsNullOrEmpty(contact.Currency))
                {
                    var currency = CurrencyProvider.Get(contact.Currency);
                    if (currency == null)
                        throw new Exception(CRMErrorsResource.CurrencyNotFound);
                }

                #endregion

                #region Base Operation Of Save/Update

                if (TargetContact != null)
                {
                    contact.ID = TargetContact.ID;
                    contact.StatusID = TargetContact.StatusID;
                    dao.GetContactDao().UpdateContact(contact);

                    var messageAction = contact is Company ? MessageAction.CompanyUpdated : MessageAction.PersonUpdated;
                    MessageService.Send(HttpContext.Current.Request, messageAction, contact.GetTitle());

                    contact = dao.GetContactDao().GetByID(contact.ID);
                }
                else
                {
                    contact.ID = dao.GetContactDao().SaveContact(contact);

                    var messageAction = contact is Company ? MessageAction.CompanyCreated : MessageAction.PersonCreated;
                    MessageService.Send(HttpContext.Current.Request, messageAction, contact.GetTitle());

                    contact = dao.GetContactDao().GetByID(contact.ID);
                }

                contactsForSetManager.Add(contact);

                #endregion

                #region persons for company

                if (contact is Company)
                {
                    var assignedContactsIDs = new List<int>();

                    if (!String.IsNullOrEmpty(Request["baseInfo_assignedNewContactsIDs"]))
                    {
                        try
                        {
                            var assignedContactsObjs = JArray.Parse(Request["baseInfo_assignedNewContactsIDs"]);
                            var newAssignedContacts = new List<Contact>();
                            var recordIndex = 0;

                            foreach (var assignedContactsObj in assignedContactsObjs)
                            {
                                newAssignedContacts.Add(new Person
                                {
                                    ID = recordIndex,
                                    ShareType = shareType,
                                    CompanyID = contact.ID,
                                    FirstName = assignedContactsObj.Value<String>("FirstName"),
                                    LastName = assignedContactsObj.Value<String>("LastName")
                                });
                                recordIndex++;
                            }

                            dao.GetContactDao().SaveContactList(newAssignedContacts);

                            if (newAssignedContacts.Count != 0)
                            {
                                contactsForSetManager.AddRange(newAssignedContacts);
                                assignedContactsIDs.AddRange(newAssignedContacts.Select(c => c.ID).ToList());
                            }
                        }
                        catch (Exception ex)
                        {
                            log4net.LogManager.GetLogger("ASC.CRM").Error(ex);
                        }

                    }

                    if (!String.IsNullOrEmpty(Request["baseInfo_assignedContactsIDs"]))
                    {
                        assignedContactsIDs.AddRange(Request["baseInfo_assignedContactsIDs"].Split(',').Select(item => Convert.ToInt32(item)).ToList());
                    }


                    if (TargetContact != null && !CRMSecurity.IsAdmin)
                    {
                        var restrictedMembers = dao.GetContactDao().GetRestrictedMembers(contact.ID);
                        assignedContactsIDs.AddRange(restrictedMembers.Select(m => m.ID).ToList());
                    }

                    dao.GetContactDao().SetMembers(contact.ID, assignedContactsIDs.ToArray());
                }

                #endregion

                #region tags

                var assignedTags = Request["baseInfo_assignedTags"];
                if (assignedTags != null)
                {
                    var oldTagList = dao.GetTagDao().GetEntityTags(EntityType.Contact, contact.ID);
                    foreach (var tag in oldTagList)
                    {
                        dao.GetTagDao().DeleteTagFromEntity(EntityType.Contact, contact.ID, tag);
                    }
                    if (assignedTags != string.Empty)
                    {
                        var tagListInfo = JObject.Parse(assignedTags)["tagListInfo"].ToArray();
                        var newTagList = tagListInfo.Select(t => t.ToString()).ToArray();
                        dao.GetTagDao().SetTagToEntity(EntityType.Contact, contact.ID, newTagList);
                    }
                }

                #endregion

                #region contact infos (addresses, mailes, phones etc.)

                var contactInfos = new List<ContactInfo>();
                var addressList = new Dictionary<int, ContactInfo>();
                var addressTemplate = new JObject();

                foreach (String addressPartName in Enum.GetNames(typeof(AddressPart)))
                {
                    addressTemplate.Add(addressPartName.ToLower(), "");
                }

                var addressTemplateStr = addressTemplate.ToString();

                foreach (var item in Request.Form.AllKeys)
                {
                    if (item.StartsWith("customField_"))
                    {
                        int fieldID = Convert.ToInt32(item.Split('_')[1]);
                        String fieldValue = Request.Form[item].Trim();

                        if (contact is Person)
                        {
                            if (!String.IsNullOrEmpty(fieldValue))
                            {
                                dao.GetCustomFieldDao().SetFieldValue(EntityType.Person, contact.ID, fieldID, "");
                            }
                            dao.GetCustomFieldDao().SetFieldValue(EntityType.Person, contact.ID, fieldID, fieldValue);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(fieldValue))
                            {
                                dao.GetCustomFieldDao().SetFieldValue(EntityType.Company, contact.ID, fieldID, "");
                            }
                            dao.GetCustomFieldDao().SetFieldValue(EntityType.Company, contact.ID, fieldID, fieldValue);
                        }
                    }
                    else if (item.StartsWith("contactInfo_"))
                    {
                        var nameParts = item.Split('_').Skip(1).ToList();
                        var contactInfoType = (ContactInfoType)Enum.Parse(typeof(ContactInfoType), nameParts[0]);
                        var category = Convert.ToInt32(nameParts[2]);

                        if (contactInfoType == ContactInfoType.Address)
                        {
                            var index = Convert.ToInt32(nameParts[1]);
                            var addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), nameParts[3]);
                            var isPrimaryAddress = Convert.ToInt32(nameParts[4]) == 1;
                            var dataValues = Request.Form.GetValues(item).Select(n => n.Trim()).ToList();

                            if (!addressList.ContainsKey(index))
                            {
                                var newAddress = new ContactInfo
                                                     {
                                                         Category = category,
                                                         InfoType = contactInfoType,
                                                         Data = addressTemplateStr,
                                                         IsPrimary = isPrimaryAddress,
                                                         ContactID = contact.ID
                                                     };
                                addressList.Add(index, newAddress);
                            }

                            foreach (var data in dataValues)
                            {
                                var addressParts = JObject.Parse(addressList[index].Data);
                                addressParts[addressPart.ToString().ToLower()] = data;
                                addressList[index].Data = addressParts.ToString();
                            }
                            continue;
                        }

                        var isPrimary = Convert.ToInt32(nameParts[3]) == 1;
                        if (Request.Form.GetValues(item) != null)
                        {
                            var dataValues = Request.Form.GetValues(item).Where(n => !string.IsNullOrEmpty(n.Trim())).ToList();

                            contactInfos.AddRange(dataValues.Select(dataValue => new ContactInfo
                                                                                     {
                                                                                         Category = category,
                                                                                         InfoType = contactInfoType,
                                                                                         Data = dataValue.Trim(),
                                                                                         IsPrimary = isPrimary,
                                                                                         ContactID = contact.ID
                                                                                     }));
                        }
                    }
                }

                if (addressList.Count > 0)
                    contactInfos.AddRange(addressList.Values.ToList());

                dao.GetContactInfoDao().DeleteByContact(contact.ID);
                dao.GetContactInfoDao().SaveList(contactInfos);

                #endregion

                #region Photo

                var photoPath = Request["uploadPhotoPath"];

                if (!String.IsNullOrEmpty(photoPath))
                {
                    if (photoPath != "null")
                    {
                        if (photoPath.StartsWith(PathProvider.BaseAbsolutePath))
                        {
                            var tmpDirName = photoPath.Substring(0, photoPath.LastIndexOf('/'));
                            ContactPhotoManager.TryUploadPhotoFromTmp(contact.ID, TargetContact == null, tmpDirName);
                        }
                        else
                        {
                            ContactPhotoManager.UploadPhoto(photoPath, contact.ID);
                        }
                    }
                }
                else if (TargetContact != null)
                {
                    ContactPhotoManager.DeletePhoto(TargetContact.ID);
                }
                #endregion


                #region Rights part #2

                SetContactManager(contactsForSetManager);

                #endregion

                #region Link with mail message

                int result;
                var linkMessageId = int.TryParse(Request["linkMessageId"], out result) ? result : 0;

                if (linkMessageId > 0) {
                    try
                    {
                        LinkWithMessage(linkMessageId, contact.ID);

                    }
                    catch(Exception ex)
                    {
                        log4net.LogManager.GetLogger("ASC.CRM").Error(ex);
                    }
                }

                #endregion

                Response.Redirect(String.Compare(e.CommandArgument.ToString(), "0", true) == 0
                                      ? String.Format("default.aspx?id={0}{1}", contact.ID,
                                                      contact is Company
                                                          ? ""
                                                          : String.Format("&{0}=people", UrlConstant.Type))
                                      : String.Format("default.aspx?action=manage{0}",
                                                      contact is Company
                                                          ? ""
                                                          : String.Format("&{0}=people", UrlConstant.Type)), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.CRM").Error(ex);
                var cookie = HttpContext.Current.Request.Cookies.Get(ErrorCookieKey);
                if (cookie == null)
                {
                    cookie = new HttpCookie(ErrorCookieKey)
                    {
                        Value = ex.Message
                    };
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }


        private void LinkWithMessage(int linkMessageId, int contactId)
        {
            var apiUrlSend = String.Format("{0}mail/conversations/crm/link.json", SetupInfo.WebApiBaseUrl);

            var bodySent = string.Format(
                "id_message={0}&crm_contact_ids[0][Id]={1}&crm_contact_ids[0][Type]=1",
                linkMessageId,
                contactId);

            var response = new ApiServer().GetApiResponse(apiUrlSend, "PUT", bodySent);

            if (response != null)
            {
                var responseObj =  JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(response)));

                if (responseObj["statusCode"].Value<int>() != (int)HttpStatusCode.OK)
                {
                    throw new Exception(string.Format(
                        "Link contact (id={0}) with mail message (id={1}) failed: {2}", contactId, linkMessageId, responseObj["error"]["message"].Value<string>()));
                }
            }
        }


        protected void SetContactManager(List<Contact> contacts)
        {
            var notifyContactManagers = false;
            bool value;

            if(bool.TryParse(Request.Form["notifyContactManagers"], out value))
            {
                notifyContactManagers = value;
            }

            var selectedManagers = Request.Form["selectedContactManagers"]
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => new Guid(item)).ToList();

            foreach (var contact in contacts)
            {
                if (notifyContactManagers)
                {
                    var notifyUsers = selectedManagers.Where(n => n != SecurityContext.CurrentAccount.ID).ToArray();
                    if (contact is Person)
                        Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Person, contact.ID, notifyUsers);
                    else
                        Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Company, contact.ID, notifyUsers);
                }

                CRMSecurity.SetAccessTo(contact, selectedManagers);
            }
        }

        protected string GetContactPhone(int contactID)
        {
            var phones = Global.DaoFactory.GetContactInfoDao().GetList(contactID, ContactInfoType.Phone, null, true);
            return phones.Count == 0 ? String.Empty : phones[0].Data.HtmlEncode();
        }

        protected string GetContactEmail(int contactID)
        {
            var emails = Global.DaoFactory.GetContactInfoDao().GetList(contactID, ContactInfoType.Email, null, true);
            return emails.Count == 0 ? String.Empty : emails[0].Data.HtmlEncode();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.ContactActionView.init({0},{1},""{2}"",{3},{4},""{5}"", ""{6}"");",
                TargetContact != null ? TargetContact.ID : 0,
                TargetContact != null ? TargetContact.ContactTypeID : 0,
                TargetContact != null ? TargetContact.Currency : "",
                TargetContact != null ? (int)TargetContact.ShareType : (int)ShareType.None,
                TargetContact != null && TargetContact is Person || TargetContact == null && UrlParameters.Type == "people" ?
                    (int)ContactSelectorTypeEnum.Companies :
                    (int)ContactSelectorTypeEnum.PersonsWithoutCompany,
                Studio.Core.FileSizeComment.GetFileImageSizeNote(CRMContactResource.ContactPhotoInfo, true),
                ErrorCookieKey
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}