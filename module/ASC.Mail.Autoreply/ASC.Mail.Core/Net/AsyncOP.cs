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


namespace ASC.Mail.Net
{
    /// <summary>
    /// This is base class for asynchronous operation.
    /// </summary>
    public abstract class AsyncOP
    {
        #region Properties

        /// <summary>
        /// Gets if asynchronous operation has completed.
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// Gets if operation completed synchronously.
        /// </summary>
        public abstract bool IsCompletedSynchronously { get; }

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public abstract bool IsDisposed { get; }

        #endregion
    }
}