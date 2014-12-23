using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a function call expression.
    /// </summary>
    internal class FunctionCallExpression : OperatorExpression
    {
        /// <summary>
        /// Creates a new instance of FunctionCallJSExpression.
        /// </summary>
        /// <param name="operator"> The binary operator to base this expression on. </param>
        public FunctionCallExpression(Operator @operator)
            : base(@operator)
        {
        }

        /// <summary>
        /// Gets an expression that evaluates to the function instance.
        /// </summary>
        public Expression Target
        {
            get { return this.GetOperand(0); }
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get { return PrimitiveType.Any; }
        }

        /// <summary>
        /// Used to implement function calls without evaluating the left operand twice.
        /// </summary>
        private class TemporaryVariableExpression : Expression
        {
            private ILLocalVariable variable;

            public TemporaryVariableExpression(ILLocalVariable variable)
            {
                this.variable = variable;
            }

            public override PrimitiveType ResultType
            {
                get { return PrimitiveType.Any; }
            }

            public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
            {
                generator.LoadVariable(this.variable);
            }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Check if this is a direct call to eval().
            if (this.Target is NameExpression && ((NameExpression)this.Target).Name == "eval")
            {
                GenerateEval(generator, optimizationInfo);
                return;
            }

            // Emit the function instance first.
            ILLocalVariable targetBase = null;
            if (this.Target is MemberAccessExpression)
            {
                // The function is a member access expression (e.g. "Math.cos()").

                // Evaluate the left part of the member access expression.
                var baseExpression = ((MemberAccessExpression)this.Target).Base;
                baseExpression.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToAny(generator, baseExpression.ResultType);
                targetBase = generator.CreateTemporaryVariable(typeof(object));
                generator.StoreVariable(targetBase);

                // Evaluate the right part of the member access expression.
                var memberAccessExpression = new MemberAccessExpression(((MemberAccessExpression)this.Target).Operator);
                memberAccessExpression.Push(new TemporaryVariableExpression(targetBase));
                memberAccessExpression.Push(((MemberAccessExpression)this.Target).GetOperand(1));
                memberAccessExpression.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToAny(generator, this.Target.ResultType);
            }
            else
            {
                // Something else (e.g. "eval()").
                this.Target.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToAny(generator, this.Target.ResultType);
            }

            // Check the object really is a function - if not, throw an exception.
            generator.IsInstance(typeof(Library.FunctionInstance));
            generator.Duplicate();
            var endOfTypeCheck = generator.CreateLabel();
            generator.BranchIfNotNull(endOfTypeCheck);

            // Throw an nicely formatted exception.
            generator.Pop();
            EmitHelpers.EmitThrow(generator, "TypeError", string.Format("'{0}' is not a function", this.Target.ToString()));
            generator.DefineLabelPosition(endOfTypeCheck);

            // Pass in the path, function name and line.
            generator.LoadStringOrNull(optimizationInfo.Source.Path);
            generator.LoadStringOrNull(optimizationInfo.FunctionName);
            generator.LoadInt32(optimizationInfo.SourceSpan.StartLine);

            // Generate code to produce the "this" value.  There are three cases.
            if (this.Target is NameExpression)
            {
                // 1. The function is a name expression (e.g. "parseInt()").
                //    In this case this = scope.ImplicitThisValue, if there is one, otherwise undefined.
                ((NameExpression)this.Target).GenerateThis(generator);
            }
            else if (this.Target is MemberAccessExpression)
            {
                // 2. The function is a member access expression (e.g. "Math.cos()").
                //    In this case this = Math.
                //var baseExpression = ((MemberAccessExpression)this.Target).Base;
                //baseExpression.GenerateCode(generator, optimizationInfo);
                //EmitConversion.ToAny(generator, baseExpression.ResultType);
                generator.LoadVariable(targetBase);
            }
            else
            {
                // 3. Neither of the above (e.g. "(function() { return 5 })()")
                //    In this case this = undefined.
                EmitHelpers.EmitUndefined(generator);
            }

            // Emit an array containing the function arguments.
            GenerateArgumentsArray(generator, optimizationInfo);

            // Call FunctionInstance.CallLateBound(thisValue, argumentValues)
            generator.Call(ReflectionHelpers.FunctionInstance_CallWithStackTrace);

            // Allow reuse of the temporary variable.
            if (targetBase != null)
                generator.ReleaseTemporaryVariable(targetBase);
        }

        /// <summary>
        /// Generates an array containing the argument values.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        internal void GenerateArgumentsArray(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Emit the arguments.  The arguments operand can be non-existant, a single expression,
            // or a comma-delimited list.
            if (this.OperandCount < 2)
            {
                // No parameters passed.  Create an empty array.
                generator.LoadInt32(0);
                generator.NewArray(typeof(object));
            }
            else
            {
                // One or more arguments.
                IList<Expression> arguments;
                var argumentsOperand = this.GetRawOperand(1);
                if (argumentsOperand is ListExpression)
                {
                    // Multiple parameters were passed to the function.
                    arguments = ((ListExpression)argumentsOperand).Items;
                }
                else
                {
                    // A single parameter was passed to the function.
                    arguments = new List<Expression>(1) { argumentsOperand };
                }

                // Generate an array containing the value of each argument.
                generator.LoadInt32(arguments.Count);
                generator.NewArray(typeof(object));
                for (int i = 0; i < arguments.Count; i++)
                {
                    generator.Duplicate();
                    generator.LoadInt32(i);
                    arguments[i].GenerateCode(generator, optimizationInfo);
                    EmitConversion.ToAny(generator, arguments[i].ResultType);
                    generator.StoreArrayElement(typeof(object));
                }
            }
        }

        /// <summary>
        /// Generates CIL for a call to eval().
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        private void GenerateEval(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // engine
            EmitHelpers.LoadScriptEngine(generator);

            // code
            if (this.OperandCount < 2)
            {
                // No arguments were supplied.
                generator.LoadNull();
            }
            else
            {
                // Take the first argument and convert it to a string.
                GenerateArgumentsArray(generator, optimizationInfo);
                generator.LoadInt32(0);
                generator.LoadArrayElement(typeof(object));
            }

            // scope
            EmitHelpers.LoadScope(generator);

            // thisObject
            EmitHelpers.LoadThis(generator);

            // strictMode
            generator.LoadBoolean(optimizationInfo.StrictMode);

            // Call Global.Eval(engine, code, scope, thisValue, strictMode)
            generator.Call(ReflectionHelpers.Global_Eval);
        }
    }

}