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

namespace ASC.Mail.Net.IMAP.Client
{
    #region usings

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// IMAP namespaces info. Defined in RFC 2342.
    /// </summary>
    public class IMAP_NamespacesInfo
    {
        #region Members

        private readonly IMAP_Namespace[] m_pOtherUsersNamespaces;
        private readonly IMAP_Namespace[] m_pPersonalNamespaces;
        private readonly IMAP_Namespace[] m_pSharedNamespaces;

        #endregion

        #region Properties

        /// <summary>
        /// Gets IMAP server "Personal Namespaces". Returns null if namespace not defined.
        /// </summary>
        public IMAP_Namespace[] PersonalNamespaces
        {
            get { return m_pPersonalNamespaces; }
        }

        /// <summary>
        /// Gets IMAP server "Other Users Namespaces". Returns null if namespace not defined.
        /// </summary>
        public IMAP_Namespace[] OtherUsersNamespaces
        {
            get { return m_pOtherUsersNamespaces; }
        }

        /// <summary>
        /// Gets IMAP server "Shared Namespaces". Returns null if namespace not defined.
        /// </summary>
        public IMAP_Namespace[] SharedNamespaces
        {
            get { return m_pSharedNamespaces; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="personalNamespaces">IMAP server "Personal Namespaces".</param>
        /// <param name="otherUsersNamespaces">IMAP server "Other Users Namespaces".</param>
        /// <param name="sharedNamespaces">IMAP server "Shared Namespaces".</param>
        public IMAP_NamespacesInfo(IMAP_Namespace[] personalNamespaces,
                                   IMAP_Namespace[] otherUsersNamespaces,
                                   IMAP_Namespace[] sharedNamespaces)
        {
            m_pPersonalNamespaces = personalNamespaces;
            m_pOtherUsersNamespaces = otherUsersNamespaces;
            m_pSharedNamespaces = sharedNamespaces;
        }

        #endregion

        #region Utility methods

        private static IMAP_Namespace[] ParseNamespaces(string val)
        {
            StringReader r = new StringReader(val);
            r.ReadToFirstChar();

            List<IMAP_Namespace> namespaces = new List<IMAP_Namespace>();
            while (r.StartsWith("("))
            {
                namespaces.Add(ParseNamespace(r.ReadParenthesized()));
            }

            return namespaces.ToArray();
        }

        private static IMAP_Namespace ParseNamespace(string val)
        {
            string[] parts = TextUtils.SplitQuotedString(val, ' ', true);
            string name = "";
            if (parts.Length > 0)
            {
                name = parts[0];
            }
            string delimiter = "";
            if (parts.Length > 1)
            {
                delimiter = parts[1];
            }

            // Remove delimiter from end, if it's there.
            if (name.EndsWith(delimiter))
            {
                name = name.Substring(0, name.Length - delimiter.Length);
            }

            return new IMAP_Namespace(name, delimiter);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses namespace info from IMAP NAMESPACE response string.
        /// </summary>
        /// <param name="namespaceString">IMAP NAMESPACE response string.</param>
        /// <returns></returns>
        internal static IMAP_NamespacesInfo Parse(string namespaceString)
        {
            StringReader r = new StringReader(namespaceString);
            // Skip NAMESPACE
            r.ReadWord();

            IMAP_Namespace[] personalNamespaces = null;
            IMAP_Namespace[] otherUsersNamespaces = null;
            IMAP_Namespace[] sharedNamespaces = null;

            // Personal namespace
            r.ReadToFirstChar();
            if (r.StartsWith("("))
            {
                personalNamespaces = ParseNamespaces(r.ReadParenthesized());
            }
                // NIL, skip it.
            else
            {
                r.ReadWord();
            }

            // Users Shared namespace
            r.ReadToFirstChar();
            if (r.StartsWith("("))
            {
                otherUsersNamespaces = ParseNamespaces(r.ReadParenthesized());
            }
                // NIL, skip it.
            else
            {
                r.ReadWord();
            }

            // Shared namespace
            r.ReadToFirstChar();
            if (r.StartsWith("("))
            {
                sharedNamespaces = ParseNamespaces(r.ReadParenthesized());
            }
                // NIL, skip it.
            else
            {
                r.ReadWord();
            }

            return new IMAP_NamespacesInfo(personalNamespaces, otherUsersNamespaces, sharedNamespaces);
        }

        #endregion
    }
}