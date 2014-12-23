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
using System.Web;
using ASC.Web.Community.Forum.Common;
using ASC.Web.Community.Forum.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.UserControls.Forum.Common;

[assembly: Product(typeof(ForumModule))]

namespace ASC.Web.Community.Forum.Common
{
    public class ForumModule : Module
    {
        public override Guid ID
        {
            get { return ForumManager.ModuleID; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return ForumResource.AddonName; }
        }

        public override string Description
        {
            get { return ForumResource.AddonDescription; }
        }

        public override string StartURL
        {
            get { return "~/products/community/modules/forum/"; }
        }
     
        public ForumModule()
        {
            Context = new ModuleContext
            {
                DefaultSortOrder = 2,
                SmallIconFileName = "forum_mini_icon.png",
                IconFileName = "forum_icon.png",
                SubscriptionManager = new ForumSubscriptionManager(),
                GetCreateContentPageAbsoluteUrl = ForumShortcutProvider.GetCreateContentPageUrl,
                SearchHandler = new ForumSearchHandler(),
            };
        }
    }
}
