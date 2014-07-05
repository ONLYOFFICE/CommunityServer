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

namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System.Collections;

    #endregion

    /// <summary>
    /// IMAP folders collection.
    /// </summary>
    public class IMAP_Folders
    {
        #region Members

        private readonly string m_Mailbox = "";
        private readonly ArrayList m_Mailboxes;
        private readonly IMAP_Session m_pSession;
        private readonly string m_RefName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets current IMAP session.
        /// </summary>
        public IMAP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gest list of IMAP folders.
        /// </summary>
        public IMAP_Folder[] Folders
        {
            get
            {
                IMAP_Folder[] retVal = new IMAP_Folder[m_Mailboxes.Count];
                m_Mailboxes.CopyTo(retVal);
                return retVal;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner IMAP session.</param>
        /// <param name="referenceName">Folder Path. Eg. Inbox\.</param>
        /// <param name="folder">Folder name.</param>
        public IMAP_Folders(IMAP_Session session, string referenceName, string folder)
        {
            m_pSession = session;
            m_Mailboxes = new ArrayList();
            m_RefName = referenceName;
            m_Mailbox = folder.Replace("\\", "/");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds folder to folders list.
        /// </summary>
        /// <param name="folder">Full path to folder, path separator = '/'. Eg. Inbox/myFolder .</param>
        /// <param name="selectable">Gets or sets if folder is selectable(SELECT command can select this folder).</param>
        public void Add(string folder, bool selectable)
        {
            folder = folder.Replace("\\", "/");

            string folderPattern = m_RefName + m_Mailbox;
            if (m_RefName != "" && !m_RefName.EndsWith("/") && !m_Mailbox.StartsWith("/"))
            {
                folderPattern = m_RefName + "/" + m_Mailbox;
            }

            if (FolderMatches(folderPattern, Core.Decode_IMAP_UTF7_String(folder)))
            {
                m_Mailboxes.Add(new IMAP_Folder(folder, selectable));
            }
        }

        // TODO: move to some global utility method

        /// <summary>
        /// Checks if specified text matches to specified asteric pattern.
        /// </summary>
        /// <param name="pattern">Asteric pattern. Foe example: *xxx,*xxx*,xx*aa*xx, ... .</param>
        /// <param name="text">Text to match.</param>
        /// <returns></returns>
        public bool AstericMatch(string pattern, string text)
        {
            pattern = pattern.ToLower();
            text = text.ToLower();

            if (pattern == "")
            {
                pattern = "*";
            }

            while (pattern.Length > 0)
            {
                // *xxx[*xxx...]
                if (pattern.StartsWith("*"))
                {
                    // *xxx*xxx
                    if (pattern.IndexOf("*", 1) > -1)
                    {
                        string indexOfPart = pattern.Substring(1, pattern.IndexOf("*", 1) - 1);
                        if (text.IndexOf(indexOfPart) == -1)
                        {
                            return false;
                        }

                        text = text.Substring(text.IndexOf(indexOfPart) + indexOfPart.Length + 1);
                        pattern = pattern.Substring(pattern.IndexOf("*", 1) + 1);
                    }
                        // *xxx   This is last pattern	
                    else
                    {
                        return text.EndsWith(pattern.Substring(1));
                    }
                }
                    // xxx*[xxx...]
                else if (pattern.IndexOfAny(new[] {'*'}) > -1)
                {
                    string startPart = pattern.Substring(0, pattern.IndexOfAny(new[] {'*'}));

                    // Text must startwith
                    if (!text.StartsWith(startPart))
                    {
                        return false;
                    }

                    text = text.Substring(text.IndexOf(startPart) + startPart.Length);
                    pattern = pattern.Substring(pattern.IndexOfAny(new[] {'*'}));
                }
                    // xxx
                else
                {
                    return text == pattern;
                }
            }

            return true;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Gets if folder matches to specified folder pattern.
        /// </summary>
        /// <param name="folderPattern">Folder pattern. * and % between path separators have same meaning (asteric pattern). 
        /// If % is at the end, then matches only last folder child folders and not child folder child folders.</param>
        /// <param name="folder">Folder name with full path.</param>
        /// <returns></returns>
        private bool FolderMatches(string folderPattern, string folder)
        {
            folderPattern = folderPattern.ToLower();
            folder = folder.ToLower();

            string[] folderParts = folder.Split('/');
            string[] patternParts = folderPattern.Split('/');

            // pattern is more nested than folder
            if (folderParts.Length < patternParts.Length)
            {
                return false;
            }
                // This can happen only if * at end
            else if (folderParts.Length > patternParts.Length && !folderPattern.EndsWith("*"))
            {
                return false;
            }
            else
            {
                // Loop patterns
                for (int i = 0; i < patternParts.Length; i++)
                {
                    string patternPart = patternParts[i].Replace("%", "*");

                    // This is asteric pattern
                    if (patternPart.IndexOf('*') > -1)
                    {
                        if (!AstericMatch(patternPart, folderParts[i]))
                        {
                            return false;
                        }
                        // else process next pattern
                    }
                        // No *, this must be exact match
                    else
                    {
                        if (folderParts[i] != patternPart)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion
    }
}