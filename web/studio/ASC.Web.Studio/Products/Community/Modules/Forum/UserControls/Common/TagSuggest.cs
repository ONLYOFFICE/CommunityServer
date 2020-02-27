/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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

                resNames += tag.Name + ",";
                resHelps += tag.ID + ",";
                counter++;
            }

            resNames = resNames.TrimEnd(',');
            resHelps = resHelps.TrimEnd(',');
            resp.rs1 = resNames;
            resp.rs2 = resHelps;
            resp.rs3 = text;
            resp.rs4 = varName;

            return resp;
        }

    }
}
