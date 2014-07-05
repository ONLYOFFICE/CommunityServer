#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.
Implemented by James Higgs

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  
*/
#endregion



using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// A task for sleeping for a specified period of time.
    /// </summary>
    /// <example>Causes the build to sleep for 300 milliseconds.
    /// <code><![CDATA[
    /// <Sleep Milliseconds="300" />
    /// ]]></code>
    /// </example>
    public class Sleep : Task
    {
        private int _milliseconds = 0;
        private int _seconds = 0;
        private int _minutes = 0;
        private int _hours = 0;

        /// <summary>
        /// The number of milliseconds to add to the time to sleep.
        /// </summary>
        public int Milliseconds
        {
            get { return _milliseconds; }
            set { _milliseconds = value; }
        }

        /// <summary>
        /// The number of seconds to add to the time to sleep.
        /// </summary>
        public int Seconds
        {
            get { return _seconds; }
            set { _seconds = value; }
        }

        /// <summary>
        /// The number of minutes to add to the time to sleep.
        /// </summary>
        public int Minutes
        {
            get { return _minutes; }
            set { _minutes = value; }
        }

        /// <summary>
        /// The number of hours to add to the time to sleep.
        /// </summary>
        public int Hours
        {
            get { return _hours; }
            set { _hours = value; }
        }
        
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            int sleepTime = GetSleepTime();
            sleepTime = System.Math.Max(sleepTime, 0);

            Log.LogMessage(MessageImportance.Normal, "Sleeping for {0} milliseconds.", sleepTime);
            Thread.Sleep(sleepTime);
            return true;
        }

        private int GetSleepTime()
        {
            TimeSpan sleepTime = new TimeSpan(0, 0, _minutes, _seconds, _milliseconds);
            return (int)sleepTime.TotalMilliseconds;
        }
    }
}
