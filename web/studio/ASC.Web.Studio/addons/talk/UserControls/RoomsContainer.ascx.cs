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
using System.Text;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Talk.UserControls
{
    public partial class RoomsContainer : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/addons/talk/js/talk.roomscontainer.js");

            var script = new StringBuilder();

            script.AppendLine("ASC.TMTalk.roomsContainer.init([");

            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile01.gif") + @"', title: ':-)', aliases: [':-)', ':)', '=)'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile02.gif") + @"', title: ';-)', aliases: [';-)'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile03.gif") + @"', title: ':-\\', aliases: [':-\\'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile04.gif") + @"', title: ':-D', aliases: [':-D', ':D'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile05.gif") + @"', title: ':-(', aliases: [':-(', ':('] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile06.gif") + @"', title: '8-)', aliases: ['8-)'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile07.gif") + @"', title: '*DANCE*', aliases: ['*DANCE*'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile08.gif") + @"', title: '[:-}', aliases: ['[:-}'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile09.gif") + @"', title: '%-)', aliases: ['%-)', '%)'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile10.gif") + @"', title: '=-O', aliases: ['=-O'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile11.gif") + @"', title: ':-P', aliases: [':-P'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile12.gif") + @"', title: ':\'(', aliases: [':\'('] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile13.gif") + @"', title: ':-!', aliases: [':-!'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile14.gif") + @"', title: '*THUMBS UP*', aliases: ['*THUMBS UP*'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile15.gif") + @"', title: '*SORRY*', aliases: ['*SORRY*'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile16.gif") + @"', title: '*YAHOO*', aliases: ['*YAHOO*'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile17.gif") + @"', title: '*OK*', aliases: ['*OK*'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile18.gif") + @"', title: ']:->', aliases: [']:-&gt;', ']:-&amp;gt;'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile19.gif") + @"', title: '*HELP*', aliases: ['*HELP*'] },");
            script.AppendLine("{ src: '" + VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/imagescss/smiles/smile20.gif") + @"', title: '*DRINK*', aliases: ['*DRINK*'] }");

            script.AppendLine("],");

            script.AppendLine("'" + GetOverdueInterval() + "');");

            Page.RegisterInlineScript(script.ToString());
        }

        public String GetOverdueInterval()
        {
            return new TalkConfiguration().OverdueInterval;
        }
    }
}
