using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript for statement (for-in is a separate statement).
    /// </summary>
    internal class ForStatement : LoopStatement
    {
        /// <summary>
        /// Creates a new ForStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public ForStatement(IList<string> labels)
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
            result.AppendFormat("for ({0} {1} {2})",
                this.InitStatement == null ? ";" : this.InitStatement.ToString(0),
                this.ConditionStatement == null ? ";" : this.ConditionStatement.ToString(),
                this.IncrementStatement == null ? string.Empty : this.Increment.ToString());
            result.AppendLine();
            result.Append(this.Body.ToString(indentLevel + 1));
            return result.ToString();
        }
    }

}