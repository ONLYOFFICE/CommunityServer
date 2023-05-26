/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.AuditTrail.Types;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    public class CrmActionMapper : IProductActionMapper
    {
        public List<IModuleActionMapper> Mappers { get; }
        public ProductType Product { get; }
        public CrmActionMapper()
        {
            Product = ProductType.CRM;

            Mappers = new List<IModuleActionMapper>()
            {
                new CompaniesActionMapper(),
                new PersonsActionMapper(),
                new ContactsActionMapper(),
                new CrmTasksActionMapper(),
                new OpportunitiesActionMapper(),
                new InvoicesActionMapper(),
                new CasesActionMapper(),
                new CommonCrmSettingsActionMapper(),
                new ContactsSettingsActionMapper(),
                new ContactTypesActionMapper(),
                new InvoiceSettingsActionMapper(),
                new OtherCrmSettingsActionMapper()
            };
        }
    }

    public class CompaniesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public CompaniesActionMapper()
        {
            Module = ModuleType.Companies;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.Contact, new Dictionary<ActionType, MessageAction[]>()
                    {
                        {
                            ActionType.Create, new []
                            {
                                MessageAction.CompanyCreated, MessageAction.CompanyCreatedWithWebForm, MessageAction.CompanyCreatedTag, MessageAction.CompanyCreatedPersonsTag
                            }
                        },
                        {
                            ActionType.Update, new []
                            {
                                MessageAction.CompanyUpdated, MessageAction.CompanyUpdatedPrincipalInfo, MessageAction.CompanyUpdatedPhoto, MessageAction.CompanyUpdatedTemperatureLevel, MessageAction.CompanyUpdatedPersonsTemperatureLevel
                            }
                        },
                        {
                            ActionType.Delete, new []
                            {
                                MessageAction.CompanyDeleted, MessageAction.CompanyDeletedTag
                            }
                        }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Attach, MessageAction.CompanyAttachedFiles },
                        { ActionType.Detach, MessageAction.CompanyDetachedFile }
                    }
                },
                {
                    EntryType.Relationship, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create,  MessageAction.CompanyCreatedHistoryEvent },
                        { ActionType.Delete,  MessageAction.CompanyDeletedHistoryEvent },
                    }
                },
                {
                    EntryType.Project, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Link,  MessageAction.CompanyLinkedProject },
                        { ActionType.Unlink,  MessageAction.CompanyUnlinkedProject },
                    }
                },
                {
                    EntryType.Contact, EntryType.Contact, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Link,  MessageAction.CompanyLinkedPerson },
                        { ActionType.Unlink,  MessageAction.CompanyUnlinkedPerson },
                        { ActionType.Update,  MessageAction.CompaniesMerged }
                    }
                }
            };
        }
    }

    public class PersonsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public PersonsActionMapper()
        {
            Module = ModuleType.Persons;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.Contact, new Dictionary<ActionType, MessageAction[]>()
                    {
                        {
                            ActionType.Create, new []
                            {
                                MessageAction.PersonCreated, MessageAction.PersonCreatedWithWebForm, MessageAction.PersonsCreated, MessageAction.PersonCreatedTag, MessageAction.PersonCreatedCompanyTag
                            }
                        },
                        {
                            ActionType.Update, new []
                            {
                                MessageAction.CompanyUpdated, MessageAction.PersonUpdated, MessageAction.PersonUpdatedPrincipalInfo, MessageAction.PersonUpdatedPhoto, MessageAction.PersonUpdatedTemperatureLevel, MessageAction.PersonUpdatedCompanyTemperatureLevel
                            }
                        },
                        {
                            ActionType.Delete, new []
                            {
                                MessageAction.PersonDeleted, MessageAction.PersonDeletedTag
                            }
                        },

                    }, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Attach, MessageAction.PersonAttachedFiles },
                        { ActionType.Detach, MessageAction.PersonDetachedFile },
                    }
                },
                {
                    EntryType.Relationship, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create,  MessageAction.PersonCreatedHistoryEvent },
                        { ActionType.Delete,  MessageAction.PersonDeletedHistoryEvent },
                    }
                },
                {
                    EntryType.Project, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Link,  MessageAction.PersonLinkedProject },
                        { ActionType.Unlink,  MessageAction.PersonUnlinkedProject },
                    }
                },
                {
                    EntryType.Contact, EntryType.Contact, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Update,  MessageAction.PersonsMerged }
                    }
                }
            };
        }
    }

    public class ContactsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ContactsActionMapper()
        {
            Module = ModuleType.Contacts;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.Contact, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Delete,  MessageAction.ContactsDeleted },
                        { ActionType.Send,  MessageAction.CrmSmtpMailSent }
                    }
                },
                { MessageAction.ContactsExportedToCsv, ActionType.Export },
                { MessageAction.ContactsImportedFromCSV, ActionType.Import }
            };
        }
    }

    public class CrmTasksActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public CrmTasksActionMapper()
        {
            Module = ModuleType.CrmTasks;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.CRMTask, new Dictionary<ActionType, MessageAction[]>()
                    {
                        {
                            ActionType.Create,  new[]
                            {
                                MessageAction.CrmTaskCreated, MessageAction.ContactsCreatedCrmTasks
                            }
                        },
                        {
                            ActionType.Update,  new[]
                            {
                                MessageAction.CrmTaskUpdated, MessageAction.CrmTaskOpened, MessageAction.CrmTaskClosed
                            }
                        }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Delete, MessageAction.CrmTaskDeleted }
                    }
                },
                { MessageAction.CrmTasksImportedFromCSV, ActionType.Import },
                { MessageAction.CrmTasksExportedToCsv, ActionType.Export }
            };
        }
    }

    public class OpportunitiesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public OpportunitiesActionMapper()
        {
            Module = ModuleType.Opportunities;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.Opportunity, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Create, new[] { MessageAction.OpportunityCreated, MessageAction.OpportunityCreatedTag } },
                        { ActionType.Update, new[] { MessageAction.OpportunityUpdated, MessageAction.OpportunityUpdatedStage } },
                        { ActionType.Delete, new[] { MessageAction.OpportunityDeleted, MessageAction.OpportunitiesDeleted, MessageAction.OpportunityDeletedTag } },
                        { ActionType.Link, new[] { MessageAction.OpportunityLinkedCompany, MessageAction.OpportunityLinkedPerson } },
                        { ActionType.Unlink, new[] { MessageAction.OpportunityUnlinkedCompany, MessageAction.OpportunityUnlinkedPerson } },
                        { ActionType.UpdateAccess, new[] { MessageAction.OpportunityOpenedAccess, MessageAction.OpportunityRestrictedAccess } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Attach, MessageAction.OpportunityAttachedFiles },
                        { ActionType.Detach, MessageAction.OpportunityDetachedFile }
                    }
                },
                {
                    EntryType.Relationship, new Dictionary<ActionType, MessageAction>
                    {
                        { ActionType.Create, MessageAction.OpportunityCreatedHistoryEvent },
                        { ActionType.Delete, MessageAction.OpportunityDeletedHistoryEvent }
                    }
                },
                { MessageAction.OpportunitiesImportedFromCSV, ActionType.Import },
                { MessageAction.OpportunitiesExportedToCsv, ActionType.Export }
            };
        }
    }

    public class InvoicesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public InvoicesActionMapper()
        {
            Module = ModuleType.Invoices;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.Invoice, new Dictionary<ActionType, MessageAction[]>
                    {
                        { ActionType.Update, new [] { MessageAction.InvoiceUpdated, MessageAction.InvoicesUpdatedStatus } },
                        { ActionType.Delete, new [] { MessageAction.InvoiceDeleted, MessageAction.InvoicesDeleted } }
                    },
                    new Dictionary<ActionType, MessageAction>
                    {
                        { ActionType.Create, MessageAction.InvoiceCreated },
                        { ActionType.Download, MessageAction.InvoiceDownloaded }
                    }
                },
                { MessageAction.CurrencyRateUpdated, ActionType.Update },
                { MessageAction.InvoiceDefaultTermsUpdated, ActionType.Update },
            };
        }
    }

    public class CasesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public CasesActionMapper()
        {
            Module = ModuleType.Cases;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.Case, new Dictionary<ActionType, MessageAction[]>
                    {
                        { ActionType.Create, new [] { MessageAction.CaseCreated, MessageAction.CaseCreatedTag } },
                        { ActionType.Update, new [] { MessageAction.CaseUpdated, MessageAction.CaseOpened, MessageAction.CaseClosed } },
                        { ActionType.Delete, new [] { MessageAction.CaseDeleted, MessageAction.CasesDeleted, MessageAction.CaseDeletedTag } },
                        { ActionType.Link, new [] { MessageAction.CaseLinkedCompany, MessageAction.CaseLinkedPerson } },
                        { ActionType.Unlink, new [] { MessageAction.CaseUnlinkedCompany, MessageAction.CaseUnlinkedPerson } },
                        { ActionType.UpdateAccess, new [] { MessageAction.CaseOpenedAccess, MessageAction.CaseRestrictedAccess } },
                    },
                    new Dictionary<ActionType, MessageAction>
                    {
                        { ActionType.Attach, MessageAction.CaseAttachedFiles },
                        { ActionType.Detach, MessageAction.CaseDetachedFile }
                    }
                },
                {
                    EntryType.Relationship, new Dictionary<ActionType, MessageAction>
                    {
                        { ActionType.Create, MessageAction.CaseCreatedHistoryEvent },
                        { ActionType.Delete, MessageAction.CaseDeletedHistoryEvent },
                    }
                },
                { MessageAction.CasesImportedFromCSV, ActionType.Import },
                { MessageAction.CasesExportedToCsv, ActionType.Export },
            };
        }
    }

    public class CommonCrmSettingsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public CommonCrmSettingsActionMapper()
        {
            Module = ModuleType.CommonCrmSettings;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                { MessageAction.CrmSmtpSettingsUpdated, ActionType.Update },
                { MessageAction.CrmTestMailSent, ActionType.Send },
                { MessageAction.CrmDefaultCurrencyUpdated, ActionType.Update },
                { MessageAction.CrmAllDataExported, ActionType.Export }
            };
        }
    }

    public class ContactsSettingsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ContactsSettingsActionMapper()
        {
            Module = ModuleType.ContactsSettings;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.ListItem,
                    new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new[]
                            {
                                MessageAction.ContactTemperatureLevelUpdated, MessageAction.ContactTemperatureLevelUpdatedColor,
                                MessageAction.ContactTemperatureLevelsUpdatedOrder, MessageAction.ContactTemperatureLevelSettingsUpdated
                            }
                        }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.ContactTemperatureLevelCreated },
                        { ActionType.Delete, MessageAction.ContactTemperatureLevelDeleted }
                    }
                }
            };
        }
    }

    public class ContactTypesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ContactTypesActionMapper()
        {
            Module = ModuleType.ContactTypes;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.ListItem, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new [] { MessageAction.ContactTypeUpdated, MessageAction.ContactTypesUpdatedOrder } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.ContactTypeCreated },
                        { ActionType.Delete, MessageAction.ContactTypeDeleted },
                    }
                }
            };
        }
    }

    public class InvoiceSettingsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public InvoiceSettingsActionMapper()
        {
            Module = ModuleType.InvoiceSettings;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.InvoiceItem, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Delete, new [] { MessageAction.InvoiceItemDeleted, MessageAction.InvoiceItemsDeleted } }
                    }, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.InvoiceItemCreated },
                        { ActionType.Update, MessageAction.InvoiceItemUpdated }
                    }
                },
                {
                    EntryType.InvoiceTax, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.InvoiceTaxCreated },
                        { ActionType.Update, MessageAction.InvoiceTaxUpdated },
                        { ActionType.Delete, MessageAction.InvoiceTaxDeleted }
                    }
                },
                {
                    ActionType.Update,
                    new[]
                    {
                        MessageAction.OrganizationProfileUpdatedCompanyName, MessageAction.OrganizationProfileUpdatedInvoiceLogo,
                        MessageAction.OrganizationProfileUpdatedAddress, MessageAction.InvoiceNumberFormatUpdated
                    }
                }
            };
        }
    }

    public class OtherCrmSettingsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public OtherCrmSettingsActionMapper()
        {
            Module = ModuleType.OtherCrmSettings;

            Actions = new MessageMapsDictionary(ProductType.CRM, Module)
            {
                {
                    EntryType.FieldDescription, new Dictionary<ActionType, MessageAction[]>()
                    {
                        {
                            ActionType.Create, new[]
                            {
                                MessageAction.ContactUserFieldCreated, MessageAction.CompanyUserFieldCreated,
                                MessageAction.PersonUserFieldCreated, MessageAction.OpportunityUserFieldCreated,
                                MessageAction.CaseUserFieldCreated
                            }
                        },
                        {
                            ActionType.Update, new[]
                            {
                                MessageAction.ContactUserFieldUpdated, MessageAction.ContactUserFieldsUpdatedOrder,
                                MessageAction.CompanyUserFieldUpdated, MessageAction.CompanyUserFieldsUpdatedOrder,
                                MessageAction.PersonUserFieldUpdated, MessageAction.PersonUserFieldsUpdatedOrder,
                                MessageAction.OpportunityUserFieldUpdated, MessageAction.OpportunityUserFieldsUpdatedOrder,
                                MessageAction.CaseUserFieldUpdated, MessageAction.CaseUserFieldsUpdatedOrder
                            }
                        },
                        {
                            ActionType.Delete, new[]
                            {
                                MessageAction.ContactUserFieldDeleted, MessageAction.CompanyUserFieldDeleted,
                                MessageAction.PersonUserFieldDeleted, MessageAction.OpportunityUserFieldDeleted,
                                MessageAction.CaseUserFieldDeleted
                            }
                        },
                    }
                },
                {
                    EntryType.ListItem, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Create, new[] { MessageAction.HistoryEventCategoryCreated, MessageAction.CrmTaskCategoryCreated } },
                        { ActionType.Update, new[]
                            {
                                MessageAction.HistoryEventCategoryUpdated, MessageAction.HistoryEventCategoryUpdatedIcon, MessageAction.HistoryEventCategoriesUpdatedOrder,
                                MessageAction.CrmTaskCategoryUpdated, MessageAction.CrmTaskCategoryUpdatedIcon, MessageAction.CrmTaskCategoriesUpdatedOrder
                            }
                        },
                        { ActionType.Delete, new[] { MessageAction.HistoryEventCategoryDeleted, MessageAction.CrmTaskCategoryDeleted } }
                    }
                },
                {
                    EntryType.OpportunityMilestone, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new[] { MessageAction.OpportunityStageUpdated, MessageAction.OpportunityStageUpdatedColor, MessageAction.OpportunityStagesUpdatedOrder } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.OpportunityStageCreated },
                        { ActionType.Delete, MessageAction.OpportunityStageDeleted },
                    }
                },
                {
                    ActionType.Create, new[]
                    {
                        MessageAction.ContactsCreatedTag, MessageAction.OpportunitiesCreatedTag, MessageAction.CasesCreatedTag
                    }
                },
                {
                    ActionType.Update, new[]
                    {
                        MessageAction.ContactsTagSettingsUpdated, MessageAction.WebsiteContactFormUpdatedKey
                    }
                },
                {
                    ActionType.Delete, new[]
                    {
                        MessageAction.ContactsDeletedTag, MessageAction.OpportunitiesDeletedTag, MessageAction.CasesDeletedTag
                    }
                },
            };
        }
    }
}