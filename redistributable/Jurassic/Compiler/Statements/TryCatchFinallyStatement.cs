using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a try-catch-finally statement.
    /// </summary>
    internal class TryCatchFinallyStatement : Statement
    {
        /// <summary>
        /// Creates a new TryCatchFinallyStatement instance.
        /// </summary>
        public TryCatchFinallyStatement(IList<string> labels)
            : base(labels)
        {
        }

        /// <summary>
        /// Gets or sets the statement(s) inside the try block.
        /// </summary>
        public BlockStatement TryBlock
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the statement(s) inside the catch block.  Can be <c>null</c>.
        /// </summary>
        public BlockStatement CatchBlock
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the scope of the variable to receive the exception.  Can be <c>null</c> but
        /// only if CatchStatement is also <c>null</c>.
        /// </summary>
        public Scope CatchScope
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the variable to receive the exception.  Can be <c>null</c> but
        /// only if CatchStatement is also <c>null</c>.
        /// </summary>
        public string CatchVariableName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the statement(s) inside the finally block.  Can be <c>null</c>.
        /// </summary>
        public BlockStatement FinallyBlock
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
            var statementLocals = new StatementLocals() { NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, optimizationInfo, statementLocals);

            // Unlike in .NET, in javascript there are no restrictions on what can appear inside
            // try, catch and finally blocks.  The one restriction which causes problems is the
            // inability to jump out of .NET finally blocks.  This is required when break, continue
            // or return statements appear inside of a finally block.  To work around this, when
            // inside a finally block these instructions throw an exception instead.

            // Setting the InsideTryCatchOrFinally flag converts BR instructions into LEAVE
            // instructions so that the finally block is executed correctly.
            var previousInsideTryCatchOrFinally = optimizationInfo.InsideTryCatchOrFinally;
            optimizationInfo.InsideTryCatchOrFinally = true;

            // Finally requires two exception nested blocks.
            if (this.FinallyBlock != null)
                generator.BeginExceptionBlock();

            // Begin the exception block.
            generator.BeginExceptionBlock();

            // Generate code for the try block.
            this.TryBlock.GenerateCode(generator, optimizationInfo);

            // Generate code for the catch block.
            if (this.CatchBlock != null)
            {
                // Begin a catch block.  The exception is on the top of the stack.
                generator.BeginCatchBlock(typeof(JavaScriptException));

                // Create a new DeclarativeScope.
                this.CatchScope.GenerateScopeCreation(generator, optimizationInfo);

                // Store the error object in the variable provided.
                generator.Call(ReflectionHelpers.JavaScriptException_ErrorObject);
                var catchVariable = new NameExpression(this.CatchScope, this.CatchVariableName);
                catchVariable.GenerateSet(generator, optimizationInfo, PrimitiveType.Any, false);

                // Make sure the scope is reverted even if an exception is thrown.
                generator.BeginExceptionBlock();

                // Emit code for the statements within the catch block.
                this.CatchBlock.GenerateCode(generator, optimizationInfo);

                // Revert the scope.
                generator.BeginFinallyBlock();
                this.CatchScope.GenerateScopeDestruction(generator, optimizationInfo);
                generator.EndExceptionBlock();
            }

            // Generate code for the finally block.
            if (this.FinallyBlock != null)
            {
                generator.BeginFinallyBlock();

                var branches = new List<ILLabel>();
                var previousStackSize = optimizationInfo.LongJumpStackSizeThreshold;
                optimizationInfo.LongJumpStackSizeThreshold = optimizationInfo.BreakOrContinueStackSize;
                var previousCallback = optimizationInfo.LongJumpCallback;
                optimizationInfo.LongJumpCallback = (generator2, label) =>
                    {
                        // It is not possible to branch out of a finally block - therefore instead of
                        // generating LEAVE instructions we throw an exception then catch it to transfer
                        // control out of the finally block.
                        generator2.LoadInt32(branches.Count);
                        generator2.NewObject(ReflectionHelpers.LongJumpException_Constructor);
                        generator2.Throw();

                        // Record any branches that are made within the finally code.
                        branches.Add(label);
                    };

                // Emit code for the finally block.
                this.FinallyBlock.GenerateCode(generator, optimizationInfo);

                // End the main exception block.
                generator.EndExceptionBlock();

                // Begin a catch block to catch any LongJumpExceptions. The exception object is on
                // the top of the stack.
                generator.BeginCatchBlock(typeof(LongJumpException));

                if (branches.Count > 0)
                {
                    // switch (exception.RouteID)
                    // {
                    //    case 0: goto label1;
                    //    case 1: goto label2;
                    // }
                    ILLabel[] switchLabels = new ILLabel[branches.Count];
                    for (int i = 0; i < branches.Count; i++)
                        switchLabels[i] = generator.CreateLabel();
                    generator.Call(ReflectionHelpers.LongJumpException_RouteID);
                    generator.Switch(switchLabels);
                    for (int i = 0; i < branches.Count; i++)
                    {
                        generator.DefineLabelPosition(switchLabels[i]);
                        generator.Leave(branches[i]);
                    }
                }

                // Reset the state we clobbered.
                optimizationInfo.LongJumpStackSizeThreshold = previousStackSize;
                optimizationInfo.LongJumpCallback = previousCallback;
            }

            // End the exception block.
            generator.EndExceptionBlock();

            // Reset the InsideTryCatchOrFinally flag.
            optimizationInfo.InsideTryCatchOrFinally = previousInsideTryCatchOrFinally;

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
                yield return this.TryBlock;
                if (this.CatchBlock != null)
                    yield return this.CatchBlock;
                if (this.FinallyBlock != null)
                    yield return this.FinallyBlock;
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
            result.AppendLine("try");
            result.AppendLine(this.TryBlock.ToString(indentLevel + 1));
            if (this.CatchBlock != null)
            {
                result.Append(new string('\t', indentLevel));
                result.Append("catch (");
                result.Append(this.CatchVariableName);
                result.AppendLine(")");
                result.AppendLine(this.CatchBlock.ToString(indentLevel + 1));
            }
            if (this.FinallyBlock != null)
            {
                result.Append(new string('\t', indentLevel));
                result.AppendLine("finally");
                result.AppendLine(this.FinallyBlock.ToString(indentLevel + 1));
            }
            return result.ToString();
        }
    }

}