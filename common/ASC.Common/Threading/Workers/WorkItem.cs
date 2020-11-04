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

namespace ASC.Common.Threading.Workers
{
    public class WorkItem<T> : IDisposable
    {
        private bool disposed;

        internal DateTime Added { get; set; }
        internal int ErrorCount { get; set; }
        internal bool IsProcessed { get; set; }

        public T Item { get; set; }
        public bool IsCompleted { get; set; }


        public WorkItem(T item)
        {
            Item = item;
            Added = DateTime.Now;
            IsProcessed = false;
        }

        ~WorkItem()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    var disposable = Item as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                disposed = true;
            }
        }
    }
}