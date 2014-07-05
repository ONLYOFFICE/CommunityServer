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
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using ASC.Notify.Cron;

namespace ASC.FullTextIndex.Service.Config
{
    class TextIndexCfg
    {
        private readonly string indexPath;


        public readonly static int MaxQueryLength = 30;


        public CronExpression ChangedCron
        {
            get;
            private set;
        }

        public CronExpression RemovedCron
        {
            get;
            private set;
        }

        public IList<ModuleInfo> Modules
        {
            get;
            private set;
        }

        public string ConnectionStringName
        {
            get;
            private set;
        }

        public int UserActivityDays
        {
            get;
            private set;
        }


        public TextIndexCfg()
        {
            var cfg = (TextIndexCfgSectionHandler)ConfigurationManager.GetSection("fullTextIndex");

            indexPath = cfg.IndexPath.Trim('\\').Trim();
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(indexPath))
            {
                indexPath = currentDirectory;
            }
            if (!Path.IsPathRooted(indexPath))
            {
                indexPath = Path.Combine(currentDirectory, indexPath);
            }

            ChangedCron = new CronExpression(cfg.ChangedCron);
            RemovedCron = new CronExpression(cfg.RemovedCron);
            ConnectionStringName = cfg.ConnectionStringName;
            UserActivityDays = cfg.UserActivityDays;
            Modules = cfg.Modules
                .Cast<TextIndexCfgModuleElement>()
                .Select(e => new ModuleInfo(e.Name, e.Select, e.ConnectionStringName ?? ConnectionStringName))
                .ToList();
        }


        public string GetIndexPath(int tenantId, string module)
        {
            var path = Path.Combine(indexPath, tenantId.ToString("00/00/00", CultureInfo.InvariantCulture));
            path = Path.Combine(path, module);
            return path;
        }
    }
}
