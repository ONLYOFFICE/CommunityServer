using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript switch statement.
    /// </summary>
    internal class SwitchStatement : Statement
    {
        /// <summary>
        /// Creates a new SwitchStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public SwitchStatement(IList<string> labels)
            : base(labels)
        {
            this.CaseClauses = new List<SwitchCase>(5);
        }

        /// <summary>
        /// Gets or sets an expression that represents the value to switch on.
        /// </summary>
        public Expression Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a list of case clauses (including the default clause).
        /// </summary>
        public IList<SwitchCase> CaseClauses
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates CIL for the statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals() { NonDefaultBreakStatementBehavior = true };
            GenerateStartOfStatement(generator, optimizationInfo, statementLocals);

            // We need a label for each case clause and one for the default case.
            var jumpTargets = new ILLabel[this.CaseClauses.Count];
            int defaultIndex = -1;
            ILLabel endOfSwitch = generator.CreateLabel();

            // Generate code for the switch value.
            var startOfSwitch = generator.CreateLabel();
            this.Value.GenerateCode(generator, optimizationInfo);
            EmitConversion.ToAny(generator, this.Value.ResultType);

            // Save the switch value in a variable.
            var switchValue = generator.CreateTemporaryVariable(typeof(object));
            generator.StoreVariable(switchValue);

            for (int i = 0; i < this.CaseClauses.Count; i ++)
            {
                var caseClause = this.CaseClauses[i];

                // Create a label for each clause.
                jumpTargets[i] = generator.CreateLabel();

                if (caseClause.Value == null)
                {
                    // This is a default clause.
                    defaultIndex = i;
                    continue;
                }

                // TypeComparer.StrictEquals(switchValue, caseValue)
                generator.LoadVariable(switchValue);
                caseClause.Value.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToAny(generator, caseClause.Value.ResultType);
                generator.Call(ReflectionHelpers.TypeComparer_StrictEquals);

                // if (TypeComparer.StrictEquals(switchValue, caseValue) == true)
                //     goto case i
                generator.BranchIfTrue(jumpTargets[i]);
            }
            
            // None of the cases matched, jump to the default clause or the end of the switch.
            if (defaultIndex >= 0)
                generator.Branch(jumpTargets[defaultIndex]);
            else
                generator.Branch(endOfSwitch);

            for (int i = 0; i < this.CaseClauses.Count; i++)
            {
                // Define a label at the start of the case clause.
                generator.DefineLabelPosition(jumpTargets[i]);

                // Set up the information needed by the break statement.
                optimizationInfo.PushBreakOrContinueInfo(this.Labels, endOfSwitch, null, labelledOnly: false);

                // Emit the case clause statements.
                foreach (var statement in this.CaseClauses[i].BodyStatements)
                    statement.GenerateCode(generator, optimizationInfo);

                // Revert the information needed by the break statement.
                optimizationInfo.PopBreakOrContinueInfo();
            }

            // Define a label for the end of the switch statement.
            generator.DefineLabelPosition(endOfSwitch);

            // Release the switch value variable for use elsewhere.
            generator.ReleaseTemporaryVariable(switchValue);

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
                yield return this.Value;
                foreach (var clause in this.CaseClauses)
                {
                    if (clause.Value != null)
                        yield return clause.Value;
                    foreach (var statement in clause.BodyStatements)
                        yield return statement;
                }
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
            result.Append("switch (");
            result.Append(this.Value);
            result.AppendLine(")");
            result.Append(new string('\t', indentLevel));
            result.AppendLine("{");
            indentLevel++;
            foreach (var caseClause in this.CaseClauses)
            {
                result.Append(new string('\t', indentLevel));
                if (caseClause.Value != null)
                {
                    result.Append("case ");
                    result.Append(caseClause.Value);
                    result.AppendLine(":");
                }
                else
                {
                    result.AppendLine("default:");
                }
                foreach (var statement in caseClause.BodyStatements)
                    result.AppendLine(statement.ToString(indentLevel + 1));
            }
            indentLevel--;
            result.Append(new string('\t', indentLevel));
            result.Append("}");
            return result.ToString();
        }
    }

    /// <summary>
    /// Represents a single case statement inside a switch statement.
    /// </summary>
    internal class SwitchCase
    {
        public SwitchCase()
        {
            this.BodyStatements = new List<Statement>();
        }

        /// <summary>
        /// Gets or sets an expression that must match the switch value for the case to execute.
        /// A value of <c>null</c> indicates this is the default clause.
        /// </summary>
        public Expression Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a list of the statement(s) in the body of the case statement.
        /// </summary>
        public IList<Statement> BodyStatements
        {
            get;
            private set;
        }
    }
}