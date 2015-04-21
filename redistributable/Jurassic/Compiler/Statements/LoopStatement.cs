using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript loop statement (for, for-in, while and do-while).
    /// </summary>
    internal abstract class LoopStatement : Statement
    {
        /// <summary>
        /// Creates a new LoopStatement instance.
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        public LoopStatement(IList<string> labels)
            : base(labels)
        {
        }

        /// <summary>
        /// Gets or sets the statement that initializes the loop variable.
        /// </summary>
        public Statement InitStatement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the var statement that initializes the loop variable.
        /// </summary>
        public VarStatement InitVarStatement
        {
            get { return this.InitStatement as VarStatement; }
        }

        /// <summary>
        /// Gets the expression that initializes the loop variable.
        /// </summary>
        public Expression InitExpression
        {
            get { return (this.InitStatement is ExpressionStatement) == true ? ((ExpressionStatement)this.InitStatement).Expression : null; }
        }

        /// <summary>
        /// Gets or sets the statement that checks whether the loop should terminate.
        /// </summary>
        public ExpressionStatement ConditionStatement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the expression that checks whether the loop should terminate.
        /// </summary>
        public Expression Condition
        {
            get { return this.ConditionStatement.Expression; }
        }

        /// <summary>
        /// Gets or sets the statement that increments (or decrements) the loop variable.
        /// </summary>
        public ExpressionStatement IncrementStatement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the expression that increments (or decrements) the loop variable.
        /// </summary>
        public Expression Increment
        {
            get { return this.IncrementStatement.Expression; }
        }

        /// <summary>
        /// Gets or sets the loop body.
        /// </summary>
        public Statement Body
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value that indicates whether the condition should be checked at the end of the
        /// loop.
        /// </summary>
        protected virtual bool CheckConditionAtEnd
        {
            get { return false; }
        }

        private struct RevertInfo
        {
            public PrimitiveType Type;
            public ILLocalVariable Variable;
        }

        /// <summary>
        /// Generates CIL for the statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals() { NonDefaultBreakStatementBehavior = true, NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, optimizationInfo, statementLocals);

            // <initializer>
            // if (<condition>)
            // {
            // 	 <loop body>
            //   <increment>
            //   while (true) {
            //     if (<condition> == false)
            //       break;
            //
            //     <body statements>
            //
            //     continue-target:
            //     <increment>
            //   }
            // }
            // break-target:

            // Set up some labels.
            var continueTarget = generator.CreateLabel();
            var breakTarget1 = generator.CreateLabel();
            var breakTarget2 = generator.CreateLabel();

            // Emit the initialization statement.
            if (this.InitStatement != null)
                this.InitStatement.GenerateCode(generator, optimizationInfo);

            // Check the condition and jump to the end if it is false.
            if (this.CheckConditionAtEnd == false && this.ConditionStatement != null)
            {
                optimizationInfo.MarkSequencePoint(generator, this.ConditionStatement.SourceSpan);
                this.Condition.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToBool(generator, this.Condition.ResultType);
                generator.BranchIfFalse(breakTarget1);
            }

            // Emit the loop body.
            optimizationInfo.PushBreakOrContinueInfo(this.Labels, breakTarget1, continueTarget, false);
            this.Body.GenerateCode(generator, optimizationInfo);
            optimizationInfo.PopBreakOrContinueInfo();

            // Increment the loop variable.
            if (this.IncrementStatement != null)
                this.IncrementStatement.GenerateCode(generator, optimizationInfo);

            // Strengthen the variable types.
            List<KeyValuePair<Scope.DeclaredVariable, RevertInfo>> previousVariableTypes = null;
            var previousInsideTryCatchOrFinally = optimizationInfo.InsideTryCatchOrFinally;
            if (optimizationInfo.OptimizeInferredTypes == true)
            {
                // Keep a record of the variable types before strengthening.
                previousVariableTypes = new List<KeyValuePair<Scope.DeclaredVariable, RevertInfo>>();

                var typedVariables = FindTypedVariables();
                foreach (var variableAndType in typedVariables)
                {
                    var variable = variableAndType.Key;
                    var variableInfo = variableAndType.Value;
                    if (variableInfo.Conditional == false && variableInfo.Type != variable.Type)
                    {
                        // Save the previous type so we can restore it later.
                        var previousType = variable.Type;
                        previousVariableTypes.Add(new KeyValuePair<Scope.DeclaredVariable, RevertInfo>(variable, new RevertInfo() { Type = previousType, Variable = variable.Store }));

                        // Load the existing value.
                        var nameExpression = new NameExpression(variable.Scope, variable.Name);
                        nameExpression.GenerateGet(generator, optimizationInfo, false);

                        // Store the typed value.
                        variable.Store = generator.DeclareVariable(variableInfo.Type);
                        variable.Type = variableInfo.Type;
                        nameExpression.GenerateSet(generator, optimizationInfo, previousType, false);
                    }
                }

                // The variables must be reverted even in the presence of exceptions.
                if (previousVariableTypes.Count > 0)
                {
                    generator.BeginExceptionBlock();

                    // Setting the InsideTryCatchOrFinally flag converts BR instructions into LEAVE
                    // instructions so that the finally block is executed correctly.
                    optimizationInfo.InsideTryCatchOrFinally = true;
                }
            }

            // The inner loop starts here.
            var startOfLoop = generator.DefineLabelPosition();

            // Check the condition and jump to the end if it is false.
            if (this.ConditionStatement != null)
            {
                optimizationInfo.MarkSequencePoint(generator, this.ConditionStatement.SourceSpan);
                this.Condition.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToBool(generator, this.Condition.ResultType);
                generator.BranchIfFalse(breakTarget2);
            }

            // Emit the loop body.
            optimizationInfo.PushBreakOrContinueInfo(this.Labels, breakTarget2, continueTarget, labelledOnly: false);
            this.Body.GenerateCode(generator, optimizationInfo);
            optimizationInfo.PopBreakOrContinueInfo();

            // The continue statement jumps here.
            generator.DefineLabelPosition(continueTarget);

            // Increment the loop variable.
            if (this.IncrementStatement != null)
                this.IncrementStatement.GenerateCode(generator, optimizationInfo);

            // Unconditionally branch back to the start of the loop.
            generator.Branch(startOfLoop);

            // Define the end of the loop (actually just after).
            generator.DefineLabelPosition(breakTarget2);

            // Revert the variable types.
            if (previousVariableTypes != null && previousVariableTypes.Count > 0)
            {
                // Revert the InsideTryCatchOrFinally flag.
                optimizationInfo.InsideTryCatchOrFinally = previousInsideTryCatchOrFinally;

                // Revert the variable types within a finally block.
                generator.BeginFinallyBlock();

                foreach (var previousVariableAndType in previousVariableTypes)
                {
                    var variable = previousVariableAndType.Key;
                    var variableRevertInfo = previousVariableAndType.Value;

                    // Load the existing value.
                    var nameExpression = new NameExpression(variable.Scope, variable.Name);
                    nameExpression.GenerateGet(generator, optimizationInfo, false);

                    // Store the typed value.
                    var previousType = variable.Type;
                    variable.Store = variableRevertInfo.Variable;
                    variable.Type = variableRevertInfo.Type;
                    nameExpression.GenerateSet(generator, optimizationInfo, previousType, false);
                }

                // End the exception block.
                generator.EndExceptionBlock();
            }

            // Define the end of the loop (actually just after).
            generator.DefineLabelPosition(breakTarget1);

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, optimizationInfo, statementLocals);
        }

        private struct InferredTypeInfo
        {
            // The inferred type of the variable.
            public PrimitiveType Type;

            // <c>true</c> if all the variable assignments are conditional (type inference cannot
            // be used in this case).
            public bool Conditional;
        }

        /// <summary>
        /// Finds variables that were assigned to and determines their types.
        /// </summary>
        /// <param name="root"> The root of the abstract syntax tree to search. </param>
        /// <param name="variableTypes"> A dictionary containing the variables that were assigned to. </param>
        private Dictionary<Scope.DeclaredVariable, InferredTypeInfo> FindTypedVariables()
        {
            var result = new Dictionary<Scope.DeclaredVariable, InferredTypeInfo>();

            // Special case for the common case of an incrementing or decrementing loop variable.
            // The loop must be one of the following forms:
            //     for (var i = <int>; i < <int>; i ++)
            //     for (var i = <int>; i < <int>; ++ i)
            //     for (var i = <int>; i > <int>; i --)
            //     for (var i = <int>; i > <int>; -- i)
            //     for (i = <int>; i < <int>; i ++)
            //     for (i = <int>; i < <int>; ++ i)
            //     for (i = <int>; i > <int>; i --)
            //     for (i = <int>; i > <int>; -- i)
            Scope loopVariableScope = null;
            string loopVariableName = null;

            // First, check the init statement.
            bool initIsOkay = false;
            if (this.InitVarStatement != null &&
                this.InitVarStatement.Declarations.Count == 1 &&
                this.InitVarStatement.Declarations[0].InitExpression != null &&
                this.InitVarStatement.Declarations[0].InitExpression.ResultType == PrimitiveType.Int32)
            {
                // for (var i = <int>; ?; ?)
                loopVariableScope = this.InitVarStatement.Scope;
                loopVariableName = this.InitVarStatement.Declarations[0].VariableName;
                initIsOkay = loopVariableScope is DeclarativeScope && loopVariableScope.HasDeclaredVariable(loopVariableName) == true;
            }
            else if (this.InitExpression != null &&
                this.InitExpression is AssignmentExpression &&
                ((AssignmentExpression)this.InitExpression).ResultType == PrimitiveType.Int32 &&
                ((AssignmentExpression)this.InitExpression).Target is NameExpression)
            {
                // for (i = <int>; ?; ?)
                loopVariableScope = ((NameExpression)((AssignmentExpression)this.InitExpression).Target).Scope;
                loopVariableName = ((NameExpression)((AssignmentExpression)this.InitExpression).Target).Name;
                initIsOkay = loopVariableScope is DeclarativeScope && loopVariableScope.HasDeclaredVariable(loopVariableName) == true;
            }

            // Next, check the condition expression.
            bool conditionAndInitIsOkay = false;
            bool lessThan = true;
            if (initIsOkay == true &&
                this.ConditionStatement != null &&
                this.Condition is BinaryExpression &&
                (((BinaryExpression)this.Condition).OperatorType == OperatorType.LessThan ||
                ((BinaryExpression)this.Condition).OperatorType == OperatorType.GreaterThan) &&
                ((BinaryExpression)this.Condition).Left is NameExpression &&
                ((NameExpression)((BinaryExpression)this.Condition).Left).Name == loopVariableName &&
                ((BinaryExpression)this.Condition).Right.ResultType == PrimitiveType.Int32)
            {
                // for (?; i < <int>; ?)
                // for (?; i > <int>; ?)
                lessThan = ((BinaryExpression)this.Condition).OperatorType == OperatorType.LessThan;
                conditionAndInitIsOkay = true;
            }

            // Next, check the increment expression.
            bool everythingIsOkay = false;
            if (conditionAndInitIsOkay == true)
            {
                if (lessThan == true &&
                    this.IncrementStatement != null &&
                    this.Increment is AssignmentExpression &&
                    (((AssignmentExpression)this.Increment).OperatorType == OperatorType.PostIncrement ||
                    ((AssignmentExpression)this.Increment).OperatorType == OperatorType.PreIncrement) &&
                    ((NameExpression)((AssignmentExpression)this.Increment).Target).Name == loopVariableName)
                {
                    // for (?; i < <int>; i ++)
                    // for (?; i < <int>; ++ i)
                    everythingIsOkay = true;
                }
                else if (lessThan == false &&
                    this.IncrementStatement != null &&
                    this.Increment is AssignmentExpression &&
                    (((AssignmentExpression)this.Increment).OperatorType == OperatorType.PostDecrement ||
                    ((AssignmentExpression)this.Increment).OperatorType == OperatorType.PreDecrement) &&
                    ((NameExpression)((AssignmentExpression)this.Increment).Target).Name == loopVariableName)
                {
                    // for (?; i > <int>; i --)
                    // for (?; i > <int>; -- i)
                    everythingIsOkay = true;
                }
            }

            bool continueEncountered = false;
            if (everythingIsOkay == true)
            {
                // The loop variable can be optimized to an integer.
                var variable = loopVariableScope.GetDeclaredVariable(loopVariableName);
                if (variable != null)
                    result.Add(variable, new InferredTypeInfo() { Type = PrimitiveType.Int32, Conditional = false });
                FindTypedVariables(this.Body, result, conditional: false, continueEncountered: ref continueEncountered);
            }
            else
            {
                // Unoptimized.
                FindTypedVariables(this, result, conditional: false, continueEncountered: ref continueEncountered);
            }

            return result;
        }

        /// <summary>
        /// Finds variables that were assigned to and determines their types.
        /// </summary>
        /// <param name="root"> The root of the abstract syntax tree to search. </param>
        /// <param name="variableTypes"> A dictionary containing the variables that were assigned to. </param>
        /// <param name="conditional"> <c>true</c> if execution of the AST node <paramref name="root"/>
        /// is conditional (i.e. the node is inside an if statement or a conditional expression. </param>
        /// <param name="continueEncountered"> Keeps track of whether a continue statement has been
        /// encountered. </param>
        private static void FindTypedVariables(AstNode root, Dictionary<Scope.DeclaredVariable, InferredTypeInfo> variableTypes, bool conditional, ref bool continueEncountered)
        {
            if (root is AssignmentExpression)
            {
                // Found an assignment.
                var assignment = (AssignmentExpression)root;
                if (assignment.Target is NameExpression)
                {
                    // Found an assignment to a variable.
                    var name = (NameExpression)assignment.Target;
                    if (name.Scope is DeclarativeScope)
                    {
                        var variable = name.Scope.GetDeclaredVariable(name.Name);
                        if (variable != null)
                        {
                            // The variable is in the top-most scope.
                            // Check if the variable has been seen before.
                            InferredTypeInfo existingTypeInfo;
                            if (variableTypes.TryGetValue(variable, out existingTypeInfo) == false)
                            {
                                // This is the first time the variable has been encountered.
                                variableTypes.Add(variable, new InferredTypeInfo() { Type = assignment.ResultType, Conditional = conditional });
                            }
                            else
                            {
                                // The variable has been seen before.
                                variableTypes[variable] = new InferredTypeInfo()
                                {
                                    Type = PrimitiveTypeUtilities.GetCommonType(existingTypeInfo.Type, assignment.ResultType),
                                    Conditional = existingTypeInfo.Conditional == true && conditional == true
                                };
                            }
                        }
                    }
                }
            }
            
            // Determine whether the child nodes are conditional.
            conditional = conditional == true ||
                continueEncountered == true ||
                root is IfStatement ||
                root is TernaryExpression ||
                root is TryCatchFinallyStatement ||
                (root is BinaryExpression && ((BinaryExpression)root).OperatorType == OperatorType.LogicalAnd) ||
                (root is BinaryExpression && ((BinaryExpression)root).OperatorType == OperatorType.LogicalOr);

            // If the AST node is a continue statement, all further assignments are conditional.
            if (root is ContinueStatement)
                continueEncountered = true;

            // Search child nodes for assignment statements.
            foreach (var node in root.ChildNodes)
                FindTypedVariables(node, variableTypes, conditional: conditional, continueEncountered: ref continueEncountered);
        }

        /// <summary>
        /// Gets an enumerable list of child nodes in the abstract syntax tree.
        /// </summary>
        public override IEnumerable<AstNode> ChildNodes
        {
            get
            {
                if (this.InitStatement != null)
                    yield return this.InitStatement;
                if (this.CheckConditionAtEnd == false && this.ConditionStatement != null)
                    yield return this.ConditionStatement;
                yield return this.Body;
                if (this.IncrementStatement != null)
                    yield return this.IncrementStatement;
                if (this.CheckConditionAtEnd == true && this.ConditionStatement != null)
                    yield return this.ConditionStatement;
            }
        }
    }

}