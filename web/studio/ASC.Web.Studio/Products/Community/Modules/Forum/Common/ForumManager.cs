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
using ASC.Forum;
using ASC.Web.Community.Product;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public static class ForumManager
    {
        public static string DbId
        {
            get { return "community"; }
        }

        public static Guid ModuleID
        {
            get { return new Guid("853B6EB9-73EE-438d-9B09-8FFEEDF36234"); }
        }

        public static string BaseVirtualPath
        {
            get { return "~/Products/Community/Modules/Forum"; }
        }


        public static UserControls.Forum.Common.ForumManager Instance
        {
            get { return Settings.ForumManager; }
        }

        public static Settings Settings { get; private set; }

        static ForumManager()
        {
            Settings = new Settings();
        }
    }
}