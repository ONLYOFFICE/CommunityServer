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
// // <copyright company="Ascensio System Limited" file="PubSub.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.pubsub
{
    public class PubSub : Element
    {
        public PubSub()
        {
            TagName = "pubsub";
            Namespace = Uri.PUBSUB;
        }

        /// <summary>
        ///   the Create Element of the Pubsub Element
        /// </summary>
        public Create Create
        {
            get { return SelectSingleElement(typeof (Create)) as Create; }
            set
            {
                if (HasTag(typeof (Create)))
                    RemoveTag(typeof (Create));

                if (value != null)
                    AddChild(value);
            }
        }

        public Publish Publish
        {
            get { return SelectSingleElement(typeof (Publish)) as Publish; }
            set
            {
                if (HasTag(typeof (Publish)))
                    RemoveTag(typeof (Publish));

                if (value != null)
                    AddChild(value);
            }
        }

        public Retract Retract
        {
            get { return SelectSingleElement(typeof (Retract)) as Retract; }
            set
            {
                if (HasTag(typeof (Retract)))
                    RemoveTag(typeof (Retract));

                if (value != null)
                    AddChild(value);
            }
        }

        public Subscribe Subscribe
        {
            get { return SelectSingleElement(typeof (Subscribe)) as Subscribe; }
            set
            {
                if (HasTag(typeof (Subscribe)))
                    RemoveTag(typeof (Subscribe));

                if (value != null)
                    AddChild(value);
            }
        }

        public Unsubscribe Unsubscribe
        {
            get { return SelectSingleElement(typeof (Unsubscribe)) as Unsubscribe; }
            set
            {
                if (HasTag(typeof (Unsubscribe)))
                    RemoveTag(typeof (Unsubscribe));

                if (value != null)
                    AddChild(value);
            }
        }

        public Subscriptions Subscriptions
        {
            get { return SelectSingleElement(typeof (Subscriptions)) as Subscriptions; }
            set
            {
                if (HasTag(typeof (Subscriptions)))
                    RemoveTag(typeof (Subscriptions));

                if (value != null)
                    AddChild(value);
            }
        }

        public Affiliations Affiliations
        {
            get { return SelectSingleElement(typeof (Affiliations)) as Affiliations; }
            set
            {
                if (HasTag(typeof (Affiliations)))
                    RemoveTag(typeof (Affiliations));

                if (value != null)
                    AddChild(value);
            }
        }

        public Options Options
        {
            get { return SelectSingleElement(typeof (Options)) as Options; }
            set
            {
                if (HasTag(typeof (Options)))
                    RemoveTag(typeof (Options));

                if (value != null)
                    AddChild(value);
            }
        }

        public Items Items
        {
            get { return SelectSingleElement(typeof (Items)) as Items; }
            set
            {
                if (HasTag(typeof (Items)))
                    RemoveTag(typeof (Items));

                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        ///   The Configure Element of the PunSub Element
        /// </summary>
        public Configure Configure
        {
            get { return SelectSingleElement(typeof (Configure)) as Configure; }
            set
            {
                if (HasTag(typeof (Configure)))
                    RemoveTag(typeof (Configure));

                if (value != null)
                    AddChild(value);
            }
        }
    }
}