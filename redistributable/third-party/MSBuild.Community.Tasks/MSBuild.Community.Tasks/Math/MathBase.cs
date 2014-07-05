#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

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
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;



namespace MSBuild.Community.Tasks.Math
{
    /// <summary>
    /// Math task base class
    /// </summary>
    public abstract class MathBase : Task
    {
        private string[] _numbers;

        /// <summary>
        /// Gets or sets the numbers to work with.
        /// </summary>
        /// <value>The numbers.</value>
        [Required]
        public string[] Numbers
        {
            get { return _numbers; }
            set { _numbers = value; }
        }

        private string _result;

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        [Output]
        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }

        private string _numericFormat;

        /// <summary>
        /// Gets or sets the numeric format.
        /// </summary>
        /// <value>The numeric format.</value>
        public string NumericFormat
        {
            get { return _numericFormat; }
            set { _numericFormat = value; }
        }
        

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            return true;
        }

        /// <summary>
        /// Strings array to decimal array.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns></returns>
        protected decimal[] StringArrayToDecimalArray(string[] numbers)
        {
            decimal[] result = new decimal[numbers.Length];

            for(int x = 0; x < numbers.Length; x++)
            {
                decimal converted;
                bool isParsed = decimal.TryParse(numbers[x], out converted);
                if (!isParsed)
                {
                    Log.LogError(Properties.Resources.MathNotNumber, numbers[x]);
                    result[x] = 0;
                }
                else
                {
                    result[x] = converted;
                }
            }
            return result;
        }
    }
}
