/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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