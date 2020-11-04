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
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    public class Comment : IWikiObjectOwner
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid ParentId
        {
            get;
            set;
        }

        public string PageName
        {
            get;
            set;
        }

        public string Body
        {
            get;
            set;
        }

        public Guid UserId
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }

        public bool Inactive
        {
            get;
            set;
        }

        public Guid OwnerID
        {
            get { return UserId; }
        }

        public object GetObjectId()
        {
            return Id;
        }
    }
}
