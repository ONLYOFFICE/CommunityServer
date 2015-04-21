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