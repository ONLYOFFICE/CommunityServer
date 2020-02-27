/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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