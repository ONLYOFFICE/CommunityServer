#region Copyright © 2006 Andy Johns. All rights reserved.
/*
Copyright © 2006 Andy Johns. All rights reserved.

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
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Base class for Regex tasks
    /// Handles public properties for Input, Expression, Options and Output
    /// </summary>
    public abstract class RegexBase : Task
    {
        private ITaskItem[] input = null;
        private ITaskItem expression = null;
        private ITaskItem[] options = null;
        private RegexOptions expressionOptions = RegexOptions.None;
        private ITaskItem[] output = null;

        /// <summary>
        /// Regex expression
        /// </summary>
        [Required]
        public ITaskItem Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        /// <summary>
        /// Input, list of items to perform the regex on
        /// </summary>
        [Required]
        public ITaskItem[] Input
        {
            get { return input; }
            set { input = value; }
        }


        /// <summary>
        /// Regex options as strings corresponding to the RegexOptions enum:
        ///     Compiled
        ///     CultureInvariant
        ///     ECMAScript 
        ///     ExplicitCapture
        ///     IgnoreCase
        ///     IgnorePatternWhitespace
        ///     Multiline
        ///     None
        ///     RightToLeft
        ///     Singleline
        /// </summary>
        /// <enum cref="System.Text.RegularExpressions.RegexOptions" />
        public ITaskItem[] Options
        {
            get { return options; }
            set 
            {

                options = value;
                expressionOptions = RegexOptions.None;

                if (options != null)
                {
                    foreach (ITaskItem option in options)
                    {
                        string thisOption = option.ItemSpec;
                        if (Enum.IsDefined(typeof(RegexOptions), thisOption))
                        {
                            expressionOptions = expressionOptions | (RegexOptions)Enum.Parse(typeof(RegexOptions), thisOption);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Results of the Regex transformation.
        /// </summary>
        [Output]
        public ITaskItem[] Output
        {
            set { output = value; }
            get { return output; }
        }

        /// <summary>
        /// Options converted to RegexOptions enum
        /// </summary>
        protected RegexOptions ExpressionOptions
        {
            get { return expressionOptions; }
        }
    }
}
