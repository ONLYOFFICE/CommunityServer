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
using System.Diagnostics;

namespace ASC.Data.Backup.Tasks.Data
{
    internal enum RelationImportance
    {
        Low,
        Normal
    }

    [DebuggerDisplay("({ParentTable},{ParentColumn})->({ChildTable},{ChildColumn})")]
    internal class RelationInfo
    {
        public string ParentTable { get; private set; }
        public string ParentColumn { get; private set; }
        public string ChildTable { get; private set; }
        public string ChildColumn { get; private set; }

        public Type ParentModule { get; private set; }
        public RelationImportance Importance { get; private set; }

        public Func<DataRowInfo, bool> CollisionResolver { get; private set; } 


        public RelationInfo(string parentTable, string parentColumn, string childTable, string childColumn)
            : this(parentTable, parentColumn, childTable, childColumn, null, null)
        {
            
        }

        public RelationInfo(string parentTable, string parentColumn, string childTable, string childColumn, Func<DataRowInfo, bool> collisionResolver)
            : this(parentTable, parentColumn, childTable, childColumn, null, collisionResolver)
        {
            
        }

        public RelationInfo(string parentTable, string parentColumn, string childTable, string childColumn, Type parentModule)
            : this(parentTable, parentColumn, childTable, childColumn, parentModule, null)
        {
            
        }

        public RelationInfo(string parentTable, string parentColumn, string childTable, string childColumn, Type parentModule, Func<DataRowInfo, bool> collisionResolver)
            : this(parentTable, parentColumn, childTable, childColumn, parentModule, collisionResolver, RelationImportance.Normal)
        {
            
        }

        public RelationInfo(string parentTable, string parentColumn, string childTable, string childColumn, Type parentModule, Func<DataRowInfo, bool> collisionResolver, RelationImportance importance)
        {
            ParentTable = parentTable;
            ParentColumn = parentColumn;
            ChildTable = childTable;
            ChildColumn = childColumn;
            ParentModule = parentModule;
            CollisionResolver = collisionResolver;
            Importance = importance;
        }


        public bool FitsForTable(string tableName)
        {
            return string.Equals(tableName, ChildTable, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool FitsForRow(DataRowInfo row)
        {
            return FitsForTable(row.TableName) && (CollisionResolver == null || CollisionResolver(row));
        }

        public bool IsExternal()
        {
            return ParentModule != null;
        }

        public bool IsSelfRelation()
        {
            return string.Equals(ParentTable, ChildTable, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}