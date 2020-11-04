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
using System.Diagnostics;

namespace ASC.Common.Data.AdoProxy
{
    class ExecuteHelper : IDisposable
    {
        private readonly Stopwatch stopwatch;
        private readonly Action<TimeSpan> onStop;


        private ExecuteHelper(Action<TimeSpan> onStop)
        {
            if (onStop == null) throw new ArgumentNullException("onStop");

            this.onStop = onStop;
            stopwatch = Stopwatch.StartNew();
        }


        public void Dispose()
        {
            stopwatch.Stop();
            try
            {
                onStop(stopwatch.Elapsed);
            }
            catch { }
        }


        public static IDisposable Begin(Action<TimeSpan> onStop)
        {
            return new ExecuteHelper(onStop);
        }
    }
}
