using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a function expression.
    /// </summary>
    internal sealed class FunctionExpression : Expression
    {
        private FunctionMethodGenerator context;

        /// <summary>
        /// Creates a new instance of FunctionExpression.
        /// </summary>
        /// <param name="functionContext"> The function context to base this expression on. </param>
        public FunctionExpression(FunctionMethodGenerator functionContext)
        {
            if (functionContext == null)
                throw new ArgumentNullException("functionContext");
            this.context = functionContext;
        }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string FunctionName
        {
            get { return this.context.Name; }
        }

        /// <summary>
        /// Gets a list of argument names.
        /// </summary>
        public IList<string> ArgumentNames
        {
            get { return this.context.ArgumentNames; }
        }

        /// <summary>
        /// Gets the source code for the body of the function.
        /// </summary>
        public string BodyText
        {
            get { return this.context.BodyText; }
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get { return PrimitiveType.Object; }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Generate a new method.
            this.context.GenerateCode();

            // Add the generated method to the nested function list.
            if (optimizationInfo.NestedFunctions == null)
                optimizationInfo.NestedFunctions = new List<GeneratedMethod>();
            optimizationInfo.NestedFunctions.Add(this.context.GeneratedMethod);

            // Add all the nested methods to the parent list.
            if (this.context.GeneratedMethod.Dependencies != null)
            {
                foreach (var nestedFunctionExpression in this.context.GeneratedMethod.Dependencies)
                    optimizationInfo.NestedFunctions.Add(nestedFunctionExpression);
            }

            // Store the generated method in the cache.
            long generatedMethodID = GeneratedMethod.Save(this.context.GeneratedMethod);

            // Create a UserDefinedFunction.

            // prototype
            EmitHelpers.LoadScriptEngine(generator);
            generator.Call(ReflectionHelpers.ScriptEngine_Function);
            generator.Call(ReflectionHelpers.FunctionInstance_InstancePrototype);

            // name
            generator.LoadString(this.FunctionName);

            // argumentNames
            generator.LoadInt32(this.ArgumentNames.Count);
            generator.NewArray(typeof(string));
            for (int i = 0; i < this.ArgumentNames.Count; i++)
            {
                generator.Duplicate();
                generator.LoadInt32(i);
                generator.LoadString(this.ArgumentNames[i]);
                generator.StoreArrayElement(typeof(string));
            }

            // scope
            EmitHelpers.LoadScope(generator);

            // bodyText
            generator.LoadString(this.BodyText);

            // body
            generator.LoadInt64(generatedMethodID);
            generator.Call(ReflectionHelpers.GeneratedMethod_Load);


            // strictMode
            generator.LoadBoolean(this.context.StrictMode);

            // new UserDefinedFunction(ObjectInstance prototype, string name, IList<string> argumentNames, DeclarativeScope scope, Func<Scope, object, object[], object> body, bool strictMode)
            generator.NewObject(ReflectionHelpers.UserDefinedFunction_Constructor);
        }

        
        /// <summary>
        /// Generates CIL to set the display name of the function.  The function should be on top of the stack.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="displayName"> The display name of the function. </param>
        /// <param name="force"> <c>true</c> to set the displayName property, even if the function has a name already. </param>
        public void GenerateDisplayName(ILGenerator generator, OptimizationInfo optimizationInfo, string displayName, bool force)
        {
            if (displayName == null)
                throw new ArgumentNullException("displayName");

            // We only infer names for functions if the function doesn't have a name.
            if (force == true || string.IsNullOrEmpty(this.FunctionName))
            {
                // Statically set the display name.
                this.context.DisplayName = displayName;

                // Generate code to set the display name at runtime.
                generator.Duplicate();
                generator.LoadString("displayName");
                generator.LoadString(displayName);
                generator.LoadBoolean(false);
                generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValue_String);
            }
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            return string.Format("function {0}({1}) {{\n{2}\n}}", this.FunctionName, StringHelpers.Join(", ", this.ArgumentNames), this.BodyText);
        }
    }

}