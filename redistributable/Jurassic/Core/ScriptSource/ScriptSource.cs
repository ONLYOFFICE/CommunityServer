using System;
using System.IO;

namespace Jurassic
{
    /// <summary>
    /// Represents a resource that can provide script code.  This is the abstract base class of all
    /// the script providers.
    /// </summary>
    public abstract class ScriptSource
    {
        /// <summary>
        /// Gets the path of the source file (either a path on the file system or a URL).  This
        /// can be <c>null</c> if no path is available.
        /// </summary>
        public abstract string Path
        {
            get;
        }

        /// <summary>
        /// Returns a reader that can be used to read the source code for the script.
        /// </summary>
        /// <returns> A reader that can be used to read the source code for the script, positioned
        /// at the start of the source code. </returns>
        /// <remarks> If this method is called multiple times then each reader must return the
        /// same source code. </remarks>
        public abstract TextReader GetReader();

        /// <summary>
        /// Reads the source code within the given span.
        /// </summary>
        /// <param name="span"> The start and end positions within the source code. </param>
        /// <returns> The source code within the given span. </returns>
        internal string ReadSpan(Compiler.SourceCodeSpan span)
        {
            if (span == null)
                throw new ArgumentNullException("span");

            int line = 1, column = 1;
            var reader = this.GetReader();
            var spanText = new System.Text.StringBuilder();
            bool insideSpan = false;

            while (true)
            {
                // Detect the start and end of the span.
                if (insideSpan == false)
                {
                    // Check for the start of the span.
                    if (line == span.StartLine && column == span.StartColumn)
                        insideSpan = true;
                }
                if (insideSpan == true)
                {
                    // Check for the end of the span.
                    if (line == span.EndLine && column == span.EndColumn)
                        break;
                }

                // Read the next character.
                int c = reader.Read();
                if (c == -1)
                    break;

                // Collect text
                if (insideSpan == true)
                    spanText.Append((char)c);

                // Update the line and column count.
                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && reader.Peek() == '\n')
                        reader.Read();
                    line++;
                    column = 1;
                }
                else
                    column++;
            }

            return spanText.ToString();
        }
    }
}
