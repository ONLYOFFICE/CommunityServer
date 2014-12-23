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
using ASC.Web.Community.News;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;

[assembly: Product(typeof(NewsModule))]

namespace ASC.Web.Community.News
{
    public class NewsModule : Module
    {
        public override Guid ID
        {
            get { return NewsConst.ModuleId; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return NewsResource.AddonName; }
        }

        public override string Description
        {
            get { return NewsResource.AddonDescriptionResourceKey; }
        }

        public override string StartURL
        {
            get { return "~/products/community/modules/news/"; }
        }

        public NewsModule()
        {
            Context = new ModuleContext
                          {
                              SmallIconFileName = "newslogo.png",
                              IconFileName = "32x_news.png",
                              SubscriptionManager = new SubscriptionManager(),
                              GetCreateContentPageAbsoluteUrl = () => CommunitySecurity.CheckPermissions(NewsConst.Action_Add) ? FeedUrls.EditNewsUrl : null,
                              SearchHandler = new SearchHandler(),
                          };
        }
    }
}