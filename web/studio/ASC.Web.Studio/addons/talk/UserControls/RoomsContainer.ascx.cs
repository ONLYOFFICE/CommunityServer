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
using System.Text;
using System.Web;

namespace ASC.Web.Talk.UserControls
{
    public partial class RoomsContainer : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/talk/js/talk.roomscontainer.js"));

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
