/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Mime entity collection.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class MimeEntityCollection : IEnumerable
    {
        #region Members

        private readonly List<MimeEntity> m_pEntities;
        private readonly MimeEntity m_pOwnerEntity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets mime entity at specified index.
        /// </summary>
        public MimeEntity this[int index]
        {
            get { return m_pEntities[index]; }
        }

        /// <summary>
        /// Gets mime entities count in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pEntities.Count; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ownerEntity">Mime entity what owns this collection.</param>
        internal MimeEntityCollection(MimeEntity ownerEntity)
        {
            m_pOwnerEntity = ownerEntity;

            m_pEntities = new List<MimeEntity>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new mime entity to the end of the collection.
        /// </summary>
        /// <returns></returns>
        public MimeEntity Add()
        {
            MimeEntity entity = new MimeEntity();
            Add(entity);

            return entity;
        }

        /// <summary>
        /// Adds specified mime entity to the end of the collection.
        /// </summary>
        /// <param name="entity">Mime entity to add to the collection.</param>
        public void Add(MimeEntity entity)
        {
            // Allow to add only for multipart/xxx...
            if ((m_pOwnerEntity.ContentType & MediaType_enum.Multipart) == 0)
            {
                throw new Exception(
                    "You don't have Content-Type: multipart/xxx. Only Content-Type: multipart/xxx can have nested mime entities !");
            }
            // Check boundary, this is required parameter for multipart/xx
            if (m_pOwnerEntity.ContentType_Boundary == null || m_pOwnerEntity.ContentType_Boundary.Length == 0)
            {
                throw new Exception("Please specify Boundary property first !");
            }

            m_pEntities.Add(entity);
        }

        /// <summary>
        /// Inserts a new mime entity into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the mime entity.</param>
        /// <param name="entity">Mime entity.</param>
        public void Insert(int index, MimeEntity entity)
        {
            // Allow to add only for multipart/xxx...
            if ((m_pOwnerEntity.ContentType & MediaType_enum.Multipart) == 0)
            {
                throw new Exception(
                    "You don't have Content-Type: multipart/xxx. Only Content-Type: multipart/xxx can have nested mime entities !");
            }
            // Check boundary, this is required parameter for multipart/xx
            if (m_pOwnerEntity.ContentType_Boundary == null || m_pOwnerEntity.ContentType_Boundary.Length == 0)
            {
                throw new Exception("Please specify Boundary property first !");
            }

            m_pEntities.Insert(index, entity);
        }

        /// <summary>
        /// Removes mime entity at the specified index from the collection.
        /// </summary>
        /// <param name="index">Index of mime entity to remove.</param>
        public void Remove(int index)
        {
            m_pEntities.RemoveAt(index);
        }

        /// <summary>
        /// Removes specified mime entity from the collection.
        /// </summary>
        /// <param name="entity">Mime entity to remove.</param>
        public void Remove(MimeEntity entity)
        {
            m_pEntities.Remove(entity);
        }

        /// <summary>
        /// Clears the collection of all mome entities.
        /// </summary>
        public void Clear()
        {
            m_pEntities.Clear();
        }

        /// <summary>
        /// Gets if collection contains specified mime entity.
        /// </summary>
        /// <param name="entity">Mime entity.</param>
        /// <returns></returns>
        public bool Contains(MimeEntity entity)
        {
            return m_pEntities.Contains(entity);
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pEntities.GetEnumerator();
        }

        #endregion
    }
}