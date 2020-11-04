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


using System;
using System.Collections.Generic;
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Core.DataInterfaces
{
    public interface ICommentDao
    {
        List<Comment> GetAll(DomainObject<int> target);

        Comment GetById(Guid id);

        int Count(DomainObject<int> target);

        List<int> Count(List<ProjectEntity> targets);

        Comment Save(Comment comment);

        void Delete(Guid id);

        List<Comment> GetComments(Exp where);
    }
}
