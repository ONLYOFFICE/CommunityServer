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

#endregion

namespace ASC.Xmpp.Core.utils.Xml.xpnet
{

    #region usings

    #endregion

    /**
 * Represents a position in an entity.
 * A position can be modified by <code>Encoding.movePosition</code>.
 * @see Encoding#movePosition
 * @version $Revision: 1.2 $ $Date: 1998/02/17 04:24:15 $
 */

    /// <summary>
    /// </summary>
    public class Position : ICloneable
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Position()
        {
            LineNumber = 1;
            ColumnNumber = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public int ColumnNumber { get; set; }

        /// <summary>
        /// </summary>
        public int LineNumber { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Clone()
        {
#if CF
	  throw new util.NotImplementedException();
#else
            throw new NotImplementedException();
#endif
        }

        #endregion

        /**
   * Creates a position for the start of an entity: the line number is
   * 1 and the column number is 0.
   */

        /**
   * Returns the line number.
   * The first line number is 1.
   */

        /**
   * Returns the column number.
   * The first column number is 0.
   * A tab character is not treated specially.
   */

        /**
   * Returns a copy of this position.
   */
    }
}