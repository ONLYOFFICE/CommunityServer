using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a line and column number in a source file.
    /// </summary>
    internal struct SourceCodePosition
    {
        /// <summary>
        /// Creates a new SourceCodePosition instance.
        /// </summary>
        /// <param name="line"> The line number. Must be greater than zero. </param>
        /// <param name="column"> The column number. Must be greater than zero. </param>
        public SourceCodePosition(int line, int column)
            : this()
        {
            if (line < 1)
                throw new ArgumentOutOfRangeException("line");
            if (column < 1)
                throw new ArgumentOutOfRangeException("column");

            this.Line = line;
            this.Column = column;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int Line
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the column number.
        /// </summary>
        public int Column
        {
            get;
            private set;
        }
    }
}
