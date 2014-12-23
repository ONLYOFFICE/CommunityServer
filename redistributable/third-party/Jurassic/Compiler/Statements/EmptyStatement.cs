using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents an empty statement.
    /// </summary>
    internal class EmptyStatement : Statement
    {
        /// <summary>
        /// Creates a new EmptyStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public EmptyStatement(IList<string> labels)
            : base(labels)
        {
        }

        /// <summary>
        /// Generates CIL for the statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals() { NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, optimizationInfo, statementLocals);

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, optimizationInfo, statementLocals);
        }

        /// <summary>
        /// Converts the statement to a string.
        /// </summary>
        /// <param name="indentLevel"> The number of tabs to include before the statement. </param>
        /// <returns> A string representing this statement. </returns>
        public override string ToString(int indentLevel)
        {
            return new string('\t', indentLevel) + ";";
        }
    }

}