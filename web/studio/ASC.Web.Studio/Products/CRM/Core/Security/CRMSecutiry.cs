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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core.Entities;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Core.Enums;
using Action = ASC.Common.Security.Authorizing.Action;
using Constants = ASC.Core.Users.Constants;
using SecurityContext = ASC.Core.SecurityContext;
using ASC.Web.CRM.Resources;

namespace ASC.CRM.Core
{
    public static class CRMSecurity
    {
        #region Members

        public static readonly IAction _actionRead = new Action(new Guid("{6F05C382-8BCA-4469-9424-C807A98C40D7}"), "", true, false);

        #endregion

        #region Check Permissions

        private static ISecurityObjectProvider GetCRMSecurityProvider()
        {
            return new CRMSecurityObjectProvider(Global.DaoFactory);
        }

        private static bool IsPrivate(ISecurityObjectId entity)
        {
            return GetAccessSubjectTo(entity).Any();
        }

        private static bool CanAccessTo(ISecurityObjectId entity)
        {
            return IsAdmin || SecurityContext.CheckPermissions(entity, GetCRMSecurityProvider(), _actionRead);
        }

        private static void MakePublic(ISecurityObjectId entity)
        {
            SetAccessTo(entity, new List<Guid>());
        }

        public static IEnumerable<int> GetPrivateItems(Type objectType)
        {
            if (IsAdmin) return new List<int>();

            return GetPrivateItems(objectType, Guid.Empty, true);

        }

        private static IEnumerable<int> GetPrivateItems(Type objectType, Guid userId, bool withoutUser)
        {
            var query = CoreContext.AuthorizationManager
                                   .GetAces(userId, _actionRead.ID)
                                   .Where(
                                       item =>
                                       !String.IsNullOrEmpty(item.ObjectId) &&
                                       item.ObjectId.StartsWith(objectType.FullName))
                                   .GroupBy(item => item.ObjectId, item => item.SubjectId);

            if (withoutUser)
            {
                if (userId != Guid.Empty)
                    query = query.Where(item => !item.Contains(userId));
                else
                    query = query.Where(item => !item.Contains(SecurityContext.CurrentAccount.ID));

            }

            return query.Select(item => Convert.ToInt32(item.Key.Split(new[] { '|' })[1]));
        }

        public static IEnumerable<int> GetContactsIdByManager(Guid userId)
        {
            return GetPrivateItems(typeof(Company), userId, false)
                .Union(GetPrivateItems(typeof(Person), userId, false));
        }

        public static int GetPrivateItemsCount(Type objectType)
        {
            if (IsAdmin) return 0;

            return GetPrivateItems(objectType).Count();
        }

        private static Dictionary<Guid, String> GetAccessSubjectTo(ISecurityObjectId entity, EmployeeStatus employeeStatus)
        {
            var allAces = CoreContext.AuthorizationManager.GetAcesWithInherits(Guid.Empty, _actionRead.ID, entity,
                                                                               GetCRMSecurityProvider())
                                     .Where(item => item.SubjectId != Constants.GroupEveryone.ID);

            var result = new Dictionary<Guid, String>();

            foreach (var azRecord in allAces)
            {
                if (!result.ContainsKey(azRecord.SubjectId))
                {
                    var userInfo = CoreContext.UserManager.GetUsers(azRecord.SubjectId);
                    var displayName = employeeStatus == EmployeeStatus.All || userInfo.Status == employeeStatus
                                          ? userInfo.DisplayUserName()
                                          : Constants.LostUser.DisplayUserName();
                    result.Add(azRecord.SubjectId, displayName);
                }
            }
            return result;
        }

        private static Dictionary<Guid, String> GetAccessSubjectTo(ISecurityObjectId entity)
        {
            return GetAccessSubjectTo(entity, EmployeeStatus.All);
        }

        private static List<Guid> GetAccessSubjectGuidsTo(ISecurityObjectId entity)
        {
            var allAces = CoreContext.AuthorizationManager.GetAcesWithInherits(Guid.Empty, _actionRead.ID, entity,
                                                                               GetCRMSecurityProvider())
                                     .Where(item => item.SubjectId != Constants.GroupEveryone.ID);

            var result = new List<Guid>();

            foreach (var azRecord in allAces)
            {
                if (!result.Contains(azRecord.SubjectId))
                    result.Add(azRecord.SubjectId);
            }
            return result;
        }

        private static void SetAccessTo(ISecurityObjectId entity, List<Guid> subjectID)
        {
            if (subjectID.Count == 0)
            {
                CoreContext.AuthorizationManager.RemoveAllAces(entity);
                return;
            }

            var aces = CoreContext.AuthorizationManager.GetAcesWithInherits(Guid.Empty, _actionRead.ID, entity, GetCRMSecurityProvider());
            foreach (var r in aces)
            {
                if (!subjectID.Contains(r.SubjectId) && (r.SubjectId != Constants.GroupEveryone.ID || r.Reaction != AceType.Allow))
                {
                    CoreContext.AuthorizationManager.RemoveAce(r);
                }
            }

            var oldSubjects = aces.Select(r => r.SubjectId).ToList();

            foreach (var s in subjectID)
            {
                if (!oldSubjects.Contains(s))
                {
                    CoreContext.AuthorizationManager.AddAce(new AzRecord(s, _actionRead.ID, AceType.Allow, entity));
                }
            }

            CoreContext.AuthorizationManager.AddAce(new AzRecord(Constants.GroupEveryone.ID, _actionRead.ID, AceType.Deny, entity));
        }

        #endregion

        #region SetAccessTo

        public static void SetAccessTo(File file)
        {
            if (IsAdmin || file.CreateBy == SecurityContext.CurrentAccount.ID || file.ModifiedBy == SecurityContext.CurrentAccount.ID)
                file.Access = FileShare.None;
            else
                file.Access = FileShare.Read;
        }

        public static void SetAccessTo(Deal deal, List<Guid> subjectID)
        {
            if (IsAdmin || deal.CreateBy == SecurityContext.CurrentAccount.ID)
            {
                SetAccessTo((ISecurityObjectId)deal, subjectID);
            }
        }

        public static void SetAccessTo(RelationshipEvent relationshipEvent, List<Guid> subjectID)
        {
            SetAccessTo((ISecurityObjectId)relationshipEvent, subjectID);
        }

        public static void SetAccessTo(Contact contact, List<Guid> subjectID)
        {
            SetAccessTo((ISecurityObjectId)contact, subjectID);
        }

        public static void SetAccessTo(Task task, List<Guid> subjectID)
        {
            SetAccessTo((ISecurityObjectId)task, subjectID);
        }

        public static void SetAccessTo(Cases cases, List<Guid> subjectID)
        {
            if (IsAdmin || cases.CreateBy == SecurityContext.CurrentAccount.ID)
            {
                SetAccessTo((ISecurityObjectId)cases, subjectID);
            }
        }

        public static void SetAccessTo(Invoice invoice, List<Guid> subjectID)
        {
            SetAccessTo((ISecurityObjectId)invoice, subjectID);
        }

        public static void SetAccessTo(InvoiceItem invoiceItem, List<Guid> subjectID)
        {
            SetAccessTo((ISecurityObjectId)invoiceItem, subjectID);
        }

        public static void SetAccessTo(InvoiceTax invoiceTax, List<Guid> subjectID)
        {
            SetAccessTo((ISecurityObjectId)invoiceTax, subjectID);
        }

        #endregion

        #region CanAccessTo

        public static bool CanAccessTo(Deal deal)
        {
            return CanAccessTo((ISecurityObjectId)deal);
        }

        public static bool CanAccessTo(RelationshipEvent relationshipEvent)
        {
            if (IsAdmin)
                return true;

            if (relationshipEvent.ContactID > 0)
            {
                var contactObj = Global.DaoFactory.GetContactDao().GetByID(relationshipEvent.ContactID);
                if (contactObj != null) return CanAccessTo(contactObj);
            }

            if (relationshipEvent.EntityType == EntityType.Case)
            {
                var caseObj = Global.DaoFactory.GetCasesDao().GetByID(relationshipEvent.EntityID);
                if (caseObj != null) return CanAccessTo(caseObj);
            }

            if (relationshipEvent.EntityType == EntityType.Opportunity)
            {
                var dealObj = Global.DaoFactory.GetDealDao().GetByID(relationshipEvent.EntityID);
                if (dealObj != null) return CanAccessTo(dealObj);
            }

            return false;

        }

        public static bool CanAccessTo(Contact contact)
        {
            return contact.ShareType == ShareType.Read || contact.ShareType == ShareType.ReadWrite || IsAdmin || GetAccessSubjectTo(contact).ContainsKey(SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(int contactID, EntityType entityType, ShareType? shareType, int companyID)
        {
            if (shareType.HasValue && (shareType.Value == ShareType.Read || shareType.Value == ShareType.ReadWrite) || IsAdmin)
            {
                return true;
            }
            if (entityType == EntityType.Company){
                var fakeContact = new Company() { ID = contactID };
                return GetAccessSubjectTo(fakeContact).ContainsKey(SecurityContext.CurrentAccount.ID);
            }
            else if (entityType == EntityType.Person)
            {
                var fakeContact = new Person() { ID = contactID, CompanyID = companyID };
                return GetAccessSubjectTo(fakeContact).ContainsKey(SecurityContext.CurrentAccount.ID);
            }
            return false;
        }

        public static bool CanAccessTo(Task task)
        {
            if (IsAdmin || task.ResponsibleID == SecurityContext.CurrentAccount.ID ||
                (task.ContactID == 0 && task.EntityID == 0) || task.CreateBy == SecurityContext.CurrentAccount.ID) return true;

            if (task.ContactID > 0)
            {
                var contactObj = Global.DaoFactory.GetContactDao().GetByID(task.ContactID);

                if (contactObj != null) return CanAccessTo(contactObj);

                // task.ContactID = 0;

                //  Global.DaoFactory.GetTaskDao().SaveOrUpdateTask(task);

            }

            if (task.EntityType == EntityType.Case)
            {
                var caseObj = Global.DaoFactory.GetCasesDao().GetByID(task.EntityID);

                if (caseObj != null) return CanAccessTo(caseObj);

                //   task.EntityType = EntityType.Any;
                //   task.EntityID = 0;

                //   Global.DaoFactory.GetTaskDao().SaveOrUpdateTask(task);

            }

            if (task.EntityType == EntityType.Opportunity)
            {
                var dealObj = Global.DaoFactory.GetDealDao().GetByID(task.EntityID);

                if (dealObj != null) return CanAccessTo(dealObj);

                //   task.EntityType = EntityType.Any;
                //  task.EntityID = 0;

                //  Global.DaoFactory.GetTaskDao().SaveOrUpdateTask(task);
            }

            return false;

        }

        public static bool CanAccessTo(Cases cases)
        {
            return CanAccessTo((ISecurityObjectId)cases);
        }

        public static bool CanAccessTo(Invoice invoice)
        {
            if (IsAdmin || invoice.CreateBy == SecurityContext.CurrentAccount.ID) return true;

            if (invoice.ContactID > 0)
                return CanAccessTo(Global.DaoFactory.GetContactDao().GetByID(invoice.ContactID));

            if (invoice.EntityType == EntityType.Opportunity)
                return CanAccessTo(Global.DaoFactory.GetDealDao().GetByID(invoice.EntityID));

            return false;
        }

        public static bool CanAccessTo(InvoiceTax invoiceTax)
        {
            if (IsAdmin || invoiceTax.CreateBy == SecurityContext.CurrentAccount.ID) return true;

            return false;
        }

        #endregion

        #region CanEdit

        public static bool CanEdit(File file)
        {
            if (!(IsAdmin || file.CreateBy == SecurityContext.CurrentAccount.ID || file.ModifiedBy == SecurityContext.CurrentAccount.ID))
                return false;

            if ((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                return false;

            return true;
        }

        public static bool CanEdit(Deal deal)
        {
            return (IsAdmin || deal.ResponsibleID == SecurityContext.CurrentAccount.ID || deal.CreateBy == SecurityContext.CurrentAccount.ID ||
                !CRMSecurity.IsPrivate(deal) || GetAccessSubjectTo(deal).ContainsKey(SecurityContext.CurrentAccount.ID));
        }

        public static bool CanEdit(RelationshipEvent relationshipEvent)
        {
            return CanAccessTo(relationshipEvent);
        }

        public static bool CanEdit(Contact contact)
        {
            return contact.ShareType == ShareType.ReadWrite || IsAdmin || GetAccessSubjectTo(contact).ContainsKey(SecurityContext.CurrentAccount.ID);
        }

        public static bool CanEdit(Task task)
        {
            return (IsAdmin || task.ResponsibleID == SecurityContext.CurrentAccount.ID || task.CreateBy == SecurityContext.CurrentAccount.ID);
        }

        public static bool CanEdit(Cases cases)
        {
            return (IsAdmin || cases.CreateBy == SecurityContext.CurrentAccount.ID ||
                !CRMSecurity.IsPrivate(cases) || GetAccessSubjectTo(cases).ContainsKey(SecurityContext.CurrentAccount.ID));
        }

        public static bool CanEdit(Invoice invoice)
        {
            return (IsAdmin || invoice.CreateBy == SecurityContext.CurrentAccount.ID) && invoice.Status == InvoiceStatus.Draft;
        }

        public static bool CanEdit(InvoiceTax invoiceTax)
        {
            return IsAdmin;
        }

        public static bool CanEdit(InvoiceItem invoiceItem)
        {
            return IsAdmin;
        }

        #endregion

        #region CanDelete

        public static bool CanDelete(Contact contact)
        {
            return CanEdit(contact) && Global.DaoFactory.GetContactDao().CanDelete(contact.ID);
        }

        public static bool CanDelete(Invoice invoice)
        {
            return (IsAdmin || invoice.CreateBy == SecurityContext.CurrentAccount.ID);
        }

        public static bool CanDelete(InvoiceItem invoiceItem)
        {
            return CanEdit(invoiceItem) && Global.DaoFactory.GetInvoiceItemDao().CanDelete(invoiceItem.ID);
        }

        public static bool CanDelete(InvoiceTax invoiceTax)
        {
            return CanEdit(invoiceTax) && Global.DaoFactory.GetInvoiceTaxDao().CanDelete(invoiceTax.ID);
        }

        public static bool CanDelete(Deal deal)
        {
            return CanEdit(deal);
        }

        public static bool CanDelete(Cases cases)
        {
            return CanEdit(cases);
        }

        public static bool CanDelete(RelationshipEvent relationshipEvent)
        {
            return CanEdit(relationshipEvent);
        }

        #endregion

        #region MakePublic

        public static void MakePublic(Deal deal)
        {
            MakePublic((ISecurityObjectId)deal);
        }

        public static void MakePublic(RelationshipEvent relationshipEvent)
        {
            MakePublic((ISecurityObjectId)relationshipEvent);
        }

        public static void MakePublic(Contact contact)
        {
            MakePublic((ISecurityObjectId)contact);
        }

        public static void MakePublic(Task task)
        {
            MakePublic((ISecurityObjectId)task);
        }

        public static void MakePublic(Cases cases)
        {
            MakePublic((ISecurityObjectId)cases);
        }

        public static void MakePublic(Invoice invoice)
        {
            MakePublic((ISecurityObjectId)invoice);
        }

        public static void MakePublic(InvoiceItem invoiceItem)
        {
            MakePublic((ISecurityObjectId)invoiceItem);
        }

        public static void MakePublic(InvoiceTax invoiceTax)
        {
            MakePublic((ISecurityObjectId)invoiceTax);
        }

        #endregion

        #region IsPrivate

        public static bool IsPrivate(Deal deal)
        {
            return IsPrivate((ISecurityObjectId)deal);
        }

        public static bool IsPrivate(RelationshipEvent relationshipEvent)
        {
            return IsPrivate((ISecurityObjectId)relationshipEvent);
        }

        public static bool IsPrivate(Contact contact)
        {
            return !CanAccessTo(contact);
        }

        public static bool IsPrivate(Task task)
        {
            return IsPrivate((ISecurityObjectId)task);
        }

        public static bool IsPrivate(Cases cases)
        {
            return IsPrivate((ISecurityObjectId)cases);
        }

        public static bool IsPrivate(Invoice invoice)
        {
            return IsPrivate((ISecurityObjectId)invoice);
        }

        public static bool IsPrivate(InvoiceItem invoiceItem)
        {
            return IsPrivate((ISecurityObjectId)invoiceItem);
        }

        public static bool IsPrivate(InvoiceTax invoiceTax)
        {
            return IsPrivate((ISecurityObjectId)invoiceTax);
        }

        #endregion

        #region GetAccessSubjectTo

        public static Dictionary<Guid, string> GetAccessSubjectTo(Deal deal)
        {
            return GetAccessSubjectTo((ISecurityObjectId)deal);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(RelationshipEvent relationshipEvent)
        {
            return GetAccessSubjectTo((ISecurityObjectId)relationshipEvent);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(Contact contact)
        {
            return GetAccessSubjectTo((ISecurityObjectId)contact);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(Contact contact, EmployeeStatus employeeStatus)
        {
            return GetAccessSubjectTo((ISecurityObjectId)contact, employeeStatus);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(Task task)
        {
            return GetAccessSubjectTo((ISecurityObjectId)task);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(Cases cases)
        {
            return GetAccessSubjectTo((ISecurityObjectId)cases);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(Invoice invoice)
        {
            return GetAccessSubjectTo((ISecurityObjectId)invoice);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(InvoiceItem invoiceItem)
        {
            return GetAccessSubjectTo((ISecurityObjectId)invoiceItem);
        }

        public static Dictionary<Guid, string> GetAccessSubjectTo(InvoiceTax invoiceTax)
        {
            return GetAccessSubjectTo((ISecurityObjectId)invoiceTax);
        }

        #endregion

        #region GetAccessSubjectGuidsTo

        public static List<Guid> GetAccessSubjectGuidsTo(Deal deal)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)deal);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(RelationshipEvent relationshipEvent)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)relationshipEvent);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(Contact contact)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)contact);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(Task task)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)task);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(Cases cases)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)cases);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(Invoice invoice)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)invoice);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(InvoiceItem invoiceItem)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)invoiceItem);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(InvoiceTax invoiceTax)
        {
            return GetAccessSubjectGuidsTo((ISecurityObjectId)invoiceTax);
        }

        #endregion

        #region DemandAccessTo

        public static void DemandAccessTo(File file)
        {
            //   if (!CanAccessTo((File)file)) CreateSecurityException();
        }

        public static void DemandAccessTo(Deal deal)
        {
            if (!CanAccessTo(deal)) throw CreateSecurityException();
        }

        public static void DemandAccessTo(RelationshipEvent relationshipEvent)
        {
            if (!CanAccessTo(relationshipEvent)) throw CreateSecurityException();
        }

        public static void DemandAccessTo(Contact contact)
        {
            if (!CanAccessTo(contact)) throw CreateSecurityException();
        }

        public static void DemandAccessTo(Task task)
        {
            if (!CanAccessTo(task)) throw CreateSecurityException();
        }

        public static void DemandAccessTo(Cases cases)
        {
            if (!CanAccessTo(cases)) throw CreateSecurityException();
        }

        public static void DemandAccessTo(Invoice invoice)
        {
            if (!CanAccessTo(invoice)) throw CreateSecurityException();
        }

        public static void DemandAccessTo(InvoiceTax invoiceTax)
        {
            if (!CanAccessTo(invoiceTax)) throw CreateSecurityException();
        }

        #endregion

        #region DemandEdit

        public static void DemandEdit(File file)
        {
            if (!CanEdit(file)) throw CreateSecurityException();
        }

        public static void DemandEdit(Deal deal)
        {
            if (!CanEdit(deal)) throw CreateSecurityException();
        }

        public static void DemandEdit(RelationshipEvent relationshipEvent)
        {
            if (!CanEdit(relationshipEvent)) throw CreateSecurityException();
        }

        public static void DemandEdit(Contact contact)
        {
            if (!CanEdit(contact)) throw CreateSecurityException();
        }

        public static void DemandEdit(Task task)
        {
            if (!CanEdit(task)) throw CreateSecurityException();
        }

        public static void DemandEdit(Cases cases)
        {
            if (!CanEdit(cases)) throw CreateSecurityException();
        }

        public static void DemandEdit(Invoice invoice)
        {
            if (!CanEdit(invoice)) throw CreateSecurityException();
        }

        public static void DemandEdit(InvoiceTax invoiceTax)
        {
            if (!CanEdit(invoiceTax)) throw CreateSecurityException();
        }

        public static void DemandEdit(InvoiceItem invoiceItem)
        {
            if (!CanEdit(invoiceItem)) throw CreateSecurityException();
        }

        #endregion

        #region DemandDelete

        public static void DemandDelete(File file)
        {
            if (!CanEdit(file)) throw CreateSecurityException();
        }


        public static void DemandDelete(Contact contact)
        {
            if (!CanDelete(contact)) throw CreateSecurityException();
        }

        public static void DemandDelete(Invoice invoice)
        {
            if (!CanDelete(invoice)) throw CreateSecurityException();
        }

        public static void DemandDelete(Deal deal)
        {
            if (!CanDelete(deal)) throw CreateSecurityException();
        }

        public static void DemandDelete(Cases cases)
        {
            if (!CanDelete(cases)) throw CreateSecurityException();
        }

        public static void DemandDelete(InvoiceItem invoiceItem)
        {
            if (!CanDelete(invoiceItem)) throw CreateSecurityException();
        }

        public static void DemandDelete(InvoiceTax invoiceTax)
        {
            if (!CanDelete(invoiceTax)) throw CreateSecurityException();
        }

        public static void DemandDelete(RelationshipEvent relationshipEvent)
        {
           if (!CanDelete(relationshipEvent)) throw CreateSecurityException();
        }

        #endregion

        #region DemandCreateOrUpdate

        public static void DemandCreateOrUpdate(RelationshipEvent relationshipEvent)
        {
            if (String.IsNullOrEmpty(relationshipEvent.Content) || relationshipEvent.CategoryID == 0 || (relationshipEvent.ContactID == 0 && relationshipEvent.EntityID == 0))
                throw new ArgumentException();

            if (relationshipEvent.EntityID > 0 && relationshipEvent.EntityType != EntityType.Opportunity && relationshipEvent.EntityType != EntityType.Case)
                throw new ArgumentException();

            if (relationshipEvent.Content.Length > Global.MaxHistoryEventCharacters)
                throw new ArgumentException(CRMErrorsResource.HistoryEventDataTooLong);

            if (!CanAccessTo(relationshipEvent)) throw CreateSecurityException();
        }

        public static void DemandCreateOrUpdate(Deal deal)
        {
            if (string.IsNullOrEmpty(deal.Title) || deal.ResponsibleID == Guid.Empty ||
                deal.DealMilestoneID <= 0 || string.IsNullOrEmpty(deal.BidCurrency))
                throw new ArgumentException();


            var listItem = Global.DaoFactory.GetDealMilestoneDao().GetByID(deal.DealMilestoneID);
            if (listItem == null) throw new ArgumentException(CRMErrorsResource.DealMilestoneNotFound);

            if (deal.ContactID != 0)
            {
                var contact = Global.DaoFactory.GetContactDao().GetByID(deal.ContactID);
                if (contact == null) throw new ArgumentException();

                if (!CanAccessTo(contact)) throw new SecurityException(CRMErrorsResource.AccessDenied);
            }

            if (string.IsNullOrEmpty(deal.BidCurrency))
            {
                throw new ArgumentException();
            }
            else
            {
                if (CurrencyProvider.Get(deal.BidCurrency.ToUpper()) == null)
                {
                    throw new ArgumentException();
                }
            }
        }

        public static void DemandCreateOrUpdate(InvoiceLine line, Invoice targetInvoice)
        {
            if (line.InvoiceID <= 0 || line.InvoiceItemID <= 0 ||
                line.Quantity < 0 || line.Price < 0 || line.Discount < 0 || line.Discount > 100 ||
                line.InvoiceTax1ID < 0 || line.InvoiceTax2ID < 0)
                throw new ArgumentException();

            if (targetInvoice == null || targetInvoice.ID != line.InvoiceID) throw new ArgumentException();
            if (!CRMSecurity.CanEdit(targetInvoice)) throw CRMSecurity.CreateSecurityException();

            if (!Global.DaoFactory.GetInvoiceItemDao().IsExist(line.InvoiceItemID))
                throw new ArgumentException();

            if (line.InvoiceTax1ID > 0 && !Global.DaoFactory.GetInvoiceTaxDao().IsExist(line.InvoiceTax1ID))
                throw new ArgumentException();

            if (line.InvoiceTax2ID > 0 && !Global.DaoFactory.GetInvoiceTaxDao().IsExist(line.InvoiceTax2ID))
                throw new ArgumentException();
        }

        public static void DemandCreateOrUpdate(Invoice invoice)
        {
            if (invoice.IssueDate == DateTime.MinValue ||
                invoice.ContactID <= 0 ||
                invoice.DueDate == DateTime.MinValue ||
                String.IsNullOrEmpty(invoice.Currency) ||
                invoice.ExchangeRate <= 0 ||
                String.IsNullOrEmpty(invoice.Terms))
                    throw new ArgumentException();

            var contact = Global.DaoFactory.GetContactDao().GetByID(invoice.ContactID);
            if (contact == null) throw new ArgumentException();
            if (!CanAccessTo(contact)) throw new SecurityException(CRMErrorsResource.AccessDenied);

            if (invoice.ConsigneeID != 0 && invoice.ConsigneeID != invoice.ContactID)
            {
                var consignee = Global.DaoFactory.GetContactDao().GetByID(invoice.ConsigneeID);
                if (consignee == null) throw new ArgumentException();
                if (!CanAccessTo(consignee)) throw new SecurityException(CRMErrorsResource.AccessDenied);
            }

            if (invoice.EntityID != 0)
            {
                var deal = Global.DaoFactory.GetDealDao().GetByID(invoice.EntityID);
                if (deal == null) throw new ArgumentException();
                if (!CanAccessTo(deal)) throw new SecurityException(CRMErrorsResource.AccessDenied);

                var dealMembers = Global.DaoFactory.GetDealDao().GetMembers(invoice.EntityID);
                if (!dealMembers.Contains(invoice.ContactID))
                    throw new ArgumentException();
            }

            if (CurrencyProvider.Get(invoice.Currency.ToUpper()) == null)
            {
                throw new ArgumentException();
            }
        }

        #endregion

        public static Exception CreateSecurityException()
        {
            throw new SecurityException(CRMErrorsResource.AccessDenied);
        }

        public static bool IsAdmin
        {
            get
            {
                return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID) ||
                       WebItemSecurity.IsProductAdministrator(ProductEntryPoint.ID, SecurityContext.CurrentAccount.ID);
            }
        }

        public static bool IsAdministrator(Guid userId)
        {
            return CoreContext.UserManager.IsUserInGroup(userId, Constants.GroupAdmin.ID) ||
                   WebItemSecurity.IsProductAdministrator(ProductEntryPoint.ID, userId);
        }

        public static IEnumerable<Task> FilterRead(IEnumerable<Task> tasks)
        {
            if (tasks == null || !tasks.Any()) return new List<Task>();

            if (IsAdmin) return tasks;

            var result = tasks;
            var contactIDs = result.Where(x => x.ResponsibleID != SecurityContext.CurrentAccount.ID).Select(x => x.ContactID).Distinct();

            if (contactIDs.Any())
            {
                contactIDs = Global.DaoFactory.GetContactDao()
                                   .GetContacts(contactIDs.ToArray())
                                   .FindAll(CanAccessTo)
                                   .Select(x => x.ID);

                result = result.Where(x => x.ContactID == 0 || contactIDs.Contains(x.ContactID) || x.ResponsibleID == SecurityContext.CurrentAccount.ID);

                if (!result.Any()) return Enumerable.Empty<Task>();
            }

            var casesIds = result.Where(x => x.EntityType == EntityType.Case && x.ResponsibleID != SecurityContext.CurrentAccount.ID).Select(x => x.EntityID).Distinct();

            if (casesIds.Any())
            {
                casesIds = Global.DaoFactory.GetCasesDao()
                                   .GetCases(casesIds.ToArray())
                                   .FindAll(CanAccessTo)
                                   .Select(x => x.ID);

                result = result.Where(x => x.EntityID == 0 || casesIds.Contains(x.EntityID) || x.ResponsibleID == SecurityContext.CurrentAccount.ID);

                if (!result.Any()) return Enumerable.Empty<Task>();
            }

            var dealsIds = result.Where(x => x.EntityType == EntityType.Opportunity && x.ResponsibleID != SecurityContext.CurrentAccount.ID).Select(x => x.EntityID).Distinct();

            if (dealsIds.Any())
            {
                dealsIds = Global.DaoFactory.GetDealDao()
                                   .GetDeals(dealsIds.ToArray())
                                   .FindAll(CanAccessTo)
                                   .Select(x => x.ID);

                result = result.Where(x => x.EntityID == 0 || dealsIds.Contains(x.EntityID) || x.ResponsibleID == SecurityContext.CurrentAccount.ID);

                if (!result.Any()) return Enumerable.Empty<Task>();
            }

            return result.ToList();

        }

        public static IEnumerable<Invoice> FilterRead(IEnumerable<Invoice> invoices)
        {
            if (invoices == null || !invoices.Any()) return new List<Invoice>();

            if (IsAdmin) return invoices;

            var result = invoices;
            var contactIDs = result.Select(x => x.ContactID).Distinct();

            if (contactIDs.Any())
            {
                contactIDs = Global.DaoFactory.GetContactDao()
                                   .GetContacts(contactIDs.ToArray())
                                   .FindAll(CanAccessTo)
                                   .Select(x => x.ID);

                result = result.Where(x => x.ContactID == 0 || contactIDs.Contains(x.ContactID));

                if (!result.Any()) return Enumerable.Empty<Invoice>();
            }

            return result.ToList();
        }

        public static bool CanGoToFeed(Task task)
        {
            return IsAdmin || task.ResponsibleID == SecurityContext.CurrentAccount.ID || task.CreateBy == SecurityContext.CurrentAccount.ID;
        }


    }
}