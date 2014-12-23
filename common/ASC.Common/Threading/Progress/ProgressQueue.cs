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

using System;
using System.Linq;
using ASC.Common.Threading.Workers;

namespace ASC.Common.Threading.Progress
{
    public class ProgressQueue : WorkerQueue<IProgressItem>
    {
        private readonly bool removeAfterCompleted;


        public ProgressQueue(int workerCount, TimeSpan waitInterval) :
            this(workerCount, waitInterval, false)
        {
        }

        
        public ProgressQueue(int workerCount, TimeSpan waitInterval, bool removeAfterCompleted)
            : base(workerCount, waitInterval, 0, false)
        {
            this.removeAfterCompleted = removeAfterCompleted;
            Start(x => x.RunJob());
        }

        public override void Add(IProgressItem item)
        {
            if (GetStatus(item.Id) == null)
            {
                base.Add(item);
            }
        }

        public IProgressItem GetStatus(object id)
        {
            IProgressItem item;
            lock (SynchRoot)
            {
                item = GetItems().Where(x => Equals(x.Id, id)).SingleOrDefault();
                if (item != null)
                {
                    if (removeAfterCompleted && item.IsCompleted)
                    {
                        Remove(item);
                    }
                    return item.Clone() as IProgressItem;
                }
            }
            return item;
        }

        public void PostComplete(object id)
        {
            lock (SynchRoot)
            {
                var item = GetItems().Where(x => Equals(x.Id, id)).SingleOrDefault();
                 
                 if (item != null)
                 {
                      item.IsCompleted = true;
                     
                      if (removeAfterCompleted)
                      {
                          Remove(item); 
                      }
                 }
            }
        }

        protected override WorkItem<IProgressItem> Selector()
        {
            return Items
                .Where(x => !x.IsProcessed && !x.IsCompleted)
                .OrderBy(x => x.Added)
                .FirstOrDefault();
        }

        protected override void PostComplete(WorkItem<IProgressItem> item)
        {
            item.Completed = DateTime.UtcNow;
            item.IsCompleted = true;
        }

        protected override void ErrorLimit(WorkItem<IProgressItem> item)
        {
            PostComplete(item);
        }
        
        protected override void Error(WorkItem<IProgressItem> workItem, Exception exception)
        {
            workItem.Item.Error = exception;
            workItem.Item.IsCompleted = true;

            base.Error(workItem, exception);
        }
    }
}