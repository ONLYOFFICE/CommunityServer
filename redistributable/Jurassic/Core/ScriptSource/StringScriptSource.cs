using System;
using System.IO;

namespace Jurassic
{
    /// <summary>
    /// Represents a string containing script code.
    /// </summary>
    public class StringScriptSource : ScriptSource
    {
        private string code;
        private string path;

        /// <summary>
        /// Creates a new StringScriptSource instance.
        /// </summary>
        /// <param name="code"> The script code. </param>
        public StringScriptSource(string code)
            : this(code, null)
        {
        }

        /// <summary>
        /// Creates a new StringScriptSource instance.
        /// </summary>
        /// <param name="code"> The script code. </param>
        /// <param name="path"> The path of the file the script code was retrieved from. </param>
        public StringScriptSource(string code, string path)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            this.code = code;
            this.path = path;
        }

        /// <summary>
        /// Gets the path of the source file (either a path on the file system or a URL).  This
        /// can be <c>null</c> if no path is available.
        /// </summary>
        public override string Path
        {
            get { return this.path; }
        }

        /// <summary>
        /// Returns a reader that can be used to read the source code for the script.
        /// </summary>
        /// <returns> A reader that can be used to read the source code for the script, positioned
        /// at the start of the source code. </returns>
        /// <remarks> If this method is called multiple times then each reader must return the
        /// same source code. </remarks>
        public override TextReader GetReader()
        {
            return new StringReader(this.code);
        }
    }
}
