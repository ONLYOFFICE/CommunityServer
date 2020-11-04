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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASC.Web.Studio.Controls.Common
{
    [ToolboxData("<{0}:InlineScript></{0}:InlineScript>")]
    public class InlineScript : WebControl
    {
        public List<Tuple<string, bool>> Scripts { get; set; }

        public InlineScript()
        {
            Scripts = new List<Tuple<string, bool>>();
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            output.RenderBeginTag(HtmlTextWriterTag.Script);

            foreach (var script in Scripts.Where(r => !r.Item2))
            {
                output.Write(script.Item1);
            }

            var inlineScripts = Scripts.Where(r => r.Item2).Select(r => r.Item1).Distinct().ToList();

            if (inlineScripts.Any())
            {
                output.Write("jq(document).ready(function(){");

                foreach (var script in inlineScripts)
                {
                    output.Write(script);
                }

                output.Write("});");
            }

            output.RenderEndTag();
        }
    }
}