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


#region using

using System.Collections;
using System.Text;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.xpnet
{

    #region usings

    #endregion

    /// <summary>
    ///   Namespace stack.
    /// </summary>
    public class NS
    {
        #region Members

        /// <summary>
        /// </summary>
        private readonly Stack m_stack = new Stack();

        #endregion

        #region Constructor

        /// <summary>
        ///   Create a new stack, primed with xmlns and xml as prefixes.
        /// </summary>
        public NS()
        {
            PushScope();
            AddNamespace("xmlns", "http://www.w3.org/2000/xmlns/");
            AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
        }

        #endregion

        #region Properties

        /// <summary>
        ///   The current default namespace.
        /// </summary>
        public string DefaultNamespace
        {
            get { return LookupNamespace(string.Empty); }
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Declare a new scope, typically at the start of each element
        /// </summary>
        public void PushScope()
        {
            m_stack.Push(new Hashtable());
        }

        /// <summary>
        ///   Pop the current scope off the stack. Typically at the end of each element.
        /// </summary>
        public void PopScope()
        {
            m_stack.Pop();
        }

        /// <summary>
        ///   Add a namespace to the current scope.
        /// </summary>
        /// <param name="prefix"> </param>
        /// <param name="uri"> </param>
        public void AddNamespace(string prefix, string uri)
        {
            ((Hashtable) m_stack.Peek()).Add(prefix, uri);
        }

        /// <summary>
        ///   Lookup a prefix to find a namespace. Searches down the stack, starting at the current scope.
        /// </summary>
        /// <param name="prefix"> </param>
        /// <returns> </returns>
        public string LookupNamespace(string prefix)
        {
            foreach (Hashtable ht in m_stack)
            {
                if ((ht.Count > 0) && ht.ContainsKey(prefix))
                {
                    return (string) ht[prefix];
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///   Debug output only.
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (Hashtable ht in m_stack)
            {
                sb.Append("---\n");
                foreach (string k in ht.Keys)
                {
                    sb.Append(string.Format("{0}={1}\n", k, ht[k]));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// </summary>
        public void Clear()
        {
#if !CF
            m_stack.Clear();
#else
			while (m_stack.Count > 0)
			    m_stack.Pop();
#endif
        }

        #endregion
    }
}