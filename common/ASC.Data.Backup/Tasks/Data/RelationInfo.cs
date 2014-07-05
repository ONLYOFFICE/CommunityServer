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