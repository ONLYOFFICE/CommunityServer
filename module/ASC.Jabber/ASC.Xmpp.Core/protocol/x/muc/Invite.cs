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


using ASC.Xmpp.Core.protocol.extensions.nickname;

namespace ASC.Xmpp.Core.protocol.x.muc
{

    #region usings

    #endregion

    /*
    <message
        from='crone1@shakespeare.lit/desktop'
        to='darkcave@macbeth.shakespeare.lit'>
      <x xmlns='http://jabber.org/protocol/muc#user'>
        <invite to='hecate@shakespeare.lit'>
          <reason>
            Hey Hecate, this is the place for all good witches!
          </reason>
        </invite>
      </x>
    </message>
    */

    /// <summary>
    ///   Invite other users t a chatroom
    /// </summary>
    public class Invite : Invitation
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Invite()
        {
            TagName = "invite";
        }

        /// <summary>
        /// </summary>
        /// <param name="reason"> </param>
        public Invite(string reason) : this()
        {
            Reason = reason;
        }

        /// <summary>
        /// </summary>
        /// <param name="to"> </param>
        public Invite(Jid to) : this()
        {
            To = to;
        }

        /// <summary>
        /// </summary>
        /// <param name="to"> </param>
        /// <param name="reason"> </param>
        public Invite(Jid to, string reason) : this()
        {
            To = to;
            Reason = reason;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public bool Continue
        {
            get { return GetTag("continue") == null ? false : true; }

            set
            {
                if (value)
                {
                    SetTag("continue");
                }
                else
                {
                    RemoveTag("continue");
                }
            }
        }

        /// <summary>
        ///   Nickname Element
        /// </summary>
        public Nickname Nickname
        {
            get { return SelectSingleElement(typeof (Nickname)) as Nickname; }

            set
            {
                if (HasTag(typeof (Nickname)))
                {
                    RemoveTag(typeof (Nickname));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        #endregion

        /*
            <invite to='wiccarocks@shakespeare.lit/laptop'>
                <reason>This coven needs both wiccarocks and hag66.</reason>
                <continue/>
            </invite>
         */
    }
}