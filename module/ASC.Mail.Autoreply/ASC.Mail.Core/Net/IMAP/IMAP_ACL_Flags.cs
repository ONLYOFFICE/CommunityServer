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


namespace ASC.Mail.Net.IMAP
{
    /// <summary>
    /// IMAP ACL(access control list) rights.
    /// </summary>
    public enum IMAP_ACL_Flags
    {
        /// <summary>
        /// No permissions at all.
        /// </summary>
        None = 0,
        /// <summary>
        /// Lookup (mailbox is visible to LIST/LSUB commands).
        /// </summary>
        l = 1,
        /// <summary>
        /// Read (SELECT the mailbox, perform CHECK, FETCH, PARTIAL,SEARCH, COPY from mailbox).
        /// </summary>
        r = 2,
        /// <summary>
        /// Keep seen/unseen information across sessions (STORE SEEN flag).
        /// </summary>
        s = 4,
        /// <summary>
        /// Write (STORE flags other than SEEN and DELETED).
        /// </summary>
        w = 8,
        /// <summary>
        /// Insert (perform APPEND, COPY into mailbox).
        /// </summary>
        i = 16,
        /// <summary>
        /// Post (send mail to submission address for mailbox,not enforced by IMAP4 itself).
        /// </summary>
        p = 32,
        /// <summary>
        /// Create (CREATE new sub-mailboxes in any implementation-defined hierarchy).
        /// </summary>
        c = 64,
        /// <summary>
        /// Delete (STORE DELETED flag, perform EXPUNGE).
        /// </summary>
        d = 128,
        /// <summary>
        /// Administer (perform SETACL).
        /// </summary>
        a = 256,
        /// <summary>
        /// All permissions
        /// </summary>
        All = 0xFFFF,
    }
}