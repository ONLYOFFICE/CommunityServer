using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a set of options that influence the compiler.
    /// </summary>
    internal sealed class CompilerOptions
    {
        /// <summary>
        /// Creates a new CompilerOptions instance.
        /// </summary>
        public CompilerOptions()
        {
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to force ES5 strict mode, even if the code
        /// does not contain a strict mode directive ("use strict").  The default is <c>false</c>.
        /// </summary>
        public bool ForceStrictMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value which indicates whether debug information should be generated.  If
        /// this is set to <c>true</c> performance and memory usage are negatively impacted.  The
        /// default is <c>false</c>.
        /// </summary>
        public bool EnableDebugging
        {
            get;
            set;
        }

        /// <summary>
        /// Performs a shallow clone of this instance.
        /// </summary>
        /// <returns> A shallow clone of this instance. </returns>
        public CompilerOptions Clone()
        {
            return (CompilerOptions)this.MemberwiseClone();
        }
    }

}