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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Circle collection. Elements will be circled clockwise.
    /// </summary>
    public class CircleCollection<T>
    {
        #region Members

        private readonly List<T> m_pItems;
        private int m_Index;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CircleCollection()
        {
            m_pItems = new List<T>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pItems.Count; }
        }

        /// <summary>
        /// Gets item at the specified index.
        /// </summary>
        /// <param name="index">Item zero based index.</param>
        /// <returns>Returns item at the specified index.</returns>
        public T this[int index]
        {
            get { return m_pItems[index]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds specified items to the collection.
        /// </summary>
        /// <param name="items">Items to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>items</b> is null.</exception>
        public void Add(T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            foreach (T item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Adds specified item to the collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>item</b> is null.</exception>
        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            m_pItems.Add(item);

            // Reset loop index.
            m_Index = 0;
        }

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>item</b> is null.</exception>
        public void Remove(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            m_pItems.Remove(item);

            // Reset loop index.
            m_Index = 0;
        }

        /// <summary>
        /// Clears all items from collection.
        /// </summary>
        public void Clear()
        {
            m_pItems.Clear();

            // Reset loop index.
            m_Index = 0;
        }

        /// <summary>
        /// Gets if the collection contain the specified item.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Returns true if the collection contain the specified item, otherwise false.</returns>
        public bool Contains(T item)
        {
            return m_pItems.Contains(item);
        }

        /// <summary>
        /// Gets next item from the collection. This method is thread-safe.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when thre is no items in the collection.</exception>
        public T Next()
        {
            if (m_pItems.Count == 0)
            {
                throw new InvalidOperationException("There is no items in the collection.");
            }

            lock (m_pItems)
            {
                T item = m_pItems[m_Index];

                m_Index++;
                if (m_Index >= m_pItems.Count)
                {
                    m_Index = 0;
                }

                return item;
            }
        }

        /// <summary>
        /// Copies all elements to new array, all elements will be in order they added. This method is thread-safe.
        /// </summary>
        /// <returns>Returns elements in a new array.</returns>
        public T[] ToArray()
        {
            lock (m_pItems)
            {
                return m_pItems.ToArray();
            }
        }

        /// <summary>
        /// Copies all elements to new array, all elements will be in current circle order. This method is thread-safe.
        /// </summary>
        /// <returns>Returns elements in a new array.</returns>
        public T[] ToCurrentOrderArray()
        {
            lock (m_pItems)
            {
                int index = m_Index;
                T[] retVal = new T[m_pItems.Count];
                for (int i = 0; i < m_pItems.Count; i++)
                {
                    retVal[i] = m_pItems[index];

                    index++;
                    if (index >= m_pItems.Count)
                    {
                        index = 0;
                    }
                }

                return retVal;
            }
        }

        #endregion
    }
}