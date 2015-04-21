using System;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a scope which is backed by the properties of an object.
    /// </summary>
    [Serializable]
    public class ObjectScope : Scope
    {
        private Library.ObjectInstance scopeObject;

        [NonSerialized]
        private Expression scopeObjectExpression;

        private bool providesImplicitThisValue;

        /// <summary>
        /// Creates a new global object scope.
        /// </summary>
        /// <returns> A new ObjectScope instance. </returns>
        internal static ObjectScope CreateGlobalScope(Library.GlobalObject globalObject)
        {
            if (globalObject == null)
                throw new ArgumentNullException("globalObject");
            return new ObjectScope(null) { ScopeObject = globalObject };
        }

        /// <summary>
        /// Creates a new object scope for use inside a with statement.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        /// <param name="scopeObject"> An expression that evaluates to the object to use. </param>
        /// <returns> A new ObjectScope instance. </returns>
        internal static ObjectScope CreateWithScope(Scope parentScope, Expression scopeObject)
        {
            if (parentScope == null)
                throw new ArgumentException("With scopes must have a parent scope.");
            return new ObjectScope(parentScope) { ScopeObjectExpression = scopeObject, ProvidesImplicitThisValue = true, CanDeclareVariables = false };
        }

        ///// <summary>
        ///// Creates a new object scope for use inside a with statement.
        ///// </summary>
        ///// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        ///// <param name="scopeObject"> An expression that evaluates to the object to use. </param>
        ///// <returns> A new ObjectScope instance. </returns>
        //public static ObjectScope CreateWithScope(Scope parentScope, Library.ObjectInstance scopeObject)
        //{
        //    if (parentScope == null)
        //        throw new ArgumentException("With scopes must have a parent scope.");
        //    return new ObjectScope(parentScope) { ScopeObject = scopeObject, ProvidesImplicitThisValue = true };
        //}

        /// <summary>
        /// Creates a new object scope for use at runtime.
        /// </summary>
        /// <param name="parentScope"> A reference to the parent scope.  Can not be <c>null</c>. </param>
        /// <param name="scopeObject"> An expression that evaluates to the object to use. </param>
        /// <param name="providesImplicitThisValue"> Indicates whether an implicit "this" value is
        /// supplied to function calls in this scope. </param>
        /// <param name="canDeclareVariables"> Indicates whether variables can be declared within
        /// the scope. </param>
        /// <returns> A new ObjectScope instance. </returns>
        public static ObjectScope CreateRuntimeScope(Scope parentScope, Library.ObjectInstance scopeObject, bool providesImplicitThisValue, bool canDeclareVariables)
        {
            return new ObjectScope(parentScope) { ScopeObject = scopeObject, ProvidesImplicitThisValue = providesImplicitThisValue, CanDeclareVariables = canDeclareVariables };
        }

        /// <summary>
        /// Creates a new ObjectScope instance.
        /// </summary>
        private ObjectScope(Scope parentScope)
            : base(parentScope)
        {
            this.ScopeObjectExpression = null;
            this.ProvidesImplicitThisValue = false;
        }

        /// <summary>
        /// Gets the object that stores the values of the variables in the scope.
        /// </summary>
        public Library.ObjectInstance ScopeObject
        {
            get { return this.scopeObject; }
            private set { this.scopeObject = value; }
        }

        /// <summary>
        /// Gets an expression that evaluates to the scope object.  <c>null</c> if the scope object
        /// is the global object.
        /// </summary>
        internal Expression ScopeObjectExpression
        {
            get { return this.scopeObjectExpression; }
            private set { this.scopeObjectExpression = value; }
        }

        /// <summary>
        /// Gets a value that indicates whether an implicit "this" value is supplied to function
        /// calls in this scope.
        /// </summary>
        public bool ProvidesImplicitThisValue
        {
            get { return this.providesImplicitThisValue; }
            private set { this.providesImplicitThisValue = value; }
        }

        /// <summary>
        /// Returns <c>true</c> if the given variable exists in this scope.
        /// </summary>
        /// <param name="variableName"> The name of the variable to check. </param>
        /// <returns> <c>true</c> if the given variable exists in this scope; <c>false</c>
        /// otherwise. </returns>
        public override bool HasValue(string variableName)
        {
            if (this.ScopeObject == null)
                throw new InvalidOperationException("The scope object is not yet available.");
            return this.ScopeObject.HasProperty(variableName);
        }

        /// <summary>
        /// Returns the value of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <returns> The value of the given variable, or <c>null</c> if the variable doesn't exist
        /// in the scope. </returns>
        public override object GetValue(string variableName)
        {
            if (this.ScopeObject == null)
                throw new InvalidOperationException("The scope object is not yet available.");
            return this.ScopeObject[variableName];
        }

        /// <summary>
        /// Sets the value of the given variable.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        /// <param name="value"> The new value of the variable. </param>
        public override void SetValue(string variableName, object value)
        {
            if (this.ScopeObject == null)
                throw new InvalidOperationException("The scope object is not yet available.");
            this.ScopeObject[variableName] = value;
        }

        /// <summary>
        /// Deletes the variable from the scope.
        /// </summary>
        /// <param name="variableName"> The name of the variable. </param>
        public override bool Delete(string variableName)
        {
            if (this.ScopeObject == null)
                throw new InvalidOperationException("The scope object is not yet available.");
            return this.ScopeObject.Delete(variableName, false);
        }

        /// <summary>
        /// Generates code that creates a new scope.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        internal override void GenerateScopeCreation(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Create a new runtime object scope.
            EmitHelpers.LoadScope(generator);  // parent scope
            if (this.ScopeObjectExpression == null)
            {
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_Global);
            }
            else
            {
                this.ScopeObjectExpression.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, this.ScopeObjectExpression.ResultType, optimizationInfo);
            }
            generator.LoadBoolean(this.ProvidesImplicitThisValue);
            generator.LoadBoolean(this.CanDeclareVariables);
            generator.Call(ReflectionHelpers.ObjectScope_CreateRuntimeScope);

            // Save the new scope.
            EmitHelpers.StoreScope(generator);
        }

        /// <summary>
        /// Get the value of the variable with the given name.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="name"> The name of the variable to get. </param>
        /// <param name="scope"> A variable that holds the current scope.  This variable is of
        /// type Scope.  Can be <c>null</c>. </param>
        /// <param name="endOfMemberLookup"> A label that points to the end the member lookup code.
        /// Can be <c>null</c>. </param>
        //internal override void GenerateGetCore(ILGenerator generator, string name, ILLocalVariable scope, ILLabel endOfMemberLookup)
        //{
            // Pseudo-code for getting the value of a variable
            // Note: scope is the first parameter of the generated method.
            // if (__object_cacheKey != scope.ScopeObject.CacheKey)
            // {
            //     object value = scope.ScopeObject.InlineGetPropertyValue(name, out index, out cacheKey)
            //     if (value != null)
            //         goto end
            // }
            // else
            // {
            //     value = object.PropertyValues[__object_property_cachedIndex]
            //     goto end
            // }
            // scope = scope.ParentScope
            // ...
            // throw new JavaScriptException("ReferenceError", name + " is not defined")
            // end:

            //// Create a label for the end of the member lookup, if it hasn't already been created.
            //bool weCreatedEndOfMemberLookup = false;
            //if (endOfMemberLookup == null)
            //{
            //    endOfMemberLookup = generator.CreateLabel();
            //    weCreatedEndOfMemberLookup = true;
            //}

            //// Store the scope object into a temp variable.
            //var scopeObject = generator.DeclareVariable(typeof(Library.ObjectInstance));
            //if (scope == null)
            //    EmitHelpers.LoadScope(generator);
            //else
            //    generator.LoadVariable(scope);
            //generator.Call(ReflectionHelpers.Scope_ScopeObject);
            //generator.StoreVariable(scopeObject);

            //// TODO: possibly share these variables somehow.
            //var cacheKey = generator.DeclareVariable(typeof(object));
            //var cachedIndex = generator.DeclareVariable(typeof(int));

            //// if (__object_cacheKey != scope.ScopeObject.CacheKey)
            //generator.LoadVariable(cacheKey);
            //generator.LoadVariable(scopeObject);
            //generator.Call(ReflectionHelpers.ObjectInstance_CacheKey);
            //var startOfElse = generator.CreateLabel();
            //generator.BranchIfEqual(startOfElse);

            //// scope.ScopeObject.InlineGetPropertyValue(name, out index, out cacheKey)
            //generator.LoadVariable(scopeObject);
            //generator.LoadString(name);
            //generator.LoadAddressOfVariable(cachedIndex);
            //generator.LoadAddressOfVariable(cacheKey);
            //generator.CallVirtual(ReflectionHelpers.ObjectInstance_InlineGetPropertyValue);

            //// if (value != null)
            ////     goto end
            //generator.Duplicate();
            //generator.BranchIfNotZero(endOfMemberLookup);
            //generator.Pop();

            //// else
            //var endOfIf = generator.CreateLabel();
            //generator.Branch(endOfIf);
            //generator.DefineLabelPosition(startOfElse);

            //// value = scope.ScopeObject.PropertyValues[__object_property_cachedIndex]
            //generator.LoadVariable(scopeObject);
            //generator.Call(ReflectionHelpers.ObjectInstance_PropertyValues);
            //generator.LoadVariable(cachedIndex);
            //generator.LoadArrayElement(typeof(object));

            //// goto end
            //generator.Branch(endOfMemberLookup);

            //// }
            //generator.DefineLabelPosition(endOfIf);

            //if (this.ParentScope != null)
            //{
            //    // scope = scope.ParentScope
            //    if (scope == null)
            //        EmitHelpers.LoadScope(generator);
            //    else
            //        generator.LoadVariable(scope);
            //    generator.Duplicate();
            //    generator.Call(ReflectionHelpers.Scope_ParentScope);
            //    if (scope == null)
            //        scope = generator.DeclareVariable(typeof(Scope));
            //    generator.StoreVariable(scope);

            //    // Generate code for the parent scope.
            //    this.ParentScope.GenerateGetCore(generator, name, scope, endOfMemberLookup);
            //}
            //else
            //{
            //    // throw new JavaScriptException("ReferenceError", name + " is not defined")
            //    EmitHelpers.EmitThrow(generator, "ReferenceError", name + " is not defined");
            //}

            //// Define the endOfMemberLookup label if this scope created it.
            //if (weCreatedEndOfMemberLookup == true)
            //    generator.DefineLabelPosition(endOfMemberLookup);
        //}

        /// <summary>
        /// Stores a value in the variable with the given name.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="name"> The name of the variable to set. </param>
        /// <param name="valueType"> The type of value to store. </param>
        /// <param name="value"> A variable that holds the value to store.  This variable is of
        /// type System.Object.  Can be <c>null</c>. </param>
        /// <param name="scope"> A variable that holds the current scope.  This variable is of
        /// type Scope.  Can be <c>null</c>. </param>
        /// <param name="endOfMemberLookup"> A label that points to the end the member lookup code.
        /// Can be <c>null</c>. </param>
        //internal override void GenerateSetCore(ILGenerator generator, string name, PrimitiveType valueType, ILLocalVariable value, ILLocalVariable scope, ILLabel endOfMemberLookup)
        //{
            // Pseudo-code for setting "property"
            // Note: scope is the first parameter of the generated method.
            // if (__object_cacheKey != scope.ScopeObject.CacheKey)
            // {
            //     bool exists = scope.ScopeObject.InlineSetPropertyValueIfExists(name, value, out index, out cacheKey)
            //     if (exists)
            //         goto end
            // }
            // else
            // {
            //     object.PropertyValues[__object_property_cachedIndex] = value
            //     goto end
            // }
            // scope = scope.ParentScope
            // ...
            // if (__object_cacheKey != scope.ScopeObject.CacheKey)
            // {
            //     scope.ScopeObject.InlineSetPropertyValue(name, value, out index, out cacheKey)
            // }
            // else
            // {
            //     object.PropertyValues[__object_property_cachedIndex] = value
            // }
            // end:

            //// Store the value into a variable if that has not yet been done.
            //if (value == null)
            //{
            //    value = generator.DeclareVariable(typeof(object));
            //    EmitConversion.Convert(generator, valueType, PrimitiveType.Any);
            //    generator.StoreVariable(value);
            //}

            //// Create a label for the end of the member lookup, if it hasn't already been created.
            //bool weCreatedEndOfMemberLookup = false;
            //if (endOfMemberLookup == null)
            //{
            //    endOfMemberLookup = generator.CreateLabel();
            //    weCreatedEndOfMemberLookup = true;
            //}

            //// Store the scope object into a temp variable.
            //var scopeObject = generator.DeclareVariable(typeof(Library.ObjectInstance));
            //if (scope == null)
            //    EmitHelpers.LoadScope(generator);
            //else
            //    generator.LoadVariable(scope);
            //generator.Call(ReflectionHelpers.Scope_ScopeObject);
            //generator.StoreVariable(scopeObject);

            //// TODO: possibly share these variables somehow.
            //var cacheKey = generator.DeclareVariable(typeof(object));
            //var cachedIndex = generator.DeclareVariable(typeof(int));

            //// if (__object_cacheKey != scope.ScopeObject.CacheKey)
            //generator.LoadVariable(cacheKey);
            //generator.LoadVariable(scopeObject);
            //generator.Call(ReflectionHelpers.ObjectInstance_CacheKey);
            //var startOfElse = generator.CreateLabel();
            //generator.BranchIfEqual(startOfElse);

            //// scope.ScopeObject.InlineSetPropertyValue(name, value, out index, out cacheKey)
            //generator.LoadVariable(scopeObject);
            //generator.LoadString(name);
            //generator.LoadVariable(value);
            //generator.LoadAddressOfVariable(cachedIndex);
            //generator.LoadAddressOfVariable(cacheKey);
            //if (this.ParentScope == null)
            //{
            //    // scope.ScopeObject.InlineSetPropertyValue(name, value, out index, out cacheKey)
            //    generator.CallVirtual(ReflectionHelpers.ObjectInstance_InlineSetPropertyValue);
            //}
            //else
            //{
            //    // bool exists = scope.ScopeObject.InlineSetPropertyValueIfExists(name, value, out index, out cacheKey)
            //    generator.CallVirtual(ReflectionHelpers.ObjectInstance_InlineSetPropertyValueIfExists);

            //    // if (exists)
            //    //     goto end
            //    generator.BranchIfNotZero(endOfMemberLookup);
            //}

            //// else
            //var endOfIf = generator.CreateLabel();
            //generator.Branch(endOfIf);
            //generator.DefineLabelPosition(startOfElse);

            //// scope.ScopeObject.PropertyValues[__object_property_cachedIndex] = value
            //generator.LoadVariable(scopeObject);
            //generator.Call(ReflectionHelpers.ObjectInstance_PropertyValues);
            //generator.LoadVariable(cachedIndex);
            //generator.LoadVariable(value);
            //generator.StoreArrayElement(typeof(object));

            //// goto end
            //if (this.ParentScope != null)
            //    generator.Branch(endOfMemberLookup);
            
            //// }
            //generator.DefineLabelPosition(endOfIf);

            //if (this.ParentScope != null)
            //{
            //    // scope = scope.ParentScope
            //    if (scope == null)
            //        EmitHelpers.LoadScope(generator);
            //    else
            //        generator.LoadVariable(scope);
            //    generator.Duplicate();
            //    generator.Call(ReflectionHelpers.Scope_ParentScope);
            //    if (scope == null)
            //        scope = generator.DeclareVariable(typeof(Scope));
            //    generator.StoreVariable(scope);

            //    // Generate code for the parent scope.
            //    this.ParentScope.GenerateSetCore(generator, name, valueType, value, scope, endOfMemberLookup);
            //}

            //// Define the endOfMemberLookup label if this scope created it.
            //if (weCreatedEndOfMemberLookup == true)
            //    generator.DefineLabelPosition(endOfMemberLookup);
        //}
    }

}