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


#region using

using System;
using System.Collections;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.Dom
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class NodeList : CollectionBase
    {
        #region Members

        /// <summary>
        ///   Owner (Parent) of the ChildElement Collection
        /// </summary>
        private readonly Node m_Owner;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        public NodeList()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="owner"> </param>
        public NodeList(Node owner)
        {
            m_Owner = owner;
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        public void Add(Node e)
        {
            // can't add a empty node, so return immediately
            // Some people tried this which caused an error
            if (e == null)
            {
                return;
            }

            if (m_Owner != null)
            {
                e.Parent = m_Owner;
                if (e.Namespace == null)
                {
                    e.Namespace = m_Owner.Namespace;
                }
            }

            e.m_Index = Count;

            List.Add(e);
        }

        // Method implementation from the CollectionBase class
        /// <summary>
        /// </summary>
        /// <param name="index"> </param>
        /// <exception cref="Exception"></exception>
        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
                // Handle the error that occurs if the valid page index is
                // not supplied. 
                // This exception will be written to the calling function
                throw new Exception("Index out of bounds");
            }

            List.RemoveAt(index);
            RebuildIndex(index);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        public void Remove(Element e)
        {
            int idx = e.Index;
            List.Remove(e);
            RebuildIndex(idx);

            // 			for ( int i = 0; i< this.Count; i++)
            // 			{
            // 				if (e == (Element) this.List[i])
            // 				{
            // 					Remove(i);
            // 					return;
            // 				}
            // 			}
        }

        /// <summary>
        /// </summary>
        /// <param name="index"> </param>
        /// <returns> </returns>
        public Node Item(int index)
        {
            return (Node) List[index];
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public object[] ToArray()
        {
            var ar = new object[List.Count];
            for (int i = 0; i < List.Count; i++)
            {
                ar[i] = List[i];
            }

            return ar;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// </summary>
        internal void RebuildIndex()
        {
            RebuildIndex(0);
        }

        /// <summary>
        /// </summary>
        /// <param name="start"> </param>
        internal void RebuildIndex(int start)
        {
            for (int i = start; i < Count; i++)
            {
                // Element e = (Element) List[i];
                var node = (Node) List[i];
                node.m_Index = i;
            }
        }

        #endregion
    }
}