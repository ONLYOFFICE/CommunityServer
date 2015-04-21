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

namespace ASC.Xmpp.Core.protocol.iq.privacy
{
    public class Privacy : Element
    {
        public Privacy()
        {
            TagName = "query";
            Namespace = Uri.IQ_PRIVACY;
        }

        /// <summary>
        ///   The active list
        /// </summary>
        public Active Active
        {
            get { return SelectSingleElement(typeof (Active)) as Active; }
            set
            {
                if (HasTag(typeof (Active)))
                    RemoveTag(typeof (Active));

                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        ///   The default list
        /// </summary>
        public Default Default
        {
            get { return SelectSingleElement(typeof (Default)) as Default; }
            set
            {
                if (HasTag(typeof (Default)))
                    RemoveTag(typeof (Default));

                AddChild(value);
            }
        }

        /// <summary>
        ///   Add a provacy list
        /// </summary>
        /// <param name="list"> </param>
        public void AddList(List list)
        {
            AddChild(list);
        }

        /// <summary>
        ///   Get all Lists
        /// </summary>
        /// <returns> Array of all privacy lists </returns>
        public List[] GetList()
        {
            ElementList el = SelectElements(typeof (List));
            int i = 0;
            var result = new List[el.Count];
            foreach (List list in el)
            {
                result[i] = list;
                i++;
            }
            return result;
        }
    }
}