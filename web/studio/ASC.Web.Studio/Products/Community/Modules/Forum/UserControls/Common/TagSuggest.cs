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
using AjaxPro;
using ASC.Forum;
using ASC.Web.Studio.Utility;

namespace ASC.Web.UserControls.Forum.Common
{
    [AjaxNamespace("TagSuggest")]
    public class TagSuggest
    {
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse GetSuggest(Guid settingsID, string text, string varName)
        {
            AjaxResponse resp = new AjaxResponse();

            string startSymbols = text;
            int ind = startSymbols.LastIndexOf(",");
            if (ind != -1)
                startSymbols = startSymbols.Substring(ind + 1);

            startSymbols = startSymbols.Trim();

            var tags = new List<Tag>();

            if (!String.IsNullOrEmpty(startSymbols))
                tags = ForumDataProvider.SearchTags(TenantProvider.CurrentTenantID, startSymbols);

            int counter = 0;
            string resNames = "", resHelps = "";

            foreach (var tag in tags)
            {
                if (counter > 10)
                    break;

                resNames += tag.Name + "$";
                resHelps += tag.ID + "$";
                counter++;
            }

            resNames = resNames.TrimEnd('$');
            resHelps = resHelps.TrimEnd('$');
            resp.rs1 = resNames;
            resp.rs2 = resHelps;
            resp.rs3 = text;
            resp.rs4 = varName;

            return resp;
        }

    }
}
