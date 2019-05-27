/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
        ApplyAnyFilters
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
