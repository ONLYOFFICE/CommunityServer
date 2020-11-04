/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
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