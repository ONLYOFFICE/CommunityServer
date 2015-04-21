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