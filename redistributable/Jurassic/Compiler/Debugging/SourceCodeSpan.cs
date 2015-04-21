using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a portion of source code.
    /// </summary>
    internal class SourceCodeSpan
    {
        /// <summary>
        /// Creates a new SourceCodeSpan instance.
        /// </summary>
        /// <param name="startLine"> The start line of this SourceCodeSpan. Must be greater than
        /// zero. </param>
        /// <param name="startColumn"> The start column of this SourceCodeSpan. Must be greater
        /// than zero. </param>
        /// <param name="endLine"> The end line of this SourceCodeSpan. Must be greater than
        /// <paramref name="startLine"/>. </param>
        /// <param name="endColumn"> The end column of this SourceCodeSpan. Must be greater than
        /// <paramref name="startColumn"/>. </param>
        public SourceCodeSpan(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine < 1)
                throw new ArgumentOutOfRangeException("startLine");
            if (startColumn < 1)
                throw new ArgumentOutOfRangeException("startColumn");
            if (endLine < startLine)
                throw new ArgumentOutOfRangeException("endLine");
            if (endColumn < startColumn)
                throw new ArgumentOutOfRangeException("endColumn");

            this.StartLine = startLine;
            this.StartColumn = startColumn;
            this.EndLine = endLine;
            this.EndColumn = endColumn;
        }

        /// <summary>
        /// Creates a new SourceCodeSpan instance.
        /// </summary>
        /// <param name="start"> The start line and column of this SourceCodeSpan. </param>
        /// <param name="end"> The end line and column of this SourceCodeSpan. </param>
        public SourceCodeSpan(SourceCodePosition start, SourceCodePosition end)
        {
            this.StartLine = start.Line;
            this.StartColumn = start.Column;
            this.EndLine = end.Line;
            this.EndColumn = end.Column;
        }

        /// <summary>
        /// Gets the starting line number of this range.
        /// </summary>
        public int StartLine
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the starting column number of this range.
        /// </summary>
        public int StartColumn
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ending line number of this range.
        /// </summary>
        public int EndLine
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ending column number of this range.
        /// </summary>
        public int EndColumn
        {
            get;
            private set;
        }
    }
}
