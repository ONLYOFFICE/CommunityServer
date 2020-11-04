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
    /// Divide numbers
    /// </summary>
    /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="Math.Divide"]/*'/>
    public class Divide : MathBase
    {
        private bool truncateResult;

        /// <summary>
        /// When <see langword="true"/>, uses integer division to truncate the result. Default is <see langword="false" />
        /// </summary>
        /// <remarks>
        /// Any remainder in the result is dropped, and the closest integer to zero is returned.
        /// <para>
        /// Refer to the documentation for the <see href="http://msdn2.microsoft.com/library/0e16fywh.aspx">\ Operator</see>
        /// for more information about integer division.
        /// </para>
        /// </remarks>
        public bool TruncateResult
        {
            get { return truncateResult; }
            set { truncateResult = value; }
        }


        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            decimal[] numbers = StringArrayToDecimalArray(this.Numbers);
            decimal? total = null;

            StringBuilder logger = new StringBuilder();
            logger.Append("Divide numbers: ");

            foreach (decimal number in numbers)
            {
                if (total.HasValue)
                {
                    logger.AppendFormat(" / {0}", number);
                    total /= number;
                }
                else
                {
                    logger.Append(number);
                    total = number;
                }
            }

            decimal actualTotal = total ?? 0;
            if (truncateResult)
            {
                actualTotal = (int)actualTotal;
                logger.Append(" [truncated]");
            }
            logger.AppendFormat(" = {0}", actualTotal);
            base.Log.LogMessage(logger.ToString());

            this.Result = actualTotal.ToString(this.NumericFormat ?? string.Empty);
            return true;
        }
    }
}
