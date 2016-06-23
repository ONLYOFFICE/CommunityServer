/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


#region using

using System;
using System.Collections;
using System.Text;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;

#endregion

namespace ASC.Xmpp.Core.protocol.extensions.caps
{
    /*
        Example 1. Annotated presence sent

        <presence>
          <c xmlns='http://jabber.org/protocol/caps'
             node='http://exodus.jabberstudio.org/caps'
             ver='0.9'/>
        </presence>

        Example 2. Annotated presence sent, with feature extensions

        <presence>
          <c xmlns='http://jabber.org/protocol/caps'
             node='http://exodus.jabberstudio.org/caps'
             ver='0.9'
             ext='jingle ftrans xhtml'/>
        </presence>
        
    */

    /// <summary>
    ///   <para>It is often desirable for a Jabber/XMPP application (commonly but not necessarily a client) to take different actions 
    ///     depending on the capabilities of another application from which it receives presence information. Examples include:</para> <list
    ///    type="bullet">
    ///                                                                                                                                  <item>
    ///                                                                                                                                    <term>Showing a different set of icons depending on the capabilities of other clients.</term>
    ///                                                                                                                                  </item>
    ///                                                                                                                                  <item>
    ///                                                                                                                                    <term>Not sending XHTML-IM content to plaintext clients such as cell phones.</term>
    ///                                                                                                                                  </item>
    ///                                                                                                                                  <item>
    ///                                                                                                                                    <term>Allowing the initiation of Voice over IP (VoIP) sessions only to clients that support VoIP.</term>
    ///                                                                                                                                  </item>
    ///                                                                                                                                  <item>
    ///                                                                                                                                    <term>Not showing a "Send a File" button if another user's client does not support File Transfer.</term>
    ///                                                                                                                                  </item>
    ///                                                                                                                                </list> <para>Recently, some existing Jabber clients have begun sending Software Version requests to each entity from which they 
    ///                                                                                                                                          receive presence. That solution is impractical on a larger scale, particularly for users or applications with large rosters. 
    ///                                                                                                                                          This document proposes a more robust and scalable solution: namely, a presence-based mechanism for exchanging information 
    ///                                                                                                                                          about entity capabilities.</para>
    /// </summary>
    public class Capabilities : Element
    {
        /// <summary>
        /// </summary>
        public Capabilities()
        {
            TagName = "c";
            Namespace = Uri.CAPS;
        }

        /// <summary>
        /// </summary>
        /// <param name="version"> </param>
        /// <param name="node"> </param>
        public Capabilities(string version, string node)
            : this()
        {
            Version = version;
            Node = node;
        }

        /// <summary>
        ///   Required node attribute
        /// </summary>
        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        /// <summary>
        ///   Required version attribute
        /// </summary>
        public string Version
        {
            get { return GetAttribute("ver"); }
            set { SetAttribute("ver", value); }
        }

        [Obsolete("This property is deprecated with version 1.4 of XEP-0115. You shouldn't use this propety anymore.")]
        public string[] Extensions
        {
            get { return GetExtensions(); }
            set { SetExtensions(value); }
        }

        /// <summary>
        ///   Builds and sets the caps ver attribute from a DiscoInfo object
        /// </summary>
        /// <param name="di"> </param>
        public void SetVersion(DiscoInfo di)
        {
            Version = BuildCapsVersion(di);
        }

        private string BuildCapsVersion(DiscoInfo di)
        {
            /*
                1.  Initialize an empty string S.
                2. Sort the service discovery identities by category and then by type (if it exists), formatted as 'category' '/' 'type'.
                3. For each identity, append the 'category/type' to S, followed by the '<' character.
                4. Sort the supported features.
                5. For each feature, append the feature to S, followed by the '<' character.
                6. Compute ver by hashing S using the SHA-1 algorithm as specified in RFC 3174 [17] (with binary output) and 
                   encoding the hash using Base64 as specified in Section 4 of RFC 4648 [18] 
                   (note: the Base64 output MUST NOT include whitespace and MUST set padding bits to zero). [19]
             */
            var features = new ArrayList();
            var identities = new ArrayList();

            foreach (DiscoIdentity did in di.GetIdentities())
                identities.Add(did.Type == null ? did.Category : did.Category + "/" + did.Type);

            foreach (DiscoFeature df in di.GetFeatures())
                features.Add(df.Var);

            identities.Sort();
            features.Sort();

            var S = new StringBuilder();

            foreach (string s in identities)
                S.Append(s + "<");

            foreach (string s in features)
                S.Append(s + "<");

            byte[] sha1 = Hash.Sha1HashBytes(S.ToString());

#if CF
            return Convert.ToBase64String(sha1, 0, sha1.Length);
#else
            return Convert.ToBase64String(sha1);
#endif
        }

        #region << Extension Helpers >>

        public void AddExtension(string ext)
        {
            string[] extensions = GetExtensions();
            // check if the extension already exists
            if (extensions != null &&
                Array.IndexOf(extensions, ext, extensions.GetLowerBound(0), extensions.Length) >= 0)
                return;

            int size = extensions == null ? 1 : extensions.Length + 1;
            var tmpExtensions = new string[size];
            if (size > 1)
                extensions.CopyTo(tmpExtensions, 0);

            tmpExtensions[size - 1] = ext;
            SetExtensions(tmpExtensions);
        }

        public void RemoveExtension(string ext)
        {
            string[] extensions = GetExtensions();
            if (extensions != null)
            {
                if (Array.IndexOf(extensions, ext, extensions.GetLowerBound(0), extensions.Length) >= 0)
                {
                    int i = 0;
                    var tmpExtensions = new string[extensions.Length - 1];
                    foreach (string s in extensions)
                    {
                        if (s != ext)
                            tmpExtensions[i++] = s;
                    }
                    SetExtensions(tmpExtensions);
                }
            }
        }

        public bool ContainsExtension(string ext)
        {
            string[] extensions = GetExtensions();
            if (extensions == null)
                return false;

            if (Array.IndexOf(extensions, ext, extensions.GetLowerBound(0), extensions.Length) >= 0)
                return true;
            else
                return false;
        }

        private string[] GetExtensions()
        {
            string ext = GetAttribute("ext");
            return ext != null ? ext.Split(' ') : null;
        }

        private void SetExtensions(string[] ext)
        {
            if (ext != null)
            {
                string temp = null;
                for (int i = 0; i < ext.Length; i++)
                {
                    temp += ext[i];
                    if (i < ext.Length - 1)
                        temp += " ";
                }
                SetAttribute("ext", temp);
            }
        }

        #endregion
    }
}