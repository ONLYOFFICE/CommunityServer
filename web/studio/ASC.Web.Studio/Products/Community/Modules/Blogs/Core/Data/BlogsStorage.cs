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


using ASC.Common.Data;
using System;

namespace ASC.Blogs.Core.Data
{
    public class BlogsStorage : IDisposable
    {
        readonly DbManager _db;
        readonly int _tenant;
        readonly IPostDao _postDao;


        public BlogsStorage(string dbId, int tenant)
        {
            _db = new DbManager(dbId);
            _tenant = tenant;
            _postDao = new DbPostDao(_db, _tenant);
        }


        public IPostDao GetPostDao()
        {
            return _postDao;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
