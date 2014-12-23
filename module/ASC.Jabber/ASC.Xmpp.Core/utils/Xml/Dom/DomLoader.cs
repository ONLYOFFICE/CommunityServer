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

using System.IO;
using System.Text;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.Dom
{

    #region usings

    #endregion

    /// <summary>
    ///   internal class that loads a xml document from a string or stream
    /// </summary>
    public class DomLoader
    {
        #region Members

        /// <summary>
        /// </summary>
        private readonly Document doc;

        /// <summary>
        /// </summary>
        private readonly StreamParser sp;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        /// <param name="xml"> </param>
        /// <param name="d"> </param>
        public DomLoader(string xml, Document d)
        {
            doc = d;
            sp = new StreamParser();

            sp.OnStreamStart += sp_OnStreamStart;
            sp.OnStreamElement += sp_OnStreamElement;
            sp.OnStreamEnd += sp_OnStreamEnd;

            byte[] b = Encoding.UTF8.GetBytes(xml);
            sp.Push(b, 0, b.Length);
        }

        /// <summary>
        /// </summary>
        /// <param name="sr"> </param>
        /// <param name="d"> </param>
        public DomLoader(StreamReader sr, Document d) : this(sr.ReadToEnd(), d)
        {
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void sp_OnStreamStart(object sender, Node e, string streamNamespace)
        {
            doc.ChildNodes.Add(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void sp_OnStreamElement(object sender, Node e)
        {
            doc.RootElement.ChildNodes.Add(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void sp_OnStreamEnd(object sender, Node e)
        {
        }

        #endregion

        // ya, the Streamparser is only usable for parsing xmpp stream.
        // it also does a very good job here
    }
}