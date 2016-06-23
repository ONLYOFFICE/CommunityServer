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

namespace ASC.Xmpp.Core.protocol.extensions.pubsub.owner
{
    public class PubSub : Element
    {
        public PubSub()
        {
            TagName = "pubsub";
            Namespace = Uri.PUBSUB_OWNER;
        }

        public Delete Delete
        {
            get { return SelectSingleElement(typeof (Delete)) as Delete; }
            set
            {
                if (HasTag(typeof (Delete)))
                    RemoveTag(typeof (Delete));

                if (value != null)
                    AddChild(value);
            }
        }

        public Purge Purge
        {
            get { return SelectSingleElement(typeof (Purge)) as Purge; }
            set
            {
                if (HasTag(typeof (Purge)))
                    RemoveTag(typeof (Purge));

                if (value != null)
                    AddChild(value);
            }
        }

        public Subscribers Subscribers
        {
            get { return SelectSingleElement(typeof (Subscribers)) as Subscribers; }
            set
            {
                if (HasTag(typeof (Subscribers)))
                    RemoveTag(typeof (Subscribers));

                if (value != null)
                    AddChild(value);
            }
        }

        public Affiliates Affiliates
        {
            get { return SelectSingleElement(typeof (Affiliates)) as Affiliates; }
            set
            {
                if (HasTag(typeof (Affiliates)))
                    RemoveTag(typeof (Affiliates));

                if (value != null)
                    AddChild(value);
            }
        }

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