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


namespace ASC.Xmpp.Core.protocol.x.muc
{
    /*
    Example 45. Invitee Declines Invitation

    <message
        from='hecate@shakespeare.lit/broom'
        to='darkcave@macbeth.shakespeare.lit'>
      <x xmlns='http://jabber.org/protocol/muc#user'>
        <decline to='crone1@shakespeare.lit'>
          <reason>
            Sorry, I'm too busy right now.
          </reason>
        </decline>
      </x>
    </message>
        

    Example 46. Room Informs Invitor that Invitation Was Declined

    <message
        from='darkcave@macbeth.shakespeare.lit'
        to='crone1@shakespeare.lit/desktop'>
      <x xmlns='http://jabber.org/protocol/muc#user'>
        <decline from='hecate@shakespeare.lit'>
          <reason>
            Sorry, I'm too busy right now.
          </reason>
        </decline>
      </x>
    </message>
    */

    /// <summary>
    /// </summary>
    public class Decline : Invitation
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Decline()
        {
            TagName = "decline";
        }

        /// <summary>
        /// </summary>
        /// <param name="reason"> </param>
        public Decline(string reason) : this()
        {
            Reason = reason;
        }

        /// <summary>
        /// </summary>
        /// <param name="to"> </param>
        public Decline(Jid to) : this()
        {
            To = to;
        }

        /// <summary>
        /// </summary>
        /// <param name="to"> </param>
        /// <param name="reason"> </param>
        public Decline(Jid to, string reason) : this()
        {
            To = to;
            Reason = reason;
        }

        #endregion
    }
}