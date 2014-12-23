using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents an enclosing context where variables are uniquely defined.
    /// </summary>
    [Serializable]
    public abstract class Scope
    {
        // A dictionary containing the variables declared in this scope.
        private Dictionary<string, DeclaredVariable> variables;

        /// <summary>
        /// Represents a variable declared in a scope.
        /// </summary>
        [Serializable]
        internal class DeclaredVariable
        {
            // The scope the variable was declared in.
            public Scope Scope;

            // The index of the variable (in the order it was added).
            public int Index;

            // The name of the variable.
            public string Name;

            // The initial value of the variable (used for function declarations only).
            [NonSerialized]
            public Expression ValueAtTopOfScope;

            // true if the variable has been set with the initial value.
            public bool Initialized;

            // true if the variable can be modified.
            public bool Writable;

            // true if the variable can be deleted.
            public bool Deletable;

            // The storage container for the variable.
            [NonSerialized]
            public ILLocalVariable Store;

            // The statically-determined storage type for the variable.
            [NonSerialized]
            public PrimitiveType Type = PrimitiveType.Any;
        }

        /// <summary>
        /// Creates a new Scope instance.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope, or <c>null</c> if this is
        /// the global scope. </param>
        protected Scope(Scope parentScope)
            : this(parentScope, 0)
        {
        }

        /// <summary>
        /// Creates a new Scope instance.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope, or <c>null</c> if this is
        /// the global scope. </param>
        /// <param name="declaredVariableCount"> The number of variables declared in this scope. </param>
        protected Scope(Scope parentScope, int declaredVariableCount)
        {
            this.ParentScope = parentScope;
            this.variables = new Dictionary<string, DeclaredVariable>(declaredVariableCount);
            this.CanDeclareVariables = true;
            this.ExistsAtRuntime = true;
        }

        /// <summary>
        /// Gets a reference to the parent scope.  Can be <c>null</c> if this is the global scope.
        /// </summary>
        public Scope ParentScope
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the scope exists at runtime.  Defaults to
        /// <c>true</c>; will only be false if GenerateScopeCreation() has been called and the
        /// scope was optimized away.
        /// </summary>
        internal bool ExistsAtRuntime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether variables can be declared within the scope.
        /// </summary>
        public bool CanDeclareVariables
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the number of variables declared in this scope.
        /// </summary>
        internal int DeclaredVariableCount
        {
            get { return this.variables.Count; }
        }

        /// <summary>
        /// Gets an enumerable list of the names of all the declared variables (including function
        /// declarations), listed in the order they were declared.
        /// </summary>
        internal IEnumerable<string> DeclaredVariableNames
        {
            get
            {
                var declaredVariables = new List<DeclaredVariable>(this.variables.Values);
                declaredVariables.Sort((a, b) => a.Index - b.Index);
                var names = new string[declaredVariables.Count];
                for (int i = 0; i < declaredVariables.Count; i++)
                    names[i] = declaredVariables[i].Name;
                return names;
            }
        }

        /// <summary>
        /// Gets an enumerable list of the declared variables, in no particular order.
        /// </summary>
        internal IEnumerable<DeclaredVariable> DeclaredVariables
        {
            get { return this.variables.Values; }
        }

        /// <summary>
        /// Gets the index of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <returns> The index of the given variable, or <c>-1</c> if the variable doesn't exist
        /// in the scope. </returns>
        internal DeclaredVariable GetDeclaredVariable(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");
            DeclaredVariable variable;
            if (this.variables.TryGetValue(variableName, out variable) == false)
                return null;
            return variable;
        }

        /// <summary>
        /// Returns <c>true</c> if the given variable has been declared in this scope.
        /// </summary>
        /// <param name="name"> The name of the variable. </param>
        /// <returns> <c>true</c> if the given variable has been declared in this scope;
        /// <c>false</c> otherwise. </returns>
        internal bool HasDeclaredVariable(string name)
        {
            return this.variables.ContainsKey(name);
        }

        /// <summary>
        /// Declares a variable or function in this scope.  This will be initialized with the value
        /// of the given expression.
        /// </summary>
        /// <param name="name"> The name of the variable. </param>
        /// <param name="valueAtTopOfScope"> The value of the variable at the top of the scope.
        /// Can be <c>null</c> to indicate the variable does not need initializing. </param>
        /// <param name="writable"> <c>true</c> if the variable can be modified; <c>false</c>
        /// otherwise.  Defaults to <c>true</c>. </param>
        /// <param name="deletable"> <c>true</c> if the variable can be deleted; <c>false</c>
        /// otherwise.  Defaults to <c>true</c>. </param>
        /// <returns> A reference to the variable that was declared. </returns>
        internal virtual DeclaredVariable DeclareVariable(string name, Expression valueAtTopOfScope = null, bool writable = true, bool deletable = false)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            // If variables cannot be declared in the scope, try the parent scope instead.
            if (this.CanDeclareVariables == false)
            {
                if (this.ParentScope == null)
                    throw new InvalidOperationException("Invalid scope chain.");
                return this.ParentScope.DeclareVariable(name, valueAtTopOfScope, writable, deletable);
            }

            DeclaredVariable variable;
            this.variables.TryGetValue(name, out variable);
            if (variable == null)
            {
                // This is a local variable that has not been declared before.
                variable = new DeclaredVariable() { Scope = this, Index = this.variables.Count, Name = name, Writable = writable, Deletable = deletable };
                this.variables.Add(name, variable);
            }

            // Set the initial value, if one was provided.
            if (valueAtTopOfScope != null)
            {
                // Function expressions override literals.
                if ((valueAtTopOfScope is LiteralExpression && variable.ValueAtTopOfScope is FunctionExpression) == false)
                    variable.ValueAtTopOfScope = valueAtTopOfScope;
            }

            return variable;
        }

        /// <summary>
        /// Removes a declared variable from the scope.
        /// </summary>
        /// <param name="name"> The name of the variable. </param>
        internal void RemovedDeclaredVariable(string name)
        {
            this.variables.Remove(name);
        }

        /// <summary>
        /// Returns <c>true</c> if the given variable exists in this scope.
        /// </summary>
        /// <param name="variableName"> The name of the variable to check. </param>
        /// <returns> <c>true</c> if the given variable exists in this scope; <c>false</c>
        /// otherwise. </returns>
        public abstract bool HasValue(string variableName);

        /// <summary>
        /// Returns the value of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <returns> The value of the given variable, or <c>null</c> if the variable doesn't exist
        /// in the scope. </returns>
        public abstract object GetValue(string variableName);

        /// <summary>
        /// Sets the value of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <param name="value"> The new value of the variable. </param>
        public abstract void SetValue(string variableName, object value);

        /// <summary>
        /// Deletes the variable from the scope.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        public abstract bool Delete(string variableName);

        /// <summary>
        /// Generates code that creates a new scope.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        internal abstract void GenerateScopeCreation(ILGenerator generator, OptimizationInfo optimizationInfo);

        /// <summary>
        /// Generates code that initializes the variable and function declarations.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        internal virtual void GenerateDeclarations(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Initialize the declared variables and functions.
            foreach (var variable in this.variables.Values)
            {
                // When a scope is reused, i.e. with an eval(), do not reinitialize the variables.
                if (variable.Initialized == true)
                    continue;

                if (variable.ValueAtTopOfScope != null)
                {
                    // Emit the initialization code.
                    if (this is ObjectScope)
                    {
                        // Determine the property attributes.
                        var attributes = Library.PropertyAttributes.Enumerable;
                        if (variable.Writable == true)
                            attributes |= Library.PropertyAttributes.Writable;
                        if (variable.Deletable == true)
                            attributes |= Library.PropertyAttributes.Configurable;

                        // bool DefineProperty(string propertyName, PropertyDescriptor descriptor, bool throwOnError)
                        EmitHelpers.LoadScope(generator);
                        generator.CastClass(typeof(ObjectScope));
                        generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);
                        generator.LoadString(variable.Name);
                        variable.ValueAtTopOfScope.GenerateCode(generator, optimizationInfo);
                        EmitConversion.Convert(generator, variable.ValueAtTopOfScope.ResultType, PrimitiveType.Any, optimizationInfo);
                        generator.LoadInt32((int)attributes);
                        generator.NewObject(ReflectionHelpers.PropertyDescriptor_Constructor2);
                        generator.LoadBoolean(false);
                        generator.Call(ReflectionHelpers.ObjectInstance_DefineProperty);
                        generator.Pop();
                    }
                    else
                    {
                        variable.ValueAtTopOfScope.GenerateCode(generator, optimizationInfo);
                        var name = new NameExpression(this, variable.Name);
                        name.GenerateSet(generator, optimizationInfo, variable.ValueAtTopOfScope.ResultType, false);
                    }

                    // Mark the variable as having been initialized.
                    variable.Initialized = true;
                }
            }
        }

        /// <summary>
        /// Generates code that restores the parent scope as the active scope.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        internal void GenerateScopeDestruction(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            if (this.ExistsAtRuntime == false)
                return;

            // Modify the scope variable so it points at the parent scope.
            EmitHelpers.LoadScope(generator);
            generator.Call(ReflectionHelpers.Scope_ParentScope);
            EmitHelpers.StoreScope(generator);
        }
    }

}