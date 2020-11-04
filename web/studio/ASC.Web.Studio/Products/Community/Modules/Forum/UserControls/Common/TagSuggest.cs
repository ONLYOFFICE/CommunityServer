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
