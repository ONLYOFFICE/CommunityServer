/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Threading;

using ASC.Migration;
using ASC.Migration.Core;

namespace ASC.Api.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class OngoingMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsCancel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CancellationTokenSource CancelTokenSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IMigration Migration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ParseProgressItem ParseTask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MigrateProgressItem MigrationTask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool MigrationEnded =>
            ParseTask != null && ParseTask.IsCompleted
            && MigrationTask != null && MigrationTask.IsCompleted;
    }
}
