/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            get { return "~/products/community/modules/forum"; }
        }


        public static UserControls.Forum.Common.ForumManager Instance
        {
            get { return Settings.ForumManager; }
        }

        public static Settings Settings { get; private set; }

        static ForumManager()
        {
            Settings = new Settings
                           {
                               ProductID = CommunityProduct.ID,
                               ModuleID = new Guid("853B6EB9-73EE-438d-9B09-8FFEEDF36234"),
                               ImageItemID = new Guid("853B6EB9-73EE-438d-9B09-8FFEEDF36234"),
                               UserControlsVirtualPath = "~/products/community/modules/forum/usercontrols",
                               StartPageVirtualPath = "~/products/community/modules/forum/default.aspx",
                               TopicPageVirtualPath = "~/products/community/modules/forum/topics.aspx",
                               PostPageVirtualPath = "~/products/community/modules/forum/posts.aspx",
                               SearchPageVirtualPath = "~/products/community/modules/forum/search.aspx",
                               NewPostPageVirtualPath = "~/products/community/modules/forum/newpost.aspx",
                               EditTopicPageVirtualPath = "~/products/community/modules/forum/edittopic.aspx",
                               FileStoreModuleID = "forum",
                               ConfigPath = "~/products/community/modules/forum/web.config"
                           };
        }
    }
}