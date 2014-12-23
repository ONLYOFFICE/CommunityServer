/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
    public class ElementList : CollectionBase
    {
        #region Constructor

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        public void Add(Node e)
        {
            // can't add a empty node, so return immediately
            // Some people tried dthis which caused an error
            if (e == null)
            {
                return;
            }

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
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        public void Remove(Element e)
        {
            List.Remove(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="index"> </param>
        /// <returns> </returns>
        public Element Item(int index)
        {
            return (Element) List[index];
        }

        #endregion
    }
}