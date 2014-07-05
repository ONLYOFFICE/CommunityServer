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

#if DEBUG
using ASC.Core;
using ASC.Data.Backup.Tasks;
using ASC.Data.Backup.Tasks.Modules;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[assembly: XmlConfigurator]

namespace ASC.Data.Backup
{
	[TestClass]
	public class Test
	{
		private static ILog log = LogManager.GetLogger("ASC.Data.Backup");


		[TestMethod]
		public void DatabaseBackupTest()
		{
			var backup = CreateBackupManager();
			backup.RemoveBackupProvider("Files");
			backup.Save(1);
		}

		[TestMethod]
		public void BackupTest()
		{
			var backup = CreateBackupManager(@"..\..\..\..\..\_ci\deploy\webstudio\Web.config");
			backup.Save(0);
		}

		private BackupManager CreateBackupManager(string config)
		{
			var backup = new BackupManager("Backup.zip", config);
			backup.ProgressChanged += ProgressChanged;
			return backup;
		}

		private BackupManager CreateBackupManager()
		{
			return CreateBackupManager(null);
		}


		private void ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.Progress == -1d)
			{
				log.InfoFormat("\r\n\r\n{0}", e.Status);
			}
			else
			{
				log.InfoFormat("{0}% / {1}", e.Progress, e.Status);
			}
		}
	}

    [TestClass]
    public class TransferTests
    {
        [TestMethod]
        public void TransferPortalTest()
        {
            var transferTask = new TransferPortalTask(
                CoreContext.TenantManager.GetTenant(0),
                @"..\..\Tests\Configs\localhost\Web.config",
                @"..\..\Tests\Configs\restore\Web.config");

            transferTask.IgnoreModule(ModuleName.Mail);

            transferTask.BlockOldPortalAfterStart = false;
            transferTask.DeleteOldPortalAfterCompletion = false;
            transferTask.ProcessStorage = false;
            transferTask.DeleteBackupFileAfterCompletion = false;

            transferTask.Message += (sender, args) => Console.WriteLine("{0}: {1}", args.Reason.ToString("g"), args.Message);
            transferTask.ProgressChanged += (sender, args) => Console.WriteLine("progress: {0}%", args.Progress);

            transferTask.Run();
        }
    }
}
#endif