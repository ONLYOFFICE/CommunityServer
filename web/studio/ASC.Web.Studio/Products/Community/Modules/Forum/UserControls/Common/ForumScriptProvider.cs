/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            Page.RegisterBodyScripts(ResolveUrl(Community.Forum.ForumManager.BaseVirtualPath + "/js/forum.js"));

            if (RegistrySearchHelper)
                Page.RegisterBodyScripts(ResolveUrl(Community.Forum.ForumManager.BaseVirtualPath + "/js/searchhelper.js"));


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
