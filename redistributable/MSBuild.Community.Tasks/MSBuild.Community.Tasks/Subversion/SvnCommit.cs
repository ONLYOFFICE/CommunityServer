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



namespace MSBuild.Community.Tasks.Subversion
{
    /// <summary>
    /// Subversion Commit command
    /// </summary>
    public class SvnCommit : SvnClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SvnCommit"/> class.
        /// </summary>
        public SvnCommit()
        {
            base.Command = "commit";
            base.NonInteractive = true;
            base.NoAuthCache = true;
        }

        /// <summary>
        /// Indicates whether all task paratmeters are valid.
        /// </summary>
        /// <returns>
        /// true if all task parameters are valid; otherwise, false.
        /// </returns>
        protected override bool ValidateParameters()
        {
            if (base.Targets == null || base.Targets.Length == 0)
            {
                Log.LogError(Properties.Resources.ParameterRequired, "SvnCommit", "Targets");
                return false;
            }
            return base.ValidateParameters();
        }
    }
}
