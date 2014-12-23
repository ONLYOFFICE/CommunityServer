using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript var statement.
    /// </summary>
    internal class VarStatement : Statement
    {
        private List<VariableDeclaration> declarations;

        /// <summary>
        /// Creates a new VarStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        /// <param name="scope"> The scope the variables are defined within. </param>
        public VarStatement(IList<string> labels, Scope scope)
            : base(labels)
        {
            if (scope == null)
                throw new ArgumentNullException("scope");
            this.Scope = scope;
            this.declarations = new List<VariableDeclaration>(1);
        }

        /// <summary>
        /// Gets the scope the variables are defined within.
        /// </summary>
        public Scope Scope
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of variable declarations.
        /// </summary>
        public IList<VariableDeclaration> Declarations
        {
            get { return this.declarations; }
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

            foreach (var declaration in this.Declarations)
            {
                if (declaration.InitExpression != null)
                {
                    // Create a new assignment expression and generate code for it.
                    var initializationStatement = new ExpressionStatement(
                        new AssignmentExpression(this.Scope, declaration.VariableName, declaration.InitExpression));
                    initializationStatement.SourceSpan = declaration.SourceSpan;
                    initializationStatement.GenerateCode(generator, optimizationInfo);
                }
            }

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
                foreach (var declaration in this.Declarations)
                    if (declaration.InitExpression != null)
                        yield return new AssignmentExpression(this.Scope, declaration.VariableName, declaration.InitExpression);
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
            result.Append("var ");
            bool first = true;
            foreach (var declaration in this.Declarations)
            {
                if (first == false)
                    result.Append(", ");
                first = false;
                result.Append(declaration.VariableName);
                if (declaration.InitExpression != null)
                {
                    result.Append(" = ");
                    result.Append(declaration.InitExpression);
                }
            }
            result.Append(";");
            return result.ToString();
        }
    }

    /// <summary>
    /// Represents a variable declaration.
    /// </summary>
    internal class VariableDeclaration
    {
        /// <summary>
        /// Gets or sets the name of the variable that is being declared.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets the initial value of the variable.  Can be <c>null</c>.
        /// </summary>
        public Expression InitExpression { get; set; }

        /// <summary>
        /// Gets or sets the portion of source code associated with the declaration.
        /// </summary>
        public SourceCodeSpan SourceSpan { get; set; }
    }

}