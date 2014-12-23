using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript do-while statement.
    /// </summary>
    internal class DoWhileStatement : LoopStatement
    {
        /// <summary>
        /// Creates a new DoWhileStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public DoWhileStatement(IList<string> labels)
            : base(labels)
        {
        }

        /// <summary>
        /// Gets a value that indicates whether the condition should be checked at the end of the
        /// loop.
        /// </summary>
        protected override bool CheckConditionAtEnd
        {
            get { return true; }
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
            result.AppendLine("do");
            result.Append(this.Body.ToString(indentLevel + 1));
            result.Append(" while (");
            result.Append(this.Condition);
            result.Append(")");
            return result.ToString();
        }
    }

}