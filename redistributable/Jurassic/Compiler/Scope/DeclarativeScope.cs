using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a scope where the variables are statically known.
    /// </summary>
    [Serializable]
    public class DeclarativeScope : Scope
    {
        // An array of values - one element for each variable declared in the scope.
        private object[] values;

        /// <summary>
        /// Creates a new declarative scope for use inside a function body.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        /// <param name="functionName"> The name of the function.  Can be empty for an anonymous function. </param>
        /// <param name="argumentNames"> The names of each of the function arguments. </param>
        /// <returns> A new DeclarativeScope instance. </returns>
        internal static DeclarativeScope CreateFunctionScope(Scope parentScope, string functionName, IEnumerable<string> argumentNames)
        {
            if (parentScope == null)
                throw new ArgumentNullException("parentScope", "Function scopes must have a parent scope.");
            if (functionName == null)
                throw new ArgumentNullException("functionName");
            if (argumentNames == null)
                throw new ArgumentNullException("argumentNames");
            var result = new DeclarativeScope(parentScope, 0);
            if (string.IsNullOrEmpty(functionName) == false)
                result.DeclareVariable(functionName);
            result.DeclareVariable("this");
            result.DeclareVariable("arguments");
            foreach (var argumentName in argumentNames)
                result.DeclareVariable(argumentName);
            return result;
        }

        /// <summary>
        /// Creates a new declarative scope for use inside a catch statement.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        /// <param name="catchVariableName"> The name of the catch variable. </param>
        /// <returns> A new DeclarativeScope instance. </returns>
        internal static DeclarativeScope CreateCatchScope(Scope parentScope, string catchVariableName)
        {
            if (parentScope == null)
                throw new ArgumentNullException("parentScope", "Catch scopes must have a parent scope.");
            if (catchVariableName == null)
                throw new ArgumentNullException("catchVariableName");
            var result = new DeclarativeScope(parentScope, 0);
            result.DeclareVariable(catchVariableName);
            result.CanDeclareVariables = false;    // Only the catch variable can be declared in this scope.
            return result;
        }

        /// <summary>
        /// Creates a new declarative scope for use inside a strict mode eval statement.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        /// <returns> A new DeclarativeScope instance. </returns>
        internal static DeclarativeScope CreateEvalScope(Scope parentScope)
        {
            if (parentScope == null)
                throw new ArgumentNullException("parentScope", "Eval scopes must have a parent scope.");
            return new DeclarativeScope(parentScope, 0);
        }

        /// <summary>
        /// Creates a new declarative scope for use at runtime.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        /// <param name="declaredVariableNames"> The names of variables that were declared in this scope. </param>
        /// <returns> A new DeclarativeScope instance. </returns>
        public static DeclarativeScope CreateRuntimeScope(Scope parentScope, string[] declaredVariableNames)
        {
            if (parentScope == null)
                throw new ArgumentNullException("parentScope", "Function scopes must have a parent scope.");
            if (declaredVariableNames == null)
                throw new ArgumentNullException("declaredVariableNames");
            var result = new DeclarativeScope(parentScope, declaredVariableNames.Length);
            foreach (string variableName in declaredVariableNames)
                result.DeclareVariable(variableName);
            result.values = new object[result.DeclaredVariableCount];
            return result;
        }

        /// <summary>
        /// Creates a new DeclarativeScope instance.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope, or <c>null</c> if this is
        /// the global scope. </param>
        /// <param name="declaredVariableCount"> The number of variables declared in this scope. </param>
        private DeclarativeScope(Scope parentScope, int declaredVariableCount)
            : base(parentScope, declaredVariableCount)
        {
        }

        /// <summary>
        /// Gets an array of values, one for each variable.  Only available for declarative scopes
        /// created using CreateRuntimeScope().
        /// </summary>
        public object[] Values
        {
            get { return this.values; }
        }

        /// <summary>
        /// Declares a variable or function in this scope.  This will be initialized with the value
        /// of the given expression.
        /// </summary>
        /// <param name="name"> The name of the variable. </param>
        /// <param name="valueAtTopOfScope"> The value of the variable at the top of the scope. </param>
        /// <param name="writable"> <c>true</c> if the variable can be modified; <c>false</c>
        /// otherwise. </param>
        /// <param name="deletable"> <c>true</c> if the variable can be deleted; <c>false</c>
        /// otherwise. </param>
        /// <returns> A reference to the variable that was declared. </returns>
        internal override DeclaredVariable DeclareVariable(string name, Expression valueAtTopOfScope = null, bool writable = true, bool deletable = false)
        {
            // Variables can be added to a declarative scope using eval().  When this happens the
            // values array needs to be resized.  That check happens here.
            if (this.values != null && this.DeclaredVariableCount >= this.Values.Length)
                Array.Resize(ref this.values, this.DeclaredVariableCount + 10);

            // Delegate to the Scope class.
            return base.DeclareVariable(name, valueAtTopOfScope, writable, deletable);
        }

        /// <summary>
        /// Returns <c>true</c> if the given variable exists in this scope.
        /// </summary>
        /// <param name="variableName"> The name of the variable to check. </param>
        /// <returns> <c>true</c> if the given variable exists in this scope; <c>false</c>
        /// otherwise. </returns>
        public override bool HasValue(string variableName)
        {
            return this.HasDeclaredVariable(variableName);
        }

        /// <summary>
        /// Returns the value of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <returns> The value of the given variable, or <c>null</c> if the variable doesn't exist
        /// in the scope. </returns>
        /// <exception cref="InvalidOperationException"> The scope was not created using
        /// CreateRuntimeScope(). </exception>
        public override object GetValue(string variableName)
        {
            if (this.Values == null)
                throw new InvalidOperationException("This method can only be used when the DeclarativeScope is created using CreateRuntimeScope().");
            var variable = GetDeclaredVariable(variableName);
            if (variable == null)
                return null;
            return this.Values[variable.Index];
        }

        /// <summary>
        /// Sets the value of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <param name="value"> The new value of the variable. </param>
        /// <exception cref="InvalidOperationException"> The scope was not created using
        /// CreateRuntimeScope(). </exception>
        public override void SetValue(string variableName, object value)
        {
            if (this.Values == null)
                throw new InvalidOperationException("This method can only be used when the DeclarativeScope is created using CreateRuntimeScope().");
            
            // Get information on the variable.
            var variable = GetDeclaredVariable(variableName);

            // If the variable doesn't exist, create it.
            if (variable == null)
                variable = this.DeclareVariable(variableName);

            // Check the variable is writable.
            if (variable.Writable == false)
                return;

            // Set the value.
            this.Values[variable.Index] = value;
        }

        /// <summary>
        /// Deletes the variable from the scope.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        public override bool Delete(string variableName)
        {
            if (this.Values == null)
                throw new InvalidOperationException("This method can only be used when the DeclarativeScope is created using CreateRuntimeScope().");
            var variable = GetDeclaredVariable(variableName);

            // Delete returns true if the variable doesn't exist.
            if (variable == null)
                return true;

            // Check if the variable is deletable.
            if (variable.Deletable == false)
                return false;

            // This variable can be deleted (it was declared in an eval()), remove the variable
            // from the scope.
            RemovedDeclaredVariable(variableName);
            return true;
        }

        /// <summary>
        /// Generates code that creates a new scope.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        internal override void GenerateScopeCreation(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Allocate storage for each variable if the declarative scope object has been optimized away.
            if (optimizationInfo.OptimizeDeclarativeScopes == false)
            {

                // Create a new declarative scope.
            
                // parentScope
                EmitHelpers.LoadScope(generator);

                // declaredVariableNames
                generator.LoadInt32(this.DeclaredVariableCount);
                generator.NewArray(typeof(string));
                int i = 0;
                foreach (string variableName in this.DeclaredVariableNames)
                {
                    generator.Duplicate();
                    generator.LoadInt32(i ++);
                    generator.LoadString(variableName);
                    generator.StoreArrayElement(typeof(string));
                }

                // DeclarativeScope.CreateRuntimeScope(parentScope, declaredVariableNames)
                generator.Call(ReflectionHelpers.DeclarativeScope_CreateRuntimeScope);

                // Save the new scope.
                EmitHelpers.StoreScope(generator);

            }
            else
            {

                // The declarative scope can be optimized away entirely.
                foreach (var variable in this.DeclaredVariables)
                {
                    variable.Store = null;
                    variable.Type = PrimitiveType.Any;
                }

                // Indicate the scope was not created.
                this.ExistsAtRuntime = false;

            }
        }
    }

}