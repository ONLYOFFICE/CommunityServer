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
// // <copyright company="Ascensio System Limited" file="Rule.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.amp
{
    public class Rule : Element
    {
        public Rule()
        {
            TagName = "rule";
            Namespace = Uri.AMP;
        }

        public Rule(Condition condition, string val, Action action)
            : this()
        {
            Condition = condition;
            Val = val;
            Action = action;
        }

        /// <summary>
        ///   The 'value' attribute defines how the condition is matched. This attribute MUST be present, and MUST NOT be an empty string (""). The interpretation of this attribute's value is determined by the 'condition' attribute.
        /// </summary>
        public string Val
        {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }

        /// <summary>
        ///   The 'action' attribute defines the result for this rule. This attribute MUST be present, and MUST be either a value defined in the Defined Actions section, or one registered with the XMPP Registrar.
        /// </summary>
        public Action Action
        {
            get { return (Action) GetAttributeEnum("action", typeof (Action)); }
            set
            {
                if (value == Action.Unknown)
                    RemoveAttribute("action");
                else
                    SetAttribute("action", value.ToString());
            }
        }

        /// <summary>
        ///   The 'condition' attribute defines the overall condition this rule applies to. This attribute MUST be present, and MUST be either a value defined in the Defined Conditions section, or one registered with the XMPP Registrar.
        /// </summary>
        public Condition Condition
        {
            get
            {
                switch (GetAttribute("condition"))
                {
                    case "deliver":
                        return Condition.Deliver;
                    case "expire-at":
                        return Condition.ExprireAt;
                    case "match-resource":
                        return Condition.MatchResource;
                    default:
                        return Condition.Unknown;
                }
            }

            set
            {
                switch (value)
                {
                    case Condition.Deliver:
                        SetAttribute("condition", "deliver");
                        break;
                    case Condition.ExprireAt:
                        SetAttribute("condition", "expire-at");
                        break;
                    case Condition.MatchResource:
                        SetAttribute("condition", "match-resource");
                        break;
                }
            }
        }
    }
}