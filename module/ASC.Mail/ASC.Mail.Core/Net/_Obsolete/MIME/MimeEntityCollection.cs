/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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