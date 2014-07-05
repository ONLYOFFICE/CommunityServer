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

namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Represents MIME child entity collection in multipart/xxx entity.
    /// </summary>
    public class MIME_EntityCollection : IEnumerable
    {
        #region Members

        private readonly List<MIME_Entity> m_pCollection;
        private bool m_IsModified;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if enity collection has modified.
        /// </summary>
        public bool IsModified
        {
            get
            {
                if (m_IsModified)
                {
                    return true;
                }

                foreach (MIME_Entity entity in m_pCollection)
                {
                    if (entity.IsModified)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pCollection.Count; }
        }

        /// <summary>
        /// Gets MIME entity at the specified index.
        /// </summary>
        /// <param name="index">MIME entity zero-based index.</param>
        /// <returns>Returns MIME entity.</returns>
        public MIME_Entity this[int index]
        {
            get { return m_pCollection[index]; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal MIME_EntityCollection()
        {
            m_pCollection = new List<MIME_Entity>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds specified MIME enity to the collection.
        /// </summary>
        /// <param name="entity">MIME entity.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>entity</b> is null reference.</exception>
        public void Add(MIME_Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            m_pCollection.Add(entity);
            m_IsModified = true;
        }

        /// <summary>
        /// Inserts a new MIME entity into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the MIME entity.</param>
        /// <param name="entity">MIME entity.</param>
        /// <exception cref="IndexOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>entity</b> is null reference.</exception>
        public void Insert(int index, MIME_Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            m_pCollection.Insert(index, entity);
            m_IsModified = true;
        }

        /// <summary>
        /// Removes specified MIME entity from the collection.
        /// </summary>
        /// <param name="entity">MIME entity.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference.</exception>
        public void Remove(MIME_Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("field");
            }

            m_pCollection.Remove(entity);
            m_IsModified = true;
        }

        /// <summary>
        /// Removes MIME entity at the specified index from the collection.
        /// </summary>
        /// <param name="index">The index of the MIME entity to remove.</param>
        /// <exception cref="IndexOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        public void Remove(int index)
        {
            m_pCollection.RemoveAt(index);
            m_IsModified = true;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            m_pCollection.Clear();
            m_IsModified = true;
        }

        /// <summary>
        /// Gets if the collection contains specified MIME entity.
        /// </summary>
        /// <param name="entity">MIME entity.</param>
        /// <returns>Returns true if the specified MIME entity exists in the collection, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>entity</b> is null.</exception>
        public bool Contains(MIME_Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            return m_pCollection.Contains(entity);
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns>Returns IEnumerator interface.</returns>
        public IEnumerator GetEnumerator()
        {
            return m_pCollection.GetEnumerator();
        }

        #endregion
    }
}