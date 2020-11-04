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
    public interface IMessageDao
    {
        List<Message> GetAll();

        List<Message> GetByProject(int projectId);

        List<Message> GetMessages(int startIndex, int maxResult);

        List<Message> GetRecentMessages(int offset, int maxResult, params int[] projects);

        List<Message> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess);

        int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess);

        List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess);

        Message GetById(int id);

        bool IsExists(int id);

        Message Save(Message message);

        void Delete(int id);

        IEnumerable<Message> GetMessages(Exp where);
    }
}
