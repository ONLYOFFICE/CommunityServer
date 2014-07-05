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
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Community.News.Resources;
using Action = ASC.Common.Security.Authorizing.Action;

namespace ASC.Web.Community.News
{
    public static class NewsConst
    {
        public static Guid ModuleId = new Guid("3CFD481B-46F2-4a4a-B55C-B8C0C9DEF02C");


        public static readonly Action Action_Add = new Action(new Guid("{2C6552B3-B2E0-4a00-B8FD-13C161E337B1}"), NewsResource.Action_Add_Name);
        public static readonly Action Action_Edit = new Action(new Guid("{14BE970F-7AF5-4590-8E81-EA32B5F7866D}"), NewsResource.Action_Edit_Name);
        public static readonly Action Action_Comment = new Action(new Guid("{FCAC42B8-9386-48eb-A938-D19B3C576912}"), NewsResource.Action_Comment_Name);

        public static INotifyAction NewFeed = new NotifyAction("new feed", "news added");
        public static INotifyAction NewComment = new NotifyAction("new feed comment", "new feed comment");

        public static string TagCaption = "Caption";
        public static string TagText = "Text";
        public static string TagDate = "Date";
        public static string TagURL = "URL";
        public static string TagUserName = "UserName";
        public static string TagUserUrl = "UserURL";
        public static string TagAnswers = "Answers";

        public static string TagFEED_TYPE = "FEED_TYPE";

    }
}