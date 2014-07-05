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

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="Document.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#region using

using System.IO;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.Dom
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class Document : Node
    {
        #region Members

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        public Document()
        {
            NodeType = NodeType.Document;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// </summary>
        public Element RootElement
        {
            get
            {
                foreach (Node n in ChildNodes)
                {
                    if (n.NodeType == NodeType.Element)
                    {
                        return n as Element;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// </summary>
        public string Version { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///   Clears the Document
        /// </summary>
        public void Clear()
        {
            ChildNodes.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="xml"> </param>
        public void LoadXml(string xml)
        {
            if (xml != string.Empty && xml != null)
            {
                var l = new DomLoader(xml, this);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="filename"> </param>
        /// <returns> </returns>
        public bool LoadFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    var sr = new StreamReader(filename);
                    var l = new DomLoader(sr, this);
                    sr.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"> </param>
        /// <returns> </returns>
        public bool LoadStream(Stream stream)
        {
            try
            {
                var sr = new StreamReader(stream);
                var l = new DomLoader(sr, this);
                sr.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="filename"> </param>
        public void Save(string filename)
        {
            var w = new StreamWriter(filename);

            w.Write(ToString(System.Text.Encoding.UTF8));
            w.Flush();
            w.Close();
        }

        #endregion
    }
}