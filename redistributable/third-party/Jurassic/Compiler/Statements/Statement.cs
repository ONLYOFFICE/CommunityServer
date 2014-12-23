using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript statement.
    /// </summary>
    internal abstract class Statement : AstNode
    {
        private List<string> labels;

        /// <summary>
        /// Creates a new Statement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public Statement(IList<string> labels)
        {
            if (labels != null && labels.Count > 0)
                this.labels = new List<string>(labels);
        }

        /// <summary>
        /// Returns a value that indicates whether the statement has one or more labels attached to
        /// it.
        /// </summary>
        public bool HasLabels
        {
            get { return this.labels != null; }
        }

        /// <summary>
        /// Gets or sets the labels associated with this statement.
        /// </summary>
        public IList<string> Labels
        {
            get { return this.labels; }
        }

        /// <summary>
        /// Gets or sets the portion of source code associated with this statement.  For
        /// single-line statements this encompasses the whole statement but for multi-line (block)
        /// statements it only encompasses part of the statement.
        /// </summary>
        public SourceCodeSpan SourceSpan
        {
            get;
            set;
        }

        /// <summary>
        /// Locals needed by GenerateStartOfStatement() and GenerateEndOfStatement().
        /// </summary>
        public class StatementLocals
        {
            /// <summary>
            /// Gets or sets a value that indicates whether the break statement will be handled
            /// specially by the calling code - this means that GenerateStartOfStatement() and
            /// GenerateEndOfStatement() do not have to generate code to handle the break
            /// statement.
            /// </summary>
            public bool NonDefaultBreakStatementBehavior;

            /// <summary>
            /// Gets or sets a value that indicates whether the debugging information will be
            /// handled specially by the calling code - this means that GenerateStartOfStatement()
            /// and GenerateEndOfStatement() do not have to set this information.
            /// </summary>
            public bool NonDefaultSourceSpanBehavior;

            /// <summary>
            /// Gets or sets a label marking the end of the statement.
            /// </summary>
            public ILLabel EndOfStatement;

#if DEBUG && !SILVERLIGHT
            /// <summary>
            /// Gets or sets the number of items on the IL stack at the start of the statement.
            /// </summary>
            public int OriginalStackSize;
#endif
        }

        /// <summary>
        /// Generates CIL for the start of every statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        public void GenerateStartOfStatement(ILGenerator generator, OptimizationInfo optimizationInfo, StatementLocals locals)
        {
#if DEBUG && !SILVERLIGHT
            // Statements must not produce or consume any values on the stack.
            if (generator is DynamicILGenerator)
                locals.OriginalStackSize = ((DynamicILGenerator)generator).StackSize;
#endif

            if (locals.NonDefaultBreakStatementBehavior == false && this.HasLabels == true)
            {
                // Set up the information needed by the break statement.
                locals.EndOfStatement = generator.CreateLabel();
                optimizationInfo.PushBreakOrContinueInfo(this.Labels, locals.EndOfStatement, null, labelledOnly: true);
            }

            // Emit debugging information.
            if (locals.NonDefaultSourceSpanBehavior == false)
                optimizationInfo.MarkSequencePoint(generator, this.SourceSpan);
        }

        /// <summary>
        /// Generates CIL for the end of every statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        public void GenerateEndOfStatement(ILGenerator generator, OptimizationInfo optimizationInfo, StatementLocals locals)
        {
            if (locals.NonDefaultBreakStatementBehavior == false && this.HasLabels == true)
            {
                // Revert the information needed by the break statement.
                generator.DefineLabelPosition(locals.EndOfStatement);
                optimizationInfo.PopBreakOrContinueInfo();
            }

#if DEBUG && !SILVERLIGHT
            // Check that the stack count is zero.
            if (generator is DynamicILGenerator && ((DynamicILGenerator)generator).StackSize != locals.OriginalStackSize)
                throw new InvalidOperationException("Encountered unexpected stack imbalance.");
#endif
        }

        /// <summary>
        /// Converts the statement to a string.
        /// </summary>
        /// <param name="indentLevel"> The number of tabs to include before the statement. </param>
        /// <returns> A string representing this statement. </returns>
        public override string ToString()
        {
            return this.ToString(0);
        }

        /// <summary>
        /// Converts the statement to a string.
        /// </summary>
        /// <param name="indentLevel"> The number of tabs to include before the statement. </param>
        /// <returns> A string representing this statement. </returns>
        public abstract string ToString(int indentLevel);
    }

}