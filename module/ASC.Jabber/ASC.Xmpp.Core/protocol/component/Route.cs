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
// // <copyright company="Ascensio System Limited" file="Route.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.component
{
    public enum RouteType
    {
        NONE = -1,
        error,
        auth,
        session
    }

    /// <summary>
    /// </summary>
    public class Route : Stanza
    {
        public Route()
        {
            TagName = "route";
            Namespace = Uri.ACCEPT;
        }

        public Route(Element route) : this()
        {
            RouteElement = route;
        }

        public Route(Element route, Jid from, Jid to) : this()
        {
            RouteElement = route;
            From = from;
            To = to;
        }

        public Route(Element route, Jid from, Jid to, RouteType type) : this()
        {
            RouteElement = route;
            From = from;
            To = to;
            Type = type;
        }

        /// <summary>
        ///   Gets or Sets the logtype
        /// </summary>
        public RouteType Type
        {
            get { return (RouteType) GetAttributeEnum("type", typeof (RouteType)); }
            set
            {
                if (value == RouteType.NONE)
                    RemoveAttribute("type");
                else
                    SetAttribute("type", value.ToString());
            }
        }

        /// <summary>
        ///   sets or gets the element to route
        /// </summary>
        public Element RouteElement
        {
            get { return FirstChild; }
            set
            {
                if (HasChildElements)
                    RemoveAllChildNodes();

                if (value != null)
                    AddChild(value);
            }
        }
    }
}