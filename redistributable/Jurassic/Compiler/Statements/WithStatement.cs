using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript with statement.
    /// </summary>
    internal class WithStatement : Statement
    {
        /// <summary>
        /// Creates a new WithStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public WithStatement(IList<string> labels)
            : base(labels)
        {
        }

        /// <summary>
        /// Gets or sets the object scope inside the with statement.
        /// </summary>
        public ObjectScope Scope
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the body of the with statement.
        /// </summary>
        public Statement Body
        {
            get;
            set;
        }

        /// <summary>
        /// Generates CIL for the statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, optimizationInfo, statementLocals);

            // Create the scope.
            this.Scope.GenerateScopeCreation(generator, optimizationInfo);

            // Make sure the scope is reverted even if an exception is thrown.
            generator.BeginExceptionBlock();

            // Setting the InsideTryCatchOrFinally flag converts BR instructions into LEAVE
            // instructions so that the finally block is executed correctly.
            var previousInsideTryCatchOrFinally = optimizationInfo.InsideTryCatchOrFinally;
            optimizationInfo.InsideTryCatchOrFinally = true;

            // Generate code for the body statements.
            this.Body.GenerateCode(generator, optimizationInfo);

            // Reset the InsideTryCatchOrFinally flag.
            optimizationInfo.InsideTryCatchOrFinally = previousInsideTryCatchOrFinally;

            // Revert the scope.
            generator.BeginFinallyBlock();
            this.Scope.GenerateScopeDestruction(generator, optimizationInfo);
            generator.EndExceptionBlock();

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, optimizationInfo, statementLocals);
        }

        /// <summary>
        /// Gets an enumerable list of child nodes in the abstract syntax tree.
        /// </summary>
        public override IEnumerable<AstNode> ChildNodes
        {
            get
            {
                yield return this.Scope.ScopeObjectExpression;
                yield return this.Body;
            }
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
            result.Append("with (");
            result.Append(this.Scope.ScopeObjectExpression);
            result.AppendLine(")");
            result.Append(this.Body.ToString(indentLevel + 1));
            return result.ToString();
        }
    }

}