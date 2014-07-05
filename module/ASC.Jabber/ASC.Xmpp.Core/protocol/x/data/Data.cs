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
// // <copyright company="Ascensio System Limited" file="Data.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.x.data
{

    #region usings

    #endregion

    /// <summary>
    ///   Form Types
    /// </summary>
    public enum XDataFormType
    {
        /// <summary>
        ///   The forms-processing entity is asking the forms-submitting entity to complete a form.
        /// </summary>
        form,

        /// <summary>
        ///   The forms-submitting entity is submitting data to the forms-processing entity.
        /// </summary>
        submit,

        /// <summary>
        ///   The forms-submitting entity has cancelled submission of data to the forms-processing entity.
        /// </summary>
        cancel,

        /// <summary>
        ///   The forms-processing entity is returning data (e.g., search results) to the forms-submitting entity, or the data is a generic data set.
        /// </summary>
        result
    }

    /// <summary>
    ///   Summary for Data.
    /// </summary>
    public class Data : FieldContainer
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Data()
        {
            TagName = "x";
            Namespace = Uri.X_DATA;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        public Data(XDataFormType type) : this()
        {
            Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public string Instructions
        {
            get { return GetTag("instructions"); }

            set { SetTag("instructions", value); }
        }

        /// <summary>
        /// </summary>
        public Reported Reported
        {
            get { return SelectSingleElement(typeof (Reported)) as Reported; }

            set
            {
                RemoveTag(typeof (Reported));
                AddChild(value);
            }
        }

        /// <summary>
        /// </summary>
        public string Title
        {
            get { return GetTag("title"); }

            set { SetTag("title", value); }
        }

        /// <summary>
        ///   Type of thie XDATA Form.
        /// </summary>
        public XDataFormType Type
        {
            get { return (XDataFormType) GetAttributeEnum("type", typeof (XDataFormType)); }

            set { SetAttribute("type", value.ToString()); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Item AddItem()
        {
            var i = new Item();
            AddChild(i);
            return i;
        }

        /// <summary>
        /// </summary>
        /// <param name="item"> </param>
        /// <returns> </returns>
        public Item AddItem(Item item)
        {
            AddChild(item);
            return item;
        }

        /// <summary>
        ///   Gets a list of all form fields
        /// </summary>
        /// <returns> </returns>
        public Item[] GetItems()
        {
            ElementList nl = SelectElements(typeof (Item));
            var items = new Item[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Item) e;
                i++;
            }

            return items;
        }

        #endregion

        /*
		The base syntax for the 'jabber:x:data' namespace is as follows (a formal description can be found in the XML Schema section below):
		
		<x xmlns='jabber:x:data'
		type='{form-type}'>
		<title/>
		<instructions/>
		<field var='field-name'
				type='{field-type}'
				label='description'>
			<desc/>
			<required/>
			<value>field-value</value>
			<option label='option-label'><value>option-value</value></option>
			<option label='option-label'><value>option-value</value></option>
		</field>
		</x>
		
		*/
    }
}