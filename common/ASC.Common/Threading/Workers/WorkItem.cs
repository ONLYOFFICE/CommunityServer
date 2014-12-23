/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#region usings

using System;
using System.Diagnostics;

#endregion

namespace ASC.Common.Threading.Workers
{
    [DebuggerDisplay("Processed={IsProcessed} Added={Added}")]
    public class WorkItem<T> : IDisposable
    {
        private bool _disposed;

        public WorkItem(T item)
        {
            Item = item;
            Added = DateTime.Now;
            Completed = DateTime.MinValue;
            IsProcessed = false;
        }

        public T Item { get; set; }
        internal DateTime Added { get; set; }
        internal DateTime Completed { get; set; }
        internal Exception Error { get; set; }
        public bool IsCompleted { get; set; }
        internal int ErrorCount { get; set; }
        internal bool IsProcessed { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (WorkItem<T>) && Equals((WorkItem<T>) obj);
        }

        public bool Equals(WorkItem<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Equals(other.Item, Item);
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        ~WorkItem()
        {
            Dispose(false);
        }

        public void Dispose(bool isdisposing)
        {
            if (!_disposed)
            {
                if (isdisposing)
                {
                    if (Item is IDisposable)
                    {
                        ((IDisposable) Item).Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}