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


namespace ASC.Mail.Core.Engine.Operations.Base
{
    public enum MailOperationType
    {
        RecalculateFolders = 1,
        RemoveMailbox,
        RemoveDomain,
        CheckDomainDns,
        RemoveUserFolder,
        ApplyFilter,
        ApplyAnyFilters,
        DownloadAllAttachments
    }

    public enum MailOperationDownloadAllAttachmentsProgress
    {
        Init = 1,
        GetAttachments = 5,
        Zipping = 10,
        ArchivePreparation = 85,
        CreateLink = 90,
        Finished = 100
    }

    public enum MailOperationRemoveMailboxProgress
    {
        Init = 1,
        RemoveFromDb = 20,
        FreeQuota = 40,
        RecalculateFolder = 50,
        ClearCache = 60,
        RemoveIndex = 70,
        Finished = 100
    }

    public enum MailOperationRecalculateMailboxProgress
    {
        Init = 1,
        CountUnreadMessages = 10,
        CountTotalMessages = 20,
        CountUreadConversation = 30,
        CountTotalConversation = 40,
        UpdateFoldersCounters = 50,
        CountUnreadUserFolderMessages = 60,
        CountTotalUserFolderMessages = 70,
        CountUreadUserFolderConversation = 80,
        CountTotalUserFolderConversation = 90,
        UpdateUserFoldersCounters = 95,
        Finished = 100
    }

    public enum MailOperationRemoveDomainProgress
    {
        Init = 1,
        RemoveFromDb = 20,
        FreeQuota = 40,
        RecalculateFolder = 60,
        ClearCache = 70,
        RemoveIndex = 80,
        Finished = 100
    }

    public enum MailOperationCheckDomainDnsProgress
    {
        Init = 1,
        CheckMx = 20,
        CheckSpf = 40,
        CheckDkim = 60,
        UpdateResults = 80,
        Finished = 100
    }

    public enum MailOperationRemoveUserFolderProgress
    {
        Init = 1,
        MoveMailsToTrash = 25,
        DeleteFolders = 50,
        Finished = 100
    }

    public enum MailOperationApplyFilterProgress
    {
        Init = 1,
        Filtering = 25,
        FilteringAndApplying = 40,
        Finished = 100
    }
}
