/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Data.Backup.Exceptions
{
    internal static class ThrowHelper
    {
        public static DbBackupException CantDetectTenant(string tableName)
        {
            return new DbBackupException(string.Format("Can't detect tenant column for table {0}.", tableName));
        }

        public static DbBackupException CantOrderTables(IEnumerable<string> conflictingTables)
        {
            return new DbBackupException(string.Format("Can't order tables [\"{0}\"].", string.Join("\", \"", conflictingTables.ToArray())));
        }

        public static DbBackupException CantOrderModules(IEnumerable<Type> conflictingTypes)
        {
            return new DbBackupException(string.Format("Can't order modules [\"{0}\"].", string.Join("\", \"", conflictingTypes.Select(x => x.Name).ToArray())));
        }

        public static DbBackupException CantRestoreTable(string tableName, Exception reason)
        {
            return new DbBackupException(string.Format("Can't restore table {0}.", tableName), reason);
        }

        public static DbBackupException CantBackupTable(string tableName, Exception reason)
        {
            return new DbBackupException(string.Format("Can't backup table {0}.", tableName), reason);
        }

        public static DbBackupException CantDeleteTable(string tableName, Exception reason)
        {
            return new DbBackupException(string.Format("Can't delete table {0}.", tableName), reason);
        }
    }
}
