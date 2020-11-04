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



namespace MSBuild.Community.Tasks.Math
{
    /// <summary>
    /// Add numbers
    /// </summary>
    /// <example>Adding numbers:
    /// <code><![CDATA[
    /// <Math.Add Numbers="4;3">
    ///     <Output TaskParameter="Result" PropertyName="Result" />
    /// </Math.Add>
    /// <Message Text="Add 4+3= $(Result)"/>
    /// ]]></code>
    /// </example>
    public class Add : MathBase
    {

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            decimal[] numbers = StringArrayToDecimalArray(this.Numbers);
            decimal total = 0;
            
            StringBuilder logger = new StringBuilder();
            logger.Append("Add numbers: ");

            foreach (decimal number in numbers)
            {
                logger.AppendFormat("{0} + ", number);
                total += number;
            }

            logger.Replace('+', '=', logger.Length - 2, 1);
            logger.Append(total);
            base.Log.LogMessage(logger.ToString());

            this.Result = total.ToString(this.NumericFormat ?? string.Empty);
            return true;
        }
    }
}
