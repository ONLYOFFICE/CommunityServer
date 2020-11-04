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

namespace ASC.Blogs.Core.Domain
{
    public class BlogInfo
    {
        private string _Name;
        private Guid _ID;
        private int _BlogsCount;
        private int _CommentsCount;
        private int _ReviewCount;

        public BlogInfo()
        { }

        public virtual int ReviewCount
        {
            get { return _ReviewCount; }
            set { _ReviewCount = value; }
        }
        public virtual string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        public virtual Guid ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        public virtual int BlogsCount
        {
            get { return _BlogsCount; }
            set { _BlogsCount = value; }
        }
        public virtual int CommentsCount
        {
            get { return _CommentsCount; }
            set { _CommentsCount = value; }
        }
    }

        
}
