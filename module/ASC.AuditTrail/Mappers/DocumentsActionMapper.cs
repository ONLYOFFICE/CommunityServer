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
    public class DocumentsActionMapper : IProductActionMapper
    {
        public List<IModuleActionMapper> Mappers { get; }
        public ProductType Product { get; }
        public DocumentsActionMapper()
        {
            Product = ProductType.Documents;

            Mappers = new List<IModuleActionMapper>()
            {
                new FilesActionMapper(),
                new FoldersActionMapper(),
                new SettingsActionMapper()
            };
        }
    }
    public class FilesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public FilesActionMapper()
        {
            Module = ModuleType.Files;
            Actions = new MessageMapsDictionary(ProductType.Documents, Module)
            {
                {
                    EntryType.File, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Create, new[] { MessageAction.FileCreated, MessageAction.FileCreatedVersion, MessageAction.FileRestoreVersion, MessageAction.FileConverted } },
                        {
                            ActionType.Update, new[]
                            {
                                MessageAction.FileRenamed, MessageAction.FileUpdated, MessageAction.UserFileUpdated, MessageAction.FileUpdatedRevisionComment,
                                MessageAction.FileLocked, MessageAction.FileUnlocked, MessageAction.FileOpenedForChange, MessageAction.FileMarkedAsFavorite,
                                MessageAction.FileRemovedFromFavorite, MessageAction.FileMarkedAsRead, MessageAction.FileReaded
                            }
                        },
                        { ActionType.Delete, new[] { MessageAction.FileDeletedVersion, MessageAction.FileDeleted, MessageAction.TrashEmptied } },
                        { ActionType.UpdateAccess, new[] { MessageAction.FileUpdatedAccess, MessageAction.FileUpdatedAccessFor, MessageAction.FileRemovedFromList, MessageAction.FileExternalLinkAccessUpdated } },
                        { ActionType.Download, new[] {  MessageAction.FileDownloaded, MessageAction.FileDownloadedAs, MessageAction.FileRevisionDownloaded } },
                        { ActionType.Send, new[] { MessageAction.FileSendAccessLink, MessageAction.FileChangeOwner } },
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Upload, MessageAction.FileUploaded },
                        { ActionType.Import, MessageAction.FileImported },
                        { ActionType.Move, MessageAction.FileMovedToTrash }
                    }
                },
                {
                    EntryType.File, EntryType.Folder, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Copy, new[] { MessageAction.FileCopied, MessageAction.FileCopiedWithOverwriting } },
                        { ActionType.Move, new[] { MessageAction.FileMoved, MessageAction.FileMovedWithOverwriting } },
                    }
                },
            };

            Actions.Add(MessageAction.DocumentSignComplete, new MessageMaps("FilesDocumentSigned", ActionType.Send, ProductType.Documents, Module, EntryType.File));
            Actions.Add(MessageAction.DocumentSendToSign, new MessageMaps("FilesRequestSign", ActionType.Send, ProductType.Documents, Module, EntryType.File));
        }
    }

    public class FoldersActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public FoldersActionMapper()
        {
            Module = ModuleType.Folders;
            Actions = new MessageMapsDictionary(ProductType.Documents, Module)
            {
                {
                    EntryType.Folder, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new[] { MessageAction.FolderRenamed, MessageAction.FolderMarkedAsRead } },
                        { ActionType.UpdateAccess, new[] { MessageAction.FolderUpdatedAccess, MessageAction.FolderUpdatedAccessFor, MessageAction.FolderRemovedFromList } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.FolderCreated },
                        { ActionType.Move, MessageAction.FolderMovedToTrash },
                        { ActionType.Delete, MessageAction.FolderDeleted },
                        { ActionType.Download, MessageAction.FolderDownloaded },
                    }
                },
                {
                    EntryType.Folder, EntryType.Folder, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Copy, new[] { MessageAction.FolderCopied, MessageAction.FolderCopiedWithOverwriting } },
                        { ActionType.Move, new[] { MessageAction.FolderMoved, MessageAction.FolderMovedFrom, MessageAction.FolderMovedWithOverwriting } },
                    }
                },
            };
        }
    }

    public class SettingsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public SettingsActionMapper()
        {
            Module = ModuleType.DocumentsSettings;
            Actions = new MessageMapsDictionary(ProductType.Documents, Module)
            {
                {
                    EntryType.Folder, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create,  MessageAction.ThirdPartyCreated  },
                        { ActionType.Update, MessageAction.ThirdPartyUpdated },
                        { ActionType.Delete, MessageAction.ThirdPartyDeleted },
                    }
                },
                {
                    ActionType.Update, new []
                    {
                        MessageAction.DocumentsThirdPartySettingsUpdated, MessageAction.DocumentsOverwritingSettingsUpdated,
                        MessageAction.DocumentsForcesave, MessageAction.DocumentsStoreForcesave, MessageAction.DocumentsUploadingFormatsSettingsUpdated,
                        MessageAction.DocumentsExternalShareSettingsUpdated
                    }
                },
            };
        }
    }
}