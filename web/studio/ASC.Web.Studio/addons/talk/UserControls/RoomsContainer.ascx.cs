/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Text;
using System.Web;

namespace ASC.Web.Talk.UserControls
{
    public partial class RoomsContainer : System.Web.UI.UserControl
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
