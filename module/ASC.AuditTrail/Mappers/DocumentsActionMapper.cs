/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Collections.Generic;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    internal class DocumentsActionMapper
    {
        public static Dictionary<MessageAction, MessageMaps> GetMaps()
        {
            return new Dictionary<MessageAction, MessageMaps>
                {
                    {
                        MessageAction.FileCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "FileCreated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileRenamed, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FileRenamed",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FileUpdated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },                    
                    {
                        MessageAction.UserFileUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserFileUpdated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileCreatedVersion, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "FileCreatedVersion",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileDeletedVersion, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "FileDeletedVersion",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileUpdatedRevisionComment, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FileUpdatedRevisionComment",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileLocked, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FileLocked",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileUnlocked, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FileUnlocked",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileUpdatedAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "FileUpdatedAccess",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileDownloaded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DownloadActionType",
                                ActionTextResourceName = "FileDownloaded",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileDownloadedAs, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DownloadActionType",
                                ActionTextResourceName = "FileDownloadedAs",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileUploaded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UploadActionType",
                                ActionTextResourceName = "FileUploaded",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileImported, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "FileImported",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileCopied, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CopyActionType",
                                ActionTextResourceName = "FileCopied",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileCopiedWithOverwriting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CopyActionType",
                                ActionTextResourceName = "FileCopiedWithOverwriting",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileMoved, new MessageMaps
                            {
                                ActionTypeTextResourceName = "MoveActionType",
                                ActionTextResourceName = "FileMoved",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileMovedWithOverwriting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "MoveActionType",
                                ActionTextResourceName = "FileMovedWithOverwriting",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileMovedToTrash, new MessageMaps
                            {
                                ActionTypeTextResourceName = "MoveActionType",
                                ActionTextResourceName = "FileMovedToTrash",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FileDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "FileDeleted",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                    {
                        MessageAction.FolderCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "FolderCreated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderRenamed, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FolderRenamed",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderUpdatedAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "FolderUpdatedAccess",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderCopied, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CopyActionType",
                                ActionTextResourceName = "FolderCopied",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderCopiedWithOverwriting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CopyActionType",
                                ActionTextResourceName = "FolderCopiedWithOverwriting",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderMoved, new MessageMaps
                            {
                                ActionTypeTextResourceName = "MoveActionType",
                                ActionTextResourceName = "FolderMoved",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderMovedWithOverwriting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "MoveActionType",
                                ActionTextResourceName = "FolderMovedWithOverwriting",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderMovedToTrash, new MessageMaps
                            {
                                ActionTypeTextResourceName = "MoveActionType",
                                ActionTextResourceName = "FolderMovedToTrash",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.FolderDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "FolderDeleted",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FoldersModule"
                            }
                    },
                    {
                        MessageAction.ThirdPartyCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ThirdPartyCreated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "DocumentsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ThirdPartyUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ThirdPartyUpdated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "DocumentsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ThirdPartyDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ThirdPartyDeleted",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "DocumentsSettingsModule"
                            }
                    },
                    {
                        MessageAction.DocumentsThirdPartySettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DocumentsThirdPartySettingsUpdated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "DocumentsSettingsModule"
                            }
                    },
                    {
                        MessageAction.DocumentsOverwritingSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DocumentsOverwritingSettingsUpdated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "DocumentsSettingsModule"
                            }
                    },
                    {
                        MessageAction.DocumentsUploadingFormatsSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DocumentsUploadingFormatsSettingsUpdated",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "DocumentsSettingsModule"
                            }
                    },
                    {
                        MessageAction.FileConverted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "FileConverted",
                                ProductResourceName = "DocumentsProduct",
                                ModuleResourceName = "FilesModule"
                            }
                    },
                };
        }
    }
}