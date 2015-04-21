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


using System.Collections.Generic;
using ASC.Files.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.Classes
{
    internal class FileEngine2
    {
        private static FileEngine Engine
        {
            get { return Global.EngineFactory.GetFileEngine(); }
        }

        public static object GetRoot(int projectId)
        {
            return Engine.GetRoot(projectId);
        }

        public static File GetFile(int id, int version)
        {
            return Engine.GetFile(id, version);
        }

        public static File SaveFile(File file, System.IO.Stream stream)
        {
            return Engine.SaveFile(file, stream);
        }

        public static void RemoveFile(object id)
        {
            Engine.RemoveFile(id);
        }

        public static Folder SaveFolder(Folder folder)
        {
            return Engine.SaveFolder(folder);
        }

        public static void AttachFileToMessage(Message message, object fileId)
        {
            Global.EngineFactory.GetMessageEngine().AttachFile(message, fileId, false);
        }

        public static void AttachFileToTask(Task task, object fileId)
        {
            Global.EngineFactory.GetTaskEngine().AttachFile(task, fileId, false);
        }

        public static IEnumerable<File> GetTaskFiles(Task task)
        {
            return Global.EngineFactory.GetTaskEngine().GetFiles(task);
        }

        public static IEnumerable<File> GetMessageFiles(Message message)
        {
            return Global.EngineFactory.GetMessageEngine().GetFiles(message);
        }
    }
}