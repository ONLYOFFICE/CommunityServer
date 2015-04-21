/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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