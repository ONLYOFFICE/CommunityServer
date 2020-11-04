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


using System.Collections.Generic;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    internal class CrmActionMapper
    {
        public static Dictionary<MessageAction, MessageMaps> GetMaps()
        {
            return new Dictionary<MessageAction, MessageMaps>
                {
                    #region companies

                    {
                        MessageAction.CompanyCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CompanyCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyCreatedWithWebForm, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CompanyCreatedWithWebForm",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUpdatedPrincipalInfo, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUpdatedPrincipalInfo",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUpdatedPhoto, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUpdatedPhoto",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUpdatedTemperatureLevel, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUpdatedTemperatureLevel",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUpdatedPersonsTemperatureLevel, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUpdatedPersonsTemperatureLevel",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CompanyCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyCreatedPersonsTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CompanyCreatedPersonsTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CompanyDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyCreatedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CompanyCreatedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyDeletedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CompanyDeletedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyLinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "CompanyLinkedPerson",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUnlinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "CompanyUnlinkedPerson",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyLinkedProject, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "CompanyLinkedProject",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyUnlinkedProject, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "CompanyUnlinkedProject",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyAttachedFiles, new MessageMaps
                            {
                                ActionTypeTextResourceName = "AttachActionType",
                                ActionTextResourceName = "CompanyAttachedFiles",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyDetachedFile, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DetachActionType",
                                ActionTextResourceName = "CompanyDetachedFile",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompaniesMerged, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompaniesMerged",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },
                    {
                        MessageAction.CompanyDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CompanyDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CompaniesModule"
                            }
                    },

                    #endregion

                    #region persons
                
                    {
                        MessageAction.PersonCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonCreatedWithWebForm, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonCreatedWithWebForm",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonsCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonsCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonUpdatedPrincipalInfo, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUpdatedPrincipalInfo",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonUpdatedPhoto, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUpdatedPhoto",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonUpdatedTemperatureLevel, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUpdatedTemperatureLevel",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonUpdatedCompanyTemperatureLevel, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUpdatedCompanyTemperatureLevel",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonCreatedCompanyTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonCreatedCompanyTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "PersonDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonCreatedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonCreatedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonDeletedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "PersonDeletedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonLinkedProject, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "PersonLinkedProject",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonUnlinkedProject, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "PersonUnlinkedProject",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonAttachedFiles, new MessageMaps
                            {
                                ActionTypeTextResourceName = "AttachActionType",
                                ActionTextResourceName = "PersonAttachedFiles",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonDetachedFile, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DetachActionType",
                                ActionTextResourceName = "PersonDetachedFile",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonsMerged, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonsMerged",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },
                    {
                        MessageAction.PersonDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "PersonDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "PersonsModule"
                            }
                    },

                    #endregion

                    #region contacts
                    
                    {
                        MessageAction.ContactsDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ContactsDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsModule"
                            }
                    },
                    {
                        MessageAction.CrmSmtpMailSent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "CrmSmtpMailSent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsModule"
                            }
                    },

                    #endregion

                    #region tasks

                    {
                        MessageAction.CrmTaskCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CrmTaskCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.ContactsCreatedCrmTasks, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ContactsCreatedCrmTasks",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmTaskUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskOpened, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmTaskOpened",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskClosed, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmTaskClosed",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CrmTaskDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },

                    #endregion

                    #region opportunities

                    {
                        MessageAction.OpportunityCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "OpportunityCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUpdatedStage, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityUpdatedStage",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "OpportunityCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunityDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityCreatedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "OpportunityCreatedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityDeletedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunityDeletedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityLinkedCompany, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "OpportunityLinkedCompany",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUnlinkedCompany, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "OpportunityUnlinkedCompany",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityLinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "OpportunityLinkedPerson",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUnlinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "OpportunityUnlinkedPerson",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityAttachedFiles, new MessageMaps
                            {
                                ActionTypeTextResourceName = "AttachActionType",
                                ActionTextResourceName = "OpportunityAttachedFiles",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityDetachedFile, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DetachActionType",
                                ActionTextResourceName = "OpportunityDetachedFile",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityOpenedAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "OpportunityOpenedAccess",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityRestrictedAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "OpportunityRestrictedAccess",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunityDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunityDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.OpportunitiesDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunitiesDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },

                    #endregion

                    #region invoices

                    {
                        MessageAction.InvoiceCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "InvoiceCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },
                    {
                        MessageAction.InvoiceUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "InvoiceUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },
                    {
                        MessageAction.InvoicesUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "InvoicesUpdatedStatus",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },
                    {
                        MessageAction.InvoiceDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "InvoiceDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },
                    {
                        MessageAction.InvoicesDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "InvoicesDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },

                    {
                        MessageAction.CurrencyRateUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CurrencyRateUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },
                    {
                        MessageAction.InvoiceDefaultTermsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "InvoiceDefaultTermsUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },
                    {
                        MessageAction.InvoiceDownloaded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DownloadActionType",
                                ActionTextResourceName = "InvoiceDownloaded",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoicesModule"
                            }
                    },

                    #endregion

                    #region cases

                    {
                        MessageAction.CaseCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CaseCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CaseUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseOpened, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CaseOpened",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseClosed, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CaseClosed",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CaseCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CaseDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseCreatedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CaseCreatedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseDeletedHistoryEvent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CaseDeletedHistoryEvent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseLinkedCompany, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "CaseLinkedCompany",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseUnlinkedCompany, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "CaseUnlinkedCompany",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseLinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "CaseLinkedPerson",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseUnlinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "CaseUnlinkedPerson",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseAttachedFiles, new MessageMaps
                            {
                                ActionTypeTextResourceName = "AttachActionType",
                                ActionTextResourceName = "CaseAttachedFiles",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseDetachedFile, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DetachActionType",
                                ActionTextResourceName = "CaseDetachedFile",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseOpenedAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "CaseOpenedAccess",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseRestrictedAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "CaseRestrictedAccess",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CaseDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CaseDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },
                    {
                        MessageAction.CasesDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CasesDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },

                    #endregion

                    #region common settings

                    {
                        MessageAction.CrmSmtpSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmSmtpSettingsUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CommonCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmTestMailSent, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "CrmTestMailSent",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CommonCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmDefaultCurrencyUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmDefaultCurrencyUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CommonCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmAllDataExported, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ExportActionType",
                                ActionTextResourceName = "CrmAllDataExported",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CommonCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region contact settings

                    {
                        MessageAction.ContactTemperatureLevelCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ContactTemperatureLevelCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactTemperatureLevelUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactTemperatureLevelUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactTemperatureLevelUpdatedColor, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactTemperatureLevelUpdatedColor",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactTemperatureLevelsUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactTemperatureLevelsUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactTemperatureLevelDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ContactTemperatureLevelDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactTemperatureLevelSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactTemperatureLevelSettingsUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CommonCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactTypeCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ContactTypeCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactTypesModule"
                            }
                    },
                    {
                        MessageAction.ContactTypeUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactTypeUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactTypesModule"
                            }
                    },
                    {
                        MessageAction.ContactTypesUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactTypesUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactTypesModule"
                            }
                    },
                    {
                        MessageAction.ContactTypeDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ContactTypeDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactTypesModule"
                            }
                    },

                    #endregion

                    #region invoice settings

                    {
                        MessageAction.InvoiceItemCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "InvoiceItemCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceItemUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "InvoiceItemUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceItemDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "InvoiceItemDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceItemsDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "InvoiceItemsDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceTaxCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "InvoiceTaxCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceTaxUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "InvoiceTaxUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceTaxDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "InvoiceTaxDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.OrganizationProfileUpdatedCompanyName, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OrganizationProfileUpdatedCompanyName",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.OrganizationProfileUpdatedInvoiceLogo, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OrganizationProfileUpdatedInvoiceLogo",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.OrganizationProfileUpdatedAddress, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OrganizationProfileUpdatedAddress",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },
                    {
                        MessageAction.InvoiceNumberFormatUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "InvoiceNumberFormatUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "InvoiceSettingsModule"
                            }
                    },

                    #endregion

                    #region user fields settings

                    {
                        MessageAction.ContactUserFieldCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ContactUserFieldCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactUserFieldUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactUserFieldUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactUserFieldsUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactUserFieldsUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactUserFieldDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ContactUserFieldDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CompanyUserFieldCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CompanyUserFieldCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CompanyUserFieldUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUserFieldUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CompanyUserFieldsUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CompanyUserFieldsUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CompanyUserFieldDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CompanyUserFieldDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.PersonUserFieldCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "PersonUserFieldCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.PersonUserFieldUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUserFieldUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.PersonUserFieldsUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PersonUserFieldsUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.PersonUserFieldDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "PersonUserFieldDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUserFieldCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "OpportunityUserFieldCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUserFieldUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityUserFieldUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUserFieldsUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityUserFieldsUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityUserFieldDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunityUserFieldDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CaseUserFieldCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CaseUserFieldCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CaseUserFieldUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CaseUserFieldUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CaseUserFieldsUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CaseUserFieldsUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CaseUserFieldDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CaseUserFieldDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region history events categories settings

                    {
                        MessageAction.HistoryEventCategoryCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "HistoryEventCategoryCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.HistoryEventCategoryUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "HistoryEventCategoryUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.HistoryEventCategoryUpdatedIcon, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "HistoryEventCategoryUpdatedIcon",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.HistoryEventCategoriesUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "HistoryEventCategoriesUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.HistoryEventCategoryDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "HistoryEventCategoryDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region task actegories settings

                    {
                        MessageAction.CrmTaskCategoryCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CrmTaskCategoryCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskCategoryUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmTaskCategoryUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskCategoryUpdatedIcon, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmTaskCategoryUpdatedIcon",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskCategoriesUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CrmTaskCategoriesUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CrmTaskCategoryDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CrmTaskCategoryDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region opportunity stages settings

                    {
                        MessageAction.OpportunityStageCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "OpportunityStageCreated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityStageUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityStageUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityStageUpdatedColor, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityStageUpdatedColor",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityStagesUpdatedOrder, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OpportunityStagesUpdatedOrder",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunityStageDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunityStageDeleted",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region tags settings

                    {
                        MessageAction.ContactsCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ContactsCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactsDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ContactsDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunitiesCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "OpportunitiesCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.OpportunitiesDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "OpportunitiesDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CasesCreatedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "CasesCreatedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.CasesDeletedTag, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "CasesDeletedTag",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },
                    {
                        MessageAction.ContactsTagSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ContactsTagSettingsUpdated",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CommonCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region other settings

                    {
                        MessageAction.WebsiteContactFormUpdatedKey, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "WebsiteContactFormUpdatedKey",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OtherCrmSettingsModule"
                            }
                    },

                    #endregion

                    #region import

                    {
                        MessageAction.ContactsImportedFromCSV, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "ContactsImportedFromCSV",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsModule"
                            }
                    },
                    {
                        MessageAction.CrmTasksImportedFromCSV, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "CrmTasksImportedFromCSV",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.OpportunitiesImportedFromCSV, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "OpportunitiesImportedFromCSV",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.CasesImportedFromCSV, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "CasesImportedFromCSV",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },

                    #endregion

                    #region export

                    {
                        MessageAction.ContactsExportedToCsv, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ExportActionType",
                                ActionTextResourceName = "ContactsExportedToCsv",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "ContactsModule"
                            }
                    },
                    {
                        MessageAction.CrmTasksExportedToCsv, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ExportActionType",
                                ActionTextResourceName = "CrmTasksExportedToCsv",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CrmTasksModule"
                            }
                    },
                    {
                        MessageAction.OpportunitiesExportedToCsv, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ExportActionType",
                                ActionTextResourceName = "OpportunitiesExportedToCsv",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "OpportunitiesModule"
                            }
                    },
                    {
                        MessageAction.CasesExportedToCsv, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ExportActionType",
                                ActionTextResourceName = "CasesExportedToCsv",
                                ProductResourceName = "CrmProduct",
                                ModuleResourceName = "CasesModule"
                            }
                    },

                    #endregion
                };
        }
    }
}