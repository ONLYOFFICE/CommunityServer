using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript while statement.
    /// </summary>
    internal class WhileStatement : LoopStatement
    {
        /// <summary>
        /// Creates a new WhileStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public WhileStatement(IList<string> labels)
            : base(labels)
        {
        }

        /// <summary>
        /// Converts the statement to a string.
        /// </summary>
        /// <param name="indentLevel"> The number of tabs to include before the statement. </param>
        /// <returns> A string representing this statement. </returns>
        public override string ToString(int indentLevel)
        {
            var result = new System.Text.StringBuilder();
            result.Append(new string('\t', indentLevel));
            result.Append("while (");
            result.Append(this.Condition);
            result.AppendLine(")");
            result.Append(this.Body.ToString(indentLevel + 1));
            return result.ToString();
        }
    }

}