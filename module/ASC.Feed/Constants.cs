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


namespace ASC.Feed
{
    public class Constants
    {
        public const string FeedDbId = "core";
        public const string ProjectsDbId = "projects";
        public const string FilesDbId = "files";
        public const string CommunityDbId = "community";
        public const string CrmDbId = "crm";

        #region Modules

        public static string BookmarksModule
        {
            get { return "bookmarks"; }
        }

        public static string BlogsModule
        {
            get { return "blogs"; }
        }

        public static string ForumsModule
        {
            get { return "forums"; }
        }

        public static string EventsModule
        {
            get { return "events"; }
        }

        public static string ProjectsModule
        {
            get { return "projects"; }
        }

        public static string MilestonesModule
        {
            get { return "milestones"; }
        }

        public static string DiscussionsModule
        {
            get { return "discussions"; }
        }

        public static string TasksModule
        {
            get { return "tasks"; }
        }

        public static string CommentsModule
        {
            get { return "comments"; }
        }

        public static string CrmTasksModule
        {
            get { return "crmTasks"; }
        }

        public static string ContactsModule
        {
            get { return "contacts"; }
        }

        public static string DealsModule
        {
            get { return "deals"; }
        }

        public static string CasesModule
        {
            get { return "cases"; }
        }

        public static string FilesModule
        {
            get { return "files"; }
        }

        public static string FoldersModule
        {
            get { return "folders"; }
        }

        #endregion
    }
}
