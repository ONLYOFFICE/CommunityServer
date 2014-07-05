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