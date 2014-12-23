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

using System;
using System.Collections.Generic;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Engine
{
    public class CommentEngine
    {
        private readonly ICommentDao _commentDao;


        public CommentEngine(IDaoFactory daoFactory)
        {
            _commentDao = daoFactory.GetCommentDao();
        }

        public List<Comment> GetComments(DomainObject<Int32> targetObject)
        {
            return targetObject != null ? _commentDao.GetAll(targetObject) : new List<Comment>();
        }

        public Comment GetByID(Guid id)
        {
            return _commentDao.GetById(id);
        }

        public Comment GetLast(DomainObject<Int32> targetObject)
        {
            return targetObject != null ? _commentDao.GetLast(targetObject) : null;
        }

        public int Count(DomainObject<Int32> targetObject)
        {
            return targetObject == null ? 0 : _commentDao.Count(targetObject);
        }

        public List<int> GetCommentsCount(List<ProjectEntity> targets)
        {
            return _commentDao.GetCommentsCount(targets);
        }

        public Comment SaveOrUpdate(Comment comment)
        {
            if (comment == null) throw new ArgumentNullException("comment");

            ProjectSecurity.DemandCreateComment();

            if (comment.CreateBy == default(Guid)) comment.CreateBy = SecurityContext.CurrentAccount.ID;

            var now = TenantUtil.DateTimeNow();
            if (comment.CreateOn == default(DateTime)) comment.CreateOn = now;

            var newComment = _commentDao.Save(comment);
            //mark entity as jast readed

            return newComment;
        }
    }
}