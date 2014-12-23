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

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Thrdparty.Configuration;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Mobile;
using Newtonsoft.Json.Linq;
using ASC.Web.CRM.Core.Enums;


namespace ASC.Web.CRM.Controls.Contacts
{
    public partial class ContactActionView : BaseUserControl
    {
        #region Properies

        public static string Location { get { return PathProvider.GetFileStaticRelativePath("Contacts/ContactActionView.ascx"); } }
        public Contact TargetContact { get; set; }
        public String TypeAddedContact { get; set; }

        public String SaveContactButtonText { get; set; }
        public String SaveAndCreateContactButtonText { get; set; }

        public String AjaxProgressText { get; set; }

        protected List<Int32> OtherCompaniesID { get; set; }

        protected List<ContactInfoType> ContactInfoTypes { get; set; }
        protected bool MobileVer = false;

        protected bool IsCrunchBaseSearchEnabled
        {
            get { return !string.IsNullOrEmpty(KeyStorage.Get("crunchBaseKey")); }
        }

        private const string ErrorCookieKey = "save_contact_error";

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;

            ContactInfoTypes = (from ContactInfoType item in Enum.GetValues(typeof(ContactInfoType))
                                where (item != ContactInfoType.Address && item != ContactInfoType.Email &&
                                item != ContactInfoType.Phone)
                                select item).ToList();


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
                return parts.Length < 2 ? String.Empty : parts[0].HtmlEncode();
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
                return (parts.Length < 2 ? URLFullName : URLFullName.Remove(0, parts[0].Length)).HtmlEncode();
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
                        throw new Exception("Contact type is not found");
                }

                contact.Currency = Convert.ToString(Request["baseInfo_currency"]);
                if (!String.IsNullOrEmpty(contact.Currency))
                {
                    var currency = CurrencyProvider.Get(contact.Currency);
                    if (currency == null)
                        throw new Exception("Currency is not found");
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

                            var ids = dao.GetContactDao().SaveContactList(newAssignedContacts);

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

                var emails = contactInfos
                    .Where(item => item.InfoType == ContactInfoType.Email)
                    .Select(item => item.Data)
                    .ToList();

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