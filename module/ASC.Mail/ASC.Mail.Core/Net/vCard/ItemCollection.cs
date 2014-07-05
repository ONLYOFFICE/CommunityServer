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

namespace ASC.Mail.Net.Mime.vCard
{
    #region usings

    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// vCard item collection.
    /// </summary>
    public class ItemCollection : IEnumerable
    {
        #region Members

        private readonly List<Item> m_pItems;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal ItemCollection()
        {
            m_pItems = new List<Item>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of vCard items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pItems.Count; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds new vCard item to the collection.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="parametes">Item parameters.</param>
        /// <param name="value">Item value.</param>
        public Item Add(string name, string parametes, string value)
        {
            Item item = new Item(name, parametes, value);
            m_pItems.Add(item);

            return item;
        }

        /// <summary>
        /// Removes all items with the specified name.
        /// </summary>
        /// <param name="name">Item name.</param>
        public void Remove(string name)
        {
            for (int i = 0; i < m_pItems.Count; i++)
            {
                if (m_pItems[i].Name.ToLower() == name.ToLower())
                {
                    m_pItems.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(Item item)
        {
            m_pItems.Remove(item);
        }

        /// <summary>
        /// Clears all items in the collection.
        /// </summary>
        public void Clear()
        {
            m_pItems.Clear();
        }

        /// <summary>
        /// Gets first item with specified name. Returns null if specified item doesn't exists.
        /// </summary>
        /// <param name="name">Item name. Name compare is case-insensitive.</param>
        /// <returns>Returns first item with specified name or null if specified item doesn't exists.</returns>
        public Item GetFirst(string name)
        {
            foreach (Item item in m_pItems)
            {
                if (item.Name.ToLower() == name.ToLower())
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets items with specified name.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <returns></returns>
        public Item[] Get(string name)
        {
            List<Item> retVal = new List<Item>();
            foreach (Item item in m_pItems)
            {
                if (item.Name.ToLower() == name.ToLower())
                {
                    retVal.Add(item);
                }
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Sets first item with specified value.  If item doesn't exist, item will be appended to the end.
        /// If value is null, all items with specified name will be removed.
        /// Value is encoed as needed and specified item.ParametersString will be updated accordingly.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="value">Item value.</param>
        public void SetDecodedValue(string name, string value)
        {
            if (value == null)
            {
                Remove(name);
                return;
            }

            Item item = GetFirst(name);
            if (item != null)
            {
                item.SetDecodedValue(value);
            }
            else
            {
                item = new Item(name, "", "");
                m_pItems.Add(item);
                item.SetDecodedValue(value);
            }
        }

        /// <summary>
        /// Sets first item with specified encoded value.  If item doesn't exist, item will be appended to the end.
        /// If value is null, all items with specified name will be removed.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="value">Item encoded value.</param>
        public void SetValue(string name, string value)
        {
            SetValue(name, "", value);
        }

        /// <summary>
        /// Sets first item with specified name encoded value.  If item doesn't exist, item will be appended to the end.
        /// If value is null, all items with specified name will be removed.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="parametes">Item parameters.</param>
        /// <param name="value">Item encoded value.</param>
        public void SetValue(string name, string parametes, string value)
        {
            if (value == null)
            {
                Remove(name);
                return;
            }

            Item item = GetFirst(name);
            if (item != null)
            {
                item.Value = value;
            }
            else
            {
                m_pItems.Add(new Item(name, parametes, value));
            }
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pItems.GetEnumerator();
        }

        #endregion
    }
}