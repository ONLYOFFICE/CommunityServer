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

using System.Collections.Generic;
using System.Linq;
using ASC.Data.Backup.Exceptions;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal static class ModuleProvider
    {
        private static readonly List<IModuleSpecifics> AllModules;

        static ModuleProvider()
        {
            AllModules = new IModuleSpecifics[]
                {
                    new TenantsModuleSpecifics(),
                    new ProjectsModuleSpecifics(),
                    new CommunityModuleSpecifics(),
                    new AuditModuleSpecifics(),
                    new CrmModuleSpecifics(),
                    new MailModuleSpecifics(),
                    new CrmModuleSpecifics2(),
                    new FilesModuleSpecifics(),
                    new CalendarModuleSpecifics(),
                    new CrmInvoiceModuleSpecifics(),
                    new WebStudioModuleSpecifics(),
                    new CoreModuleSpecifics()
                }
                .OrderBy(m => m, new ModuleComparer())
                .ToList();
        }

        public static IModuleSpecifics GetByStorageModule(string storageModuleName, string storageDomainName = null)
        {
            switch (storageModuleName)
            {
                case "files":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Files);

                case "projects":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Projects);

                case "crm":
                    return AllModules.FirstOrDefault(m => m.ModuleName == (storageDomainName == "mail_messages" ? ModuleName.Crm2 : ModuleName.Crm));

                case "forum":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Community);
                
                case "mailaggregator":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Mail);

                default:
                    return null;
            }
        }

        public static IEnumerable<IModuleSpecifics> GetAll()
        {
            return AllModules;
        }

        private class ModuleComparer : IComparer<IModuleSpecifics>
        {
            public int Compare(IModuleSpecifics x, IModuleSpecifics y)
            {
                var typeOfX = x.GetType();
                var typeOfY = y.GetType();
                bool xParentToY = y.TableRelations.Any(r => r.ParentModule == typeOfX);
                bool yParentToX = x.TableRelations.Any(r => r.ParentModule == typeOfY);
                if (xParentToY && yParentToX)
                    throw ThrowHelper.CantOrderModules(new[] {typeOfX, typeOfY});
                if (xParentToY)
                    return -1;
                if (yParentToX)
                    return 1;
                return 0;
            }
        }
    }
}
