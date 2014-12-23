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

namespace ASC.Xmpp.Core.protocol.x.muc
{

    #region usings

    #endregion

    /*
     
        <iq from='crone1@shakespeare.lit/desktop'
            id='begone'
            to='heath@macbeth.shakespeare.lit'
            type='set'>
          <query xmlns='http://jabber.org/protocol/muc#owner'>
            <destroy jid='darkcave@macbeth.shakespeare.lit'>
              <reason>Macbeth doth come.</reason>
            </destroy>
          </query>
        </iq>
     
     */

    /// <summary>
    /// </summary>
    public class Destroy : Element
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Destroy()
        {
            TagName = "destroy";
            Namespace = Uri.MUC_OWNER;
        }

        /// <summary>
        /// </summary>
        /// <param name="reason"> </param>
        public Destroy(string reason) : this()
        {
            Reason = reason;
        }

        /// <summary>
        /// </summary>
        /// <param name="altVenue"> </param>
        public Destroy(Jid altVenue) : this()
        {
            AlternateVenue = altVenue;
        }

        /// <summary>
        /// </summary>
        /// <param name="reason"> </param>
        /// <param name="altVenue"> </param>
        public Destroy(string reason, Jid altVenue) : this()
        {
            Reason = reason;
            AlternateVenue = altVenue;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Pptional attribute for a alternate venue
        /// </summary>
        public Jid AlternateVenue
        {
            get
            {
                if (HasAttribute("jid"))
                {
                    return new Jid(GetAttribute("jid"));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    SetAttribute("jid", value.ToString());
                }
            }
        }

        /// <summary>
        /// </summary>
        public string Password
        {
            get { return GetTag("password"); }

            set { SetTag("password", value); }
        }

        /// <summary>
        /// </summary>
        public string Reason
        {
            get { return GetTag("reason"); }

            set { SetTag("reason", value); }
        }

        #endregion
    }
}