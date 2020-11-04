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