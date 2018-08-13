/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Web;
using System.Web.UI;
using System.Text;
using ASC.Data.Storage;

namespace ASC.Web.UserControls.Forum.Common
{
    public class ForumScriptProvider : Control
    {
        public bool RegistrySearchHelper { get; set; }

        public Guid SettingsID { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            Page.RegisterBodyScripts(Community.Forum.ForumManager.BaseVirtualPath + "/js/forum.js");

            if (RegistrySearchHelper)
                Page.RegisterBodyScripts(Community.Forum.ForumManager.BaseVirtualPath + "/js/searchhelper.js");


            var script = new StringBuilder();
            script.Append("if (typeof(ForumManager)=== 'undefined'){ForumManager = {};}");
            script.Append("ForumManager.QuestionEmptyMessage = '" + Resources.ForumUCResource.QuestionEmptyMessage + "';");
            script.Append("ForumManager.SubjectEmptyMessage = '" + Resources.ForumUCResource.SubjectEmptyMessage + "';");
            script.Append("ForumManager.ApproveTopicButton = '" + Resources.ForumUCResource.ApproveButton + "';");
            script.Append("ForumManager.OpenTopicButton = '" + Resources.ForumUCResource.OpenTopicButton + "';");
            script.Append("ForumManager.CloseTopicButton = '" + Resources.ForumUCResource.CloseTopicButton + "';");
            script.Append("ForumManager.StickyTopicButton = '" + Resources.ForumUCResource.StickyTopicButton + "';");
            script.Append("ForumManager.ClearStickyTopicButton = '" + Resources.ForumUCResource.ClearStickyTopicButton + "';");
            script.Append("ForumManager.DeleteTopicButton = '" + Resources.ForumUCResource.DeleteButton + "';");
            script.Append("ForumManager.EditTopicButton = '" + Resources.ForumUCResource.EditButton + "';");
            script.Append("ForumManager.ConfirmMessage = '" + Resources.ForumUCResource.ConfirmMessage + "';");
            script.Append("ForumManager.NameEmptyString = '" + Resources.ForumUCResource.NameEmptyString + "';");
            script.Append("ForumManager.SaveButton = '" + Resources.ForumUCResource.SaveButton + "';");
            script.Append("ForumManager.CancelButton = '" + Resources.ForumUCResource.CancelButton + "';");
            script.Append("ForumManager.SettingsID = '" + SettingsID + "';");
            script.Append("ForumManager.TextEmptyMessage = '" + Resources.ForumUCResource.TextEmptyMessage + "';");

            Page.RegisterInlineScript(script.ToString());

        }

    }
}
