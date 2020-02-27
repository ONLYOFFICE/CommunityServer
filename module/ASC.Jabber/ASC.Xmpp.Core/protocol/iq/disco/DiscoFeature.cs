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


using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.disco
{
    /// <summary>
    ///   Disco feature
    /// </summary>
    /// <remarks>
    ///   see: http://www.jabber.org/registrar/disco-features.html
    /// </remarks>
    public class DiscoFeature : Element
    {
        #region << Constructors >>

        public DiscoFeature()
        {
            TagName = "feature";
            Namespace = Uri.DISCO_INFO;
        }

        public DiscoFeature(string var) : this()
        {
            Var = var;
        }

        #endregion

        /*
        <iq type='result'
            from='plays.shakespeare.lit'
            to='romeo@montague.net/orchard'
            id='info1'>
        <query xmlns='http://jabber.org/protocol/disco#info'>
            <identity
                category='conference'
                type='text'
                name='Play-Specific Chatrooms'/>
            <identity
                category='directory'
                type='chatroom'
                name='Play-Specific Chatrooms'/>
            <feature var='http://jabber.org/protocol/disco#info'/>
            <feature var='http://jabber.org/protocol/disco#items'/>
            <feature var='http://jabber.org/protocol/muc'/>
            <feature var='jabber:iq:register'/>
            <feature var='jabber:iq:search'/>
            <feature var='jabber:iq:time'/>
            <feature var='jabber:iq:version'/>
        </query>
        </iq>
        */

        /// <summary>
        ///   feature name or namespace
        /// </summary>
        public string Var
        {
            get { return GetAttribute("var"); }
            set { SetAttribute("var", value); }
        }
    }
}