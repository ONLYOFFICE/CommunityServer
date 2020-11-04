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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Core.Enums;
using Action = ASC.Common.Security.Authorizing.Action;
using Constants = ASC.Core.Users.Constants;
using SecurityContext = ASC.Core.SecurityContext;
using ASC.Web.CRM.Resources;
using Autofac;

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
            return new CRMSecurityObjectProvider();
        }

        public static bool IsPrivate(ISecurityObjectId entity)
        {
            return GetAccessSubjectTo(entity).Any();
        }

        public static bool CanAccessTo(ISecurityObjectId entity)
        {
            return CanAccessTo(entity, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(ISecurityObjectId entity, Guid userId)
        {
            return IsAdministrator(userId) || SecurityContext.CheckPermissions(entity, GetCRMSecurityProvider(), _actionRead);
        }

        public static void MakePublic(ISecurityObjectId entity)
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

        public static Dictionary<Guid, String> GetAccessSubjectTo(ISecurityObjectId entity)
        {
            return GetAccessSubjectTo(entity, EmployeeStatus.All);
        }

        public static List<Guid> GetAccessSubjectGuidsTo(ISecurityObjectId entity)
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

        public static void SetAccessTo(ISecurityObjectId entity, List<Guid> subjectID)
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

        public static void SetAccessTo(Cases cases, List<Guid> subjectID)
        {
            if (IsAdmin || cases.CreateBy == SecurityContext.CurrentAccount.ID)
            {
                SetAccessTo((ISecurityObjectId)cases, subjectID);
            }
        }

        #endregion

        #region CanAccessTo


        public static bool CanAccessTo(RelationshipEvent relationshipEvent)
        {
            return CanAccessTo(relationshipEvent, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(RelationshipEvent relationshipEvent, Guid userId)
        {
            if (IsAdministrator(userId))
                return true;

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();

                if (relationshipEvent.ContactID > 0)
                {
                    var contactObj = daoFactory.ContactDao.GetByID(relationshipEvent.ContactID);
                    if (contactObj != null) return CanAccessTo(contactObj, userId);
                }

                if (relationshipEvent.EntityType == EntityType.Case)
                {
                    var caseObj = daoFactory.CasesDao.GetByID(relationshipEvent.EntityID);
                    if (caseObj != null) return CanAccessTo(caseObj, userId);
                }

                if (relationshipEvent.EntityType == EntityType.Opportunity)
                {
                    var dealObj = daoFactory.DealDao.GetByID(relationshipEvent.EntityID);
                    if (dealObj != null) return CanAccessTo(dealObj, userId);
                }

                return false;
            }
        }

        public static bool CanAccessTo(Contact contact)
        {
            return CanAccessTo(contact, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(Contact contact, Guid userId)
        {
            return contact.ShareType == ShareType.Read ||
                contact.ShareType == ShareType.ReadWrite ||
                IsAdministrator(userId) ||
                GetAccessSubjectTo(contact).ContainsKey(userId);
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
            return CanAccessTo(task, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(Task task, Guid userId)
        {
            if (IsAdministrator(userId) || task.ResponsibleID == userId ||
                (task.ContactID == 0 && task.EntityID == 0) || task.CreateBy == userId)
                return true;

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                if (task.ContactID > 0)
                {
                    var contactObj = daoFactory.ContactDao.GetByID(task.ContactID);
                    if (contactObj != null) return CanAccessTo(contactObj, userId);
                }

                if (task.EntityType == EntityType.Case)
                {
                    var caseObj = daoFactory.CasesDao.GetByID(task.EntityID);
                    if (caseObj != null) return CanAccessTo(caseObj, userId);
                }

                if (task.EntityType == EntityType.Opportunity)
                {
                    var dealObj = daoFactory.DealDao.GetByID(task.EntityID);
                    if (dealObj != null) return CanAccessTo(dealObj, userId);
                }

                return false;
            }
        }

        public static bool CanAccessTo(Invoice invoice)
        {
            return CanAccessTo(invoice, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(Invoice invoice, Guid userId)
        {
            if (IsAdministrator(userId) || invoice.CreateBy == userId) return true;

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                if (invoice.ContactID > 0)
                    return CanAccessTo(daoFactory.ContactDao.GetByID(invoice.ContactID), userId);

                if (invoice.EntityType == EntityType.Opportunity)
                    return CanAccessTo(daoFactory.DealDao.GetByID(invoice.EntityID), userId);

                return false;
            }
        }

        public static bool CanAccessTo(InvoiceTax invoiceTax)
        {
            return CanAccessTo(invoiceTax, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanAccessTo(InvoiceTax invoiceTax, Guid userId)
        {
            if (IsAdministrator(userId) || invoiceTax.CreateBy == userId) return true;

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
            var userId = SecurityContext.CurrentAccount.ID;

            if (IsAdmin) return true;

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();

                if (relationshipEvent.ContactID > 0)
                {
                    var contactObj = daoFactory.ContactDao.GetByID(relationshipEvent.ContactID);
                    if (contactObj != null)
                    {
                        if(CanEdit(contactObj)) return true;

                        return CanAccessTo(contactObj, userId) && relationshipEvent.CreateBy == userId;
                    }
                }

                if (relationshipEvent.EntityType == EntityType.Case)
                {
                    var caseObj = daoFactory.CasesDao.GetByID(relationshipEvent.EntityID);
                    if (caseObj != null)
                    {
                        if (CanEdit(caseObj)) return true;

                        return CanAccessTo(caseObj, userId) && relationshipEvent.CreateBy == userId;
                    }
                }

                if (relationshipEvent.EntityType == EntityType.Opportunity)
                {
                    var dealObj = daoFactory.DealDao.GetByID(relationshipEvent.EntityID);
                    if (dealObj != null)
                    {
                        if (CanEdit(dealObj)) return true;

                        return CanAccessTo(dealObj, userId) && relationshipEvent.CreateBy == userId;
                    }
                }

                return false;
            }
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
            using (var scope = DIHelper.Resolve())
            {
                return CanEdit(contact) && scope.Resolve<DaoFactory>().ContactDao.CanDelete(contact.ID);
            }
        }

        public static bool CanDelete(Invoice invoice)
        {
            return (IsAdmin || invoice.CreateBy == SecurityContext.CurrentAccount.ID);
        }

        public static bool CanDelete(InvoiceItem invoiceItem)
        {
            using (var scope = DIHelper.Resolve())
            {
                return CanEdit(invoiceItem) && scope.Resolve<DaoFactory>().InvoiceItemDao.CanDelete(invoiceItem.ID);
            }
        }

        public static bool CanDelete(InvoiceTax invoiceTax)
        {
            using (var scope = DIHelper.Resolve())
            {
                return CanEdit(invoiceTax) && scope.Resolve<DaoFactory>().InvoiceTaxDao.CanDelete(invoiceTax.ID);
            }
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

        #region IsPrivate

        public static bool IsPrivate(Contact contact)
        {
            return contact.ShareType == ShareType.None;
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


            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var listItem = daoFactory.DealMilestoneDao.GetByID(deal.DealMilestoneID);
                if (listItem == null) throw new ArgumentException(CRMErrorsResource.DealMilestoneNotFound);

                if (deal.ContactID != 0)
                {
                    var contact = daoFactory.ContactDao.GetByID(deal.ContactID);
                    if (contact == null) throw new ArgumentException();

                    if (!CanAccessTo(contact)) throw new SecurityException(CRMErrorsResource.AccessDenied);
                }
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

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                if (!daoFactory.InvoiceItemDao.IsExist(line.InvoiceItemID))
                    throw new ArgumentException();

                if (line.InvoiceTax1ID > 0 && !daoFactory.InvoiceTaxDao.IsExist(line.InvoiceTax1ID))
                    throw new ArgumentException();

                if (line.InvoiceTax2ID > 0 && !daoFactory.InvoiceTaxDao.IsExist(line.InvoiceTax2ID))
                    throw new ArgumentException();
            }
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

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var contact = daoFactory.ContactDao.GetByID(invoice.ContactID);
                if (contact == null) throw new ArgumentException();
                if (!CanAccessTo(contact)) throw new SecurityException(CRMErrorsResource.AccessDenied);

                if (invoice.ConsigneeID != 0 && invoice.ConsigneeID != invoice.ContactID)
                {
                    var consignee = daoFactory.ContactDao.GetByID(invoice.ConsigneeID);
                    if (consignee == null) throw new ArgumentException();
                    if (!CanAccessTo(consignee)) throw new SecurityException(CRMErrorsResource.AccessDenied);
                }

                if (invoice.EntityID != 0)
                {
                    var deal = daoFactory.DealDao.GetByID(invoice.EntityID);
                    if (deal == null) throw new ArgumentException();
                    if (!CanAccessTo(deal)) throw new SecurityException(CRMErrorsResource.AccessDenied);

                    var dealMembers = daoFactory.DealDao.GetMembers(invoice.EntityID);
                    if (!dealMembers.Contains(invoice.ContactID))
                        throw new ArgumentException();
                }
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
                return IsAdministrator(SecurityContext.CurrentAccount.ID);
            }
        }

        public static bool IsAdministrator(Guid userId)
        {
            return WebItemSecurity.IsProductAdministrator(ProductEntryPoint.ID, userId);
        }

        public static bool IsAvailableForUser(Guid userId)
        {
            return WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID, userId);
        }

        public static IEnumerable<Task> FilterRead(IEnumerable<Task> tasks)
        {
            if (tasks == null || !tasks.Any()) return new List<Task>();

            if (IsAdmin) return tasks;

            var result = tasks.ToList();
            var contactIDs = result
                .Where(x => x.ResponsibleID != SecurityContext.CurrentAccount.ID)
                .Select(x => x.ContactID)
                .Distinct()
                .ToList();

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                if (contactIDs.Any())
                {
                    contactIDs = daoFactory.ContactDao
                        .GetContacts(contactIDs.ToArray())
                        .Select(x => x.ID)
                        .ToList();

                    result = result.Where(x => x.ContactID == 0 || contactIDs.Contains(x.ContactID) || x.ResponsibleID == SecurityContext.CurrentAccount.ID).ToList();

                    if (!result.Any()) return Enumerable.Empty<Task>();
                }

                var casesIds = result.Where(x => x.EntityType == EntityType.Case && x.ResponsibleID != SecurityContext.CurrentAccount.ID)
                        .Select(x => x.EntityID)
                        .Distinct()
                        .ToList();

                if (casesIds.Any())
                {
                    casesIds = daoFactory.CasesDao
                        .GetCases(casesIds.ToArray())
                        .Select(x => x.ID)
                        .ToList();

                    result = result.Where(x => x.EntityID == 0 || casesIds.Contains(x.EntityID) || x.ResponsibleID == SecurityContext.CurrentAccount.ID).ToList();

                    if (!result.Any()) return Enumerable.Empty<Task>();
                }

                var dealsIds = result.Where(x => x.EntityType == EntityType.Opportunity && x.ResponsibleID != SecurityContext.CurrentAccount.ID)
                        .Select(x => x.EntityID)
                        .Distinct()
                        .ToList();

                if (dealsIds.Any())
                {
                    dealsIds = daoFactory.DealDao
                        .GetDeals(dealsIds.ToArray())
                        .Select(x => x.ID)
                        .ToList();

                    result = result
                        .Where(x => x.EntityID == 0 || dealsIds.Contains(x.EntityID) || x.ResponsibleID == SecurityContext.CurrentAccount.ID)
                        .ToList();

                    if (!result.Any()) return Enumerable.Empty<Task>();
                }

                return result;
            }

        }

        public static IEnumerable<Invoice> FilterRead(IEnumerable<Invoice> invoices)
        {
            if (invoices == null || !invoices.Any()) return new List<Invoice>();

            if (IsAdmin) return invoices;

            var result = invoices.ToList();
            var contactIDs = result.Select(x => x.ContactID).Distinct().ToList();

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                if (contactIDs.Any())
                {
                    contactIDs = daoFactory.ContactDao
                        .GetContacts(contactIDs.ToArray())
                        .Select(x => x.ID)
                        .ToList();

                    result = result.Where(x => x.ContactID == 0 || contactIDs.Contains(x.ContactID)).ToList();

                    if (!result.Any()) return Enumerable.Empty<Invoice>();
                }
            }

            return result;
        }

        public static bool CanGoToFeed(Task task)
        {
            return IsAdmin || task.ResponsibleID == SecurityContext.CurrentAccount.ID || task.CreateBy == SecurityContext.CurrentAccount.ID;
        }


    }
}