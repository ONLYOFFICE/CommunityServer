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
using System.Threading;

namespace ASC.Data.Backup
{
    public static class ActionInvoker
    {
        public static void Try(
            Action action, 
            int maxAttempts,
            Action<Exception> onFailure = null,
            Action<Exception> onAttemptFailure = null,
            int sleepMs = 1000,
            bool isSleepExponential = true)
        {
            Try(state => action(), null, maxAttempts, onFailure, onAttemptFailure, sleepMs, isSleepExponential);
        }

        public static void Try(
            Action<object> action,
            object state,
            int maxAttempts,
            Action<Exception> onFailure = null,
            Action<Exception> onAttemptFailure = null,
            int sleepMs = 1000,
            bool isSleepExponential = true)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            int countAttempts = 0;
            while (countAttempts++ < maxAttempts)
            {
                try
                {
                    action(state);
                    return;
                }
                catch (Exception error)
                {
                    if (countAttempts < maxAttempts)
                    {
                        if (onAttemptFailure != null)
                            onAttemptFailure(error);

                        if (sleepMs > 0) 
                            Thread.Sleep(isSleepExponential ? sleepMs*countAttempts : sleepMs);
                    }
                    else if (onFailure != null)
                    {
                        onFailure(error);
                    }
                }
            }
        }
    }
}
