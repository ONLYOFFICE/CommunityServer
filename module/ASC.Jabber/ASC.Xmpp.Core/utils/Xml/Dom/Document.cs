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
                new DomLoader(xml, this);
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
                    new DomLoader(sr, this);
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
                new DomLoader(sr, this);
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