

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.SourceSafe.Interop;

namespace MSBuild.Community.Tasks.SourceSafe
{
    /// <summary>
    /// Base class for VSS tasks that can act recursively.
    /// </summary>
    public abstract class VssRecursiveBase : VssBase
    {
        internal const VSSFlags RecursiveFlag = VSSFlags.VSSFLAG_RECURSYES |
            VSSFlags.VSSFLAG_FORCEDIRNO;

        private bool _recursive = true;

        /// <summary>
        /// Determines whether to perform the SourceSafe operation
        /// recursively. The default is <see langword="true"/>.
        /// </summary>
        public bool Recursive
        {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <returns>Reserved.</returns>
        public override bool Execute()
        {
            throw new InvalidOperationException("You cannot execute this task directly.");
        }
    }
}
