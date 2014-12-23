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

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.bookmarks
{
    /// <summary>
    /// </summary>
    public class Storage : Element
    {
        /*
            <iq type='result' id='2'>
              <query xmlns='jabber:iq:private'>
                <storage xmlns='storage:bookmarks'>
                  <conference name='Council of Oberon' 
                              autojoin='true'
                              jid='council@conference.underhill.org'>
                    <nick>Puck</nick>
                    <password>titania</password>
                  </conference>
                </storage>
              </query>
            </iq>   
        */

        public Storage()
        {
            TagName = "storage";
            Namespace = Uri.STORAGE_BOOKMARKS;
        }

        /// <summary>
        ///   Add a conference bookmark to the storage object
        /// </summary>
        /// <param name="conf"> </param>
        /// <returns> </returns>
        public Conference AddConference(Conference conf)
        {
            AddChild(conf);
            return conf;
        }

        /// <summary>
        ///   Add a conference bookmark to the storage object
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public Conference AddConference(Jid jid, string name)
        {
            return AddConference(new Conference(jid, name));
        }

        /// <summary>
        ///   Add a conference bookmark to the storage object
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="name"> </param>
        /// <param name="nickname"> </param>
        /// <returns> </returns>
        public Conference AddConference(Jid jid, string name, string nickname)
        {
            return AddConference(new Conference(jid, name, nickname));
        }

        /// <summary>
        ///   Add a conference bookmark to the storage object
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="name"> </param>
        /// <param name="nickname"> </param>
        /// <param name="password"> </param>
        /// <returns> </returns>
        public Conference AddConference(Jid jid, string name, string nickname, string password)
        {
            return AddConference(new Conference(jid, name, nickname, password));
        }

        /// <summary>
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="name"> </param>
        /// <param name="nickname"> </param>
        /// <param name="password"> </param>
        /// <param name="autojoin"> </param>
        /// <returns> </returns>
        public Conference AddConference(Jid jid, string name, string nickname, string password, bool autojoin)
        {
            return AddConference(new Conference(jid, name, nickname, password, autojoin));
        }

        /// <summary>
        ///   add multiple conference bookmarks
        /// </summary>
        /// <param name="confs"> </param>
        public void AddConferences(Conference[] confs)
        {
            foreach (Conference conf in confs)
            {
                AddConference(conf);
            }
        }

        /// <summary>
        ///   get all conference booksmarks
        /// </summary>
        /// <returns> </returns>
        public Conference[] GetConferences()
        {
            ElementList nl = SelectElements(typeof (Conference));
            var items = new Conference[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Conference) e;
                i++;
            }
            return items;
        }

        /// <summary>
        ///   add a url bookmark
        /// </summary>
        /// <param name="url"> </param>
        /// <returns> </returns>
        public Url AddUrl(Url url)
        {
            AddChild(url);
            return url;
        }

        public Url AddUrl(string address, string name)
        {
            return AddUrl(new Url(address, name));
        }

        /// <summary>
        ///   add multiple url bookmarks
        /// </summary>
        /// <param name="urls"> </param>
        public void AddUrls(Url[] urls)
        {
            foreach (Url url in urls)
            {
                AddUrl(url);
            }
        }

        /// <summary>
        ///   Get all url bookmarks
        /// </summary>
        /// <returns> </returns>
        public Url[] GetUrls()
        {
            ElementList nl = SelectElements(typeof (Url));
            var items = new Url[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Url) e;
                i++;
            }
            return items;
        }
    }
}