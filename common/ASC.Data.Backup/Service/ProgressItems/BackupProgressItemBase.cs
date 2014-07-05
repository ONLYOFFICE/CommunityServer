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
using ASC.Common.Threading.Progress;
using ASC.Data.Backup.Logging;

namespace ASC.Data.Backup.Service.ProgressItems
{
    internal abstract class BackupProgressItemBase : IProgressItem
    {
        private readonly Guid id;
        private bool isCompleted;

        public int TenantId { get; private set; }

        public int Progress { get; protected set; }

        public bool IsCompleted
        {
            get { return isCompleted; }
            set { throw new NotSupportedException(); }
        }

        public Exception Error { get; private set; }

        protected ILog Log { get; private set; }

        protected BackupProgressItemBase(ILog log, int tenantId)
        {
            Log = log;
            id = Guid.NewGuid();
            TenantId = tenantId;
        }

        public void RunJob()
        {
            try
            {
                RunInternal();
                Progress = 100;
                isCompleted = true;
            }
            catch (Exception error)
            {
                if (Log != null)
                {
                    Log.Error(error);
                }
                Error = error;
                isCompleted = true;
            }
        }

        protected abstract void RunInternal();

        #region IProgressItem

        object IProgressItem.Id
        {
            get { return id; }
            set { throw new NotSupportedException(); }
        }

        double IProgressItem.Percentage
        {
            get { return Progress; }
            set { throw new NotSupportedException(); }
        }

        object IProgressItem.Status
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        object IProgressItem.Error
        {
            get { return Error; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region ICloneable

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
