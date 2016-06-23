/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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