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
using System.Xml.Linq;

namespace ASC.Data.Backup
{
    public interface IBackupProvider
    {
        string Name
        {
            get;
        }

        IEnumerable<XElement> GetElements(int tenant, string[] configs, IDataWriteOperator writer);

        void LoadFrom(IEnumerable<XElement> elements, int tenant, string[] configs, IDataReadOperator reader);

        event EventHandler<ProgressChangedEventArgs> ProgressChanged;

    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public string Status
        {
            get;
            private set;
        }

        public double Progress
        {
            get;
            private set;
        }

        public bool Completed
        {
            get;
            private set;
        }

        public ProgressChangedEventArgs(string status, double progress)
            : this(status, progress, false)
        {
        }

        public ProgressChangedEventArgs(string status, double progress, bool completed)
        {
            Status = status;
            Progress = progress;
            Completed = completed;
        }
    }
}