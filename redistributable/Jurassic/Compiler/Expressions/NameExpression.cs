using System;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a variable or part of a member reference.
    /// </summary>
    internal sealed class NameExpression : Expression, IReferenceExpression
    {
        /// <summary>
        /// Creates a new NameExpression instance.
        /// </summary>
        /// <param name="scope"> The current scope. </param>
        /// <param name="name"> The name of the variable or member that is being referenced. </param>
        public NameExpression(Scope scope, string name)
        {
            if (scope == null)
                throw new ArgumentNullException("scope");
            if (name == null)
                throw new ArgumentNullException("name");
            this.Scope = scope;
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the scope the name is contained within.
        /// </summary>
        public Scope Scope
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the variable or member.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get
            {
                // Typed variables are only allowed for declarative scopes that have been optimized away.
                if (this.Scope.ExistsAtRuntime == false)
                {
                    var scope = this.Scope;
                    do
                    {
                        var variableInfo = this.Scope.GetDeclaredVariable(this.Name);
                        if (variableInfo != null)
                            return variableInfo.Type;
                        scope = scope.ParentScope;
                    } while (scope != null && scope is DeclarativeScope && scope.ExistsAtRuntime == false);
                }
                return PrimitiveType.Any;
            }
        }

        /// <summary>
        /// Gets the static type of the reference.
        /// </summary>
        public PrimitiveType Type
        {
            get { return this.ResultType; }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // NOTE: this is a get reference because assignment expressions do not call this method.
            GenerateGet(generator, optimizationInfo, true);
        }

        /// <summary>
        /// Pushes the value of the reference onto the stack.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="throwIfUnresolvable"> <c>true</c> to throw a ReferenceError exception if
        /// the name is unresolvable; <c>false</c> to output <c>null</c> instead. </param>
        public void GenerateGet(ILGenerator generator, OptimizationInfo optimizationInfo, bool throwIfUnresolvable)
        {
            // This method generates code to retrieve the value of a variable, given the name of
            // variable and scope in which the variable is being referenced.  The variable was
            // not necessary declared in this scope - it might be declared in any of the parent
            // scopes (together called a scope chain).  The general algorithm is to start at the
            // head of the chain and search backwards until the variable is found.  There are
            // two types of scopes: declarative scopes and object scopes.  Object scopes are hard -
            // it cannot be known at compile time whether the variable exists or not so runtime
            // checks have to be inserted.  Declarative scopes are easier - variables have to be
            // declared and cannot be deleted.  There is one tricky bit: new variables can be
            // introduced into a declarative scope at runtime by a non-strict eval() statement.
            // Even worse, variables that were introduced by means of an eval() *can* be deleted.

            var scope = this.Scope;
            ILLocalVariable scopeVariable = null;
            var endOfGet = generator.CreateLabel();
            do
            {
                if (scope is DeclarativeScope)
                {
                    // The variable was declared in this scope.
                    var variable = scope.GetDeclaredVariable(this.Name);

                    if (variable != null)
                    {
                        if (scope.ExistsAtRuntime == false)
                        {
                            // The scope has been optimized away.  The value of the variable is stored
                            // in an ILVariable.

                            // Declare an IL local variable if no storage location has been allocated yet.
                            if (variable.Store == null)
                                variable.Store = generator.DeclareVariable(typeof(object), variable.Name);

                            // Load the value from the variable.
                            generator.LoadVariable(variable.Store);

                            // Ensure that we match ResultType.
                            EmitConversion.Convert(generator, variable.Type, this.ResultType, optimizationInfo);
                        }
                        else
                        {
                            // scope.Values[index]
                            if (scopeVariable == null)
                                EmitHelpers.LoadScope(generator);
                            else
                                generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.Call(ReflectionHelpers.DeclarativeScope_Values);
                            generator.LoadInt32(variable.Index);
                            generator.LoadArrayElement(typeof(object));
                        }

                        // The variable was found - no need to search any more parent scopes.
                        break;
                    }
                    else
                    {
                        // The variable was not defined at compile time, but may have been
                        // introduced by an eval() statement.
                        if (optimizationInfo.MethodOptimizationHints.HasEval == true)
                        {
                            // Check the variable exists: if (scope.HasValue(variableName) == true) {
                            if (scopeVariable == null)
                                EmitHelpers.LoadScope(generator);
                            else
                                generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.Call(ReflectionHelpers.Scope_HasValue);
                            var hasValueClause = generator.CreateLabel();
                            generator.BranchIfFalse(hasValueClause);

                            // Load the value of the variable.
                            if (scopeVariable == null)
                                EmitHelpers.LoadScope(generator);
                            else
                                generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.Call(ReflectionHelpers.Scope_GetValue);
                            generator.Branch(endOfGet);

                            // }
                            generator.DefineLabelPosition(hasValueClause);
                        }
                    }
                }
                else
                {
                    if (scope.ParentScope == null)
                    {

                        // Global variable access
                        // -------------------------------------------
                        // __object_cacheKey = null;
                        // __object_property_cachedIndex = 0;
                        // ...
                        // if (__object_cacheKey != object.InlineCacheKey)
                        //     xxx = object.InlineGetPropertyValue("variable", out __object_property_cachedIndex, out __object_cacheKey)
                        // else
                        //     xxx = object.InlinePropertyValues[__object_property_cachedIndex];

                        // Get a reference to the global object.
                        if (scopeVariable == null)
                            EmitHelpers.LoadScope(generator);
                        else
                            generator.LoadVariable(scopeVariable);
                        generator.CastClass(typeof(ObjectScope));
                        generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);

                        // TODO: share these variables somehow.
                        var cacheKey = generator.DeclareVariable(typeof(object));
                        var cachedIndex = generator.DeclareVariable(typeof(int));

                        // Store the object into a temp variable.
                        var objectInstance = generator.DeclareVariable(PrimitiveType.Object);
                        generator.StoreVariable(objectInstance);

                        // if (__object_cacheKey != object.InlineCacheKey)
                        generator.LoadVariable(cacheKey);
                        generator.LoadVariable(objectInstance);
                        generator.Call(ReflectionHelpers.ObjectInstance_InlineCacheKey);
                        var elseClause = generator.CreateLabel();
                        generator.BranchIfEqual(elseClause);

                        // value = object.InlineGetProperty("property", out __object_property_cachedIndex, out __object_cacheKey)
                        generator.LoadVariable(objectInstance);
                        generator.LoadString(this.Name);
                        generator.LoadAddressOfVariable(cachedIndex);
                        generator.LoadAddressOfVariable(cacheKey);
                        generator.Call(ReflectionHelpers.ObjectInstance_InlineGetPropertyValue);

                        var endOfIf = generator.CreateLabel();
                        generator.Branch(endOfIf);

                        // else
                        generator.DefineLabelPosition(elseClause);

                        // value = object.InlinePropertyValues[__object_property_cachedIndex];
                        generator.LoadVariable(objectInstance);
                        generator.Call(ReflectionHelpers.ObjectInstance_InlinePropertyValues);
                        generator.LoadVariable(cachedIndex);
                        generator.LoadArrayElement(typeof(object));

                        // End of the if statement
                        generator.DefineLabelPosition(endOfIf);

                        // Check if the value is null.
                        generator.Duplicate();
                        generator.BranchIfNotNull(endOfGet);
                        if (scope.ParentScope != null)
                            generator.Pop();

                    }
                    else
                    {

                        // Gets the value of a variable in an object scope.
                        if (scopeVariable == null)
                            EmitHelpers.LoadScope(generator);
                        else
                            generator.LoadVariable(scopeVariable);
                        generator.CastClass(typeof(ObjectScope));
                        generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);
                        generator.LoadString(this.Name);
                        generator.Call(ReflectionHelpers.ObjectInstance_GetPropertyValue_String);

                        // Check if the value is null.
                        generator.Duplicate();
                        generator.BranchIfNotNull(endOfGet);
                        if (scope.ParentScope != null)
                            generator.Pop();

                    }
                }

                // Try the parent scope.
                if (scope.ParentScope != null && scope.ExistsAtRuntime == true)
                {
                    if (scopeVariable == null)
                    {
                        scopeVariable = generator.CreateTemporaryVariable(typeof(Scope));
                        EmitHelpers.LoadScope(generator);
                    }
                    else
                    {
                        generator.LoadVariable(scopeVariable);
                    }
                    generator.Call(ReflectionHelpers.Scope_ParentScope);
                    generator.StoreVariable(scopeVariable);
                }
                scope = scope.ParentScope;

            } while (scope != null);

            // Throw an error if the name does not exist and throwIfUnresolvable is true.
            if (scope == null && throwIfUnresolvable == true)
                EmitHelpers.EmitThrow(generator, "ReferenceError", this.Name + " is not defined", optimizationInfo);

            // Release the temporary variable.
            if (scopeVariable != null)
                generator.ReleaseTemporaryVariable(scopeVariable);

            // Define a label at the end.
            generator.DefineLabelPosition(endOfGet);

            // Object scope references may have side-effects (because of getters) so if the value
            // is to be ignored we evaluate the value then pop the value from the stack.
            //if (optimizationInfo.SuppressReturnValue == true)
            //    generator.Pop();
        }

        /// <summary>
        /// Stores the value on the top of the stack in the reference.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="valueType"> The primitive type of the value that is on the top of the stack. </param>
        /// <param name="throwIfUnresolvable"> <c>true</c> to throw a ReferenceError exception if
        /// the name is unresolvable; <c>false</c> to create a new property instead. </param>
        public void GenerateSet(ILGenerator generator, OptimizationInfo optimizationInfo, PrimitiveType valueType, bool throwIfUnresolvable)
        {
            // The value is initially on the top of the stack but is stored in this variable
            // at the last possible moment.
            ILLocalVariable value = null;

            var scope = this.Scope;
            ILLocalVariable scopeVariable = null;
            var endOfSet = generator.CreateLabel();
            do
            {
                if (scope is DeclarativeScope)
                {
                    // Get information about the variable.
                    var variable = scope.GetDeclaredVariable(this.Name);
                    if (variable != null)
                    {
                        // The variable was declared in this scope.

                        if (scope.ExistsAtRuntime == false)
                        {
                            // The scope has been optimized away.  The value of the variable is stored
                            // in an ILVariable.

                            // Declare an IL local variable if no storage location has been allocated yet.
                            if (variable.Store == null)
                                variable.Store = generator.DeclareVariable(typeof(object), variable.Name);

                            if (value == null)
                            {
                                // The value to store is on the top of the stack - convert it to the
                                // storage type of the variable.
                                EmitConversion.Convert(generator, valueType, variable.Type, optimizationInfo);
                            }
                            else
                            {
                                // The value to store is in a temporary variable.
                                generator.LoadVariable(value);
                                EmitConversion.Convert(generator, PrimitiveType.Any, variable.Type, optimizationInfo);
                            }

                            // Store the value in the variable.
                            generator.StoreVariable(variable.Store);
                        }
                        else if (variable.Writable == true)
                        {
                            if (value == null)
                            {
                                // The value to store is on the top of the stack - convert it to an
                                // object and store it in a temporary variable.
                                EmitConversion.Convert(generator, valueType, PrimitiveType.Any, optimizationInfo);
                                value = generator.CreateTemporaryVariable(typeof(object));
                                generator.StoreVariable(value);
                            }

                            // scope.Values[index] = value
                            if (scopeVariable == null)
                                EmitHelpers.LoadScope(generator);
                            else
                                generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.Call(ReflectionHelpers.DeclarativeScope_Values);
                            generator.LoadInt32(variable.Index);
                            generator.LoadVariable(value);
                            generator.StoreArrayElement(typeof(object));
                        }
                        else
                        {
                            // The variable exists, but is read-only.
                            // Pop the value off the stack (if it is still there).
                            if (value == null)
                                generator.Pop();
                        }

                        // The variable was found - no need to search any more parent scopes.
                        break;
                    }
                    else
                    {
                        // The variable was not defined at compile time, but may have been
                        // introduced by an eval() statement.
                        if (optimizationInfo.MethodOptimizationHints.HasEval == true)
                        {
                            if (value == null)
                            {
                                // The value to store is on the top of the stack - convert it to an
                                // object and store it in a temporary variable.
                                EmitConversion.Convert(generator, valueType, PrimitiveType.Any, optimizationInfo);
                                value = generator.CreateTemporaryVariable(typeof(object));
                                generator.StoreVariable(value);
                            }

                            // Check the variable exists: if (scope.HasValue(variableName) == true) {
                            if (scopeVariable == null)
                                EmitHelpers.LoadScope(generator);
                            else
                                generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.Call(ReflectionHelpers.Scope_HasValue);
                            var hasValueClause = generator.CreateLabel();
                            generator.BranchIfFalse(hasValueClause);

                            // Set the value of the variable.
                            if (scopeVariable == null)
                                EmitHelpers.LoadScope(generator);
                            else
                                generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.LoadVariable(value);
                            generator.Call(ReflectionHelpers.Scope_SetValue);
                            generator.Branch(endOfSet);

                            // }
                            generator.DefineLabelPosition(hasValueClause);
                        }
                    }
                }
                else
                {
                    if (value == null)
                    {
                        // The value to store is on the top of the stack - convert it to an
                        // object and store it in a temporary variable.
                        EmitConversion.Convert(generator, valueType, PrimitiveType.Any, optimizationInfo);
                        value = generator.CreateTemporaryVariable(typeof(object));
                        generator.StoreVariable(value);
                    }

                    if (scope.ParentScope == null)
                    {
                        // Optimization: if this is the global scope, use hidden classes to
                        // optimize variable access.

                        // Global variable modification
                        // ----------------------------
                        // __object_cacheKey = null;
                        // __object_property_cachedIndex = 0;
                        // ...
                        // if (__object_cacheKey != object.InlineCacheKey)
                        //     object.InlineSetPropertyValueIfExists("property", value, strictMode, out __object_property_cachedIndex, out __object_cacheKey)
                        // else
                        //     object.InlinePropertyValues[__object_property_cachedIndex] = value;

                        // Get a reference to the global object.
                        if (scopeVariable == null)
                            EmitHelpers.LoadScope(generator);
                        else
                            generator.LoadVariable(scopeVariable);
                        generator.CastClass(typeof(ObjectScope));
                        generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);

                        // TODO: share these variables somehow.
                        var cacheKey = generator.DeclareVariable(typeof(object));
                        var cachedIndex = generator.DeclareVariable(typeof(int));

                        // Store the object into a temp variable.
                        var objectInstance = generator.DeclareVariable(PrimitiveType.Object);
                        generator.StoreVariable(objectInstance);

                        // if (__object_cacheKey != object.InlineCacheKey)
                        generator.LoadVariable(cacheKey);
                        generator.LoadVariable(objectInstance);
                        generator.Call(ReflectionHelpers.ObjectInstance_InlineCacheKey);
                        var elseClause = generator.CreateLabel();
                        generator.BranchIfEqual(elseClause);

                        // xxx = object.InlineSetPropertyValueIfExists("property", value, strictMode, out __object_property_cachedIndex, out __object_cacheKey)
                        generator.LoadVariable(objectInstance);
                        generator.LoadString(this.Name);
                        generator.LoadVariable(value);
                        generator.LoadBoolean(optimizationInfo.StrictMode);
                        generator.LoadAddressOfVariable(cachedIndex);
                        generator.LoadAddressOfVariable(cacheKey);
                        if (throwIfUnresolvable == false)
                        {
                            // Set the property value unconditionally.
                            generator.Call(ReflectionHelpers.ObjectInstance_InlineSetPropertyValue);
                        }
                        else
                        {
                            // Set the property value if the property exists.
                            generator.Call(ReflectionHelpers.ObjectInstance_InlineSetPropertyValueIfExists);

                            // The return value is true if the property was defined, and false if it wasn't.
                            generator.BranchIfTrue(endOfSet);
                        }

                        var endOfIf = generator.CreateLabel();
                        generator.Branch(endOfIf);

                        // else
                        generator.DefineLabelPosition(elseClause);

                        // object.InlinePropertyValues[__object_property_cachedIndex] = value;
                        generator.LoadVariable(objectInstance);
                        generator.Call(ReflectionHelpers.ObjectInstance_InlinePropertyValues);
                        generator.LoadVariable(cachedIndex);
                        generator.LoadVariable(value);
                        generator.StoreArrayElement(typeof(object));
                        generator.Branch(endOfSet);

                        // End of the if statement
                        generator.DefineLabelPosition(endOfIf);

                    }
                    else
                    {
                        // Slow route.

                        if (scopeVariable == null)
                            EmitHelpers.LoadScope(generator);
                        else
                            generator.LoadVariable(scopeVariable);
                        generator.CastClass(typeof(ObjectScope));
                        generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);
                        generator.LoadString(this.Name);
                        generator.LoadVariable(value);
                        generator.LoadBoolean(optimizationInfo.StrictMode);

                        if (scope.ParentScope == null && throwIfUnresolvable == false)
                        {
                            // Set the property value unconditionally.
                            generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValue_String);
                        }
                        else
                        {
                            // Set the property value if the property exists.
                            generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValueIfExists);

                            // The return value is true if the property was defined, and false if it wasn't.
                            generator.BranchIfTrue(endOfSet);
                        }

                    }
                }

                // Try the parent scope.
                if (scope.ParentScope != null && scope.ExistsAtRuntime == true)
                {
                    if (scopeVariable == null)
                    {
                        scopeVariable = generator.CreateTemporaryVariable(typeof(Scope));
                        EmitHelpers.LoadScope(generator);
                    }
                    else
                    {
                        generator.LoadVariable(scopeVariable);
                    }
                    generator.Call(ReflectionHelpers.Scope_ParentScope);
                    generator.StoreVariable(scopeVariable);
                }
                scope = scope.ParentScope;

            } while (scope != null);

            // The value might be still on top of the stack.
            if (value == null && scope == null)
                generator.Pop();

            // Throw an error if the name does not exist and throwIfUnresolvable is true.
            if (scope == null && throwIfUnresolvable == true)
                EmitHelpers.EmitThrow(generator, "ReferenceError", this.Name + " is not defined", optimizationInfo);

            // Release the temporary variables.
            if (value != null)
                generator.ReleaseTemporaryVariable(value);
            if (scopeVariable != null)
                generator.ReleaseTemporaryVariable(scopeVariable);

            // Define a label at the end.
            generator.DefineLabelPosition(endOfSet);
        }

        /// <summary>
        /// Deletes the reference and pushes <c>true</c> if the delete succeeded, or <c>false</c>
        /// if the delete failed.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public void GenerateDelete(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Deleting a variable is not allowed in strict mode.
            if (optimizationInfo.StrictMode == true)
                throw new JavaScriptException(optimizationInfo.Engine, "SyntaxError", string.Format("Cannot delete {0} because deleting a variable or argument is not allowed in strict mode", this.Name), optimizationInfo.SourceSpan.StartLine, optimizationInfo.Source.Path, optimizationInfo.FunctionName);

            var endOfDelete = generator.CreateLabel();
            var scope = this.Scope;
            ILLocalVariable scopeVariable = generator.CreateTemporaryVariable(typeof(Scope));
            EmitHelpers.LoadScope(generator);
            generator.StoreVariable(scopeVariable);
            do
            {
                if (scope is DeclarativeScope)
                {
                    var variable = scope.GetDeclaredVariable(this.Name);
                    if (variable != null)
                    {
                        // The variable is known at compile-time.
                        if (variable.Deletable == false)
                        {
                            // The variable cannot be deleted - return false.
                            generator.LoadBoolean(false);
                        }
                        else
                        {
                            // The variable can be deleted (it was declared inside an eval()).
                            // Delete the variable.
                            generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.Call(ReflectionHelpers.Scope_Delete);
                        }
                        break;
                    }
                    else
                    {
                        // The variable was not defined at compile time, but may have been
                        // introduced by an eval() statement.
                        if (optimizationInfo.MethodOptimizationHints.HasEval == true)
                        {
                            // Check the variable exists: if (scope.HasValue(variableName) == true) {
                            generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.Call(ReflectionHelpers.Scope_HasValue);
                            var hasValueClause = generator.CreateLabel();
                            generator.BranchIfFalse(hasValueClause);

                            // If the variable does exist, return true.
                            generator.LoadVariable(scopeVariable);
                            generator.CastClass(typeof(DeclarativeScope));
                            generator.LoadString(this.Name);
                            generator.Call(ReflectionHelpers.Scope_Delete);
                            generator.Branch(endOfDelete);

                            // }
                            generator.DefineLabelPosition(hasValueClause);
                        }
                    }
                }
                else
                {
                    // Check if the property exists by calling scope.ScopeObject.HasProperty(propertyName)
                    generator.LoadVariable(scopeVariable);
                    generator.CastClass(typeof(ObjectScope));
                    generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);
                    generator.Duplicate();
                    generator.LoadString(this.Name);
                    generator.Call(ReflectionHelpers.ObjectInstance_HasProperty);

                    // Jump past the delete if the property doesn't exist.
                    var endOfExistsCheck = generator.CreateLabel();
                    generator.BranchIfFalse(endOfExistsCheck);

                    // Call scope.ScopeObject.Delete(propertyName, false)
                    generator.LoadString(this.Name);
                    generator.LoadBoolean(false);
                    generator.Call(ReflectionHelpers.ObjectInstance_Delete);
                    generator.Branch(endOfDelete);

                    generator.DefineLabelPosition(endOfExistsCheck);
                    generator.Pop();

                    // If the name is not defined, return true.
                    if (scope.ParentScope == null)
                    {
                        generator.LoadBoolean(true);
                    }
                }

                // Try the parent scope.
                if (scope.ParentScope != null && scope.ExistsAtRuntime == true)
                {
                    generator.LoadVariable(scopeVariable);
                    generator.Call(ReflectionHelpers.Scope_ParentScope);
                    generator.StoreVariable(scopeVariable);
                }
                scope = scope.ParentScope;

            } while (scope != null);

            // Release the temporary variable.
            generator.ReleaseTemporaryVariable(scopeVariable);

            // Define a label at the end.
            generator.DefineLabelPosition(endOfDelete);

            // Delete obviously has side-effects so we evaluate the return value then pop it from
            // the stack.
            //if (optimizationInfo.SuppressReturnValue == true)
            //    generator.Pop();
        }

        /// <summary>
        /// Generates code to push the "this" value for a function call.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        public void GenerateThis(ILGenerator generator)
        {
            // Optimization: if there are no with scopes, simply emit undefined.
            bool scopeChainHasWithScope = false;
            var scope = this.Scope;
            do
            {
                if (scope is ObjectScope && ((ObjectScope)scope).ProvidesImplicitThisValue == true)
                {
                    scopeChainHasWithScope = true;
                    break;
                }
                scope = scope.ParentScope;
            } while (scope != null);

            if (scopeChainHasWithScope == false)
            {
                // No with scopes in the scope chain, use undefined as the "this" value.
                EmitHelpers.EmitUndefined(generator);
                return;
            }

            var end = generator.CreateLabel();

            scope = this.Scope;
            ILLocalVariable scopeVariable = generator.CreateTemporaryVariable(typeof(Scope));
            EmitHelpers.LoadScope(generator);
            generator.StoreVariable(scopeVariable);

            do
            {
                if (scope is DeclarativeScope)
                {
                    if (scope.HasDeclaredVariable(this.Name))
                    {
                        // The variable exists but declarative scopes always produce undefined for
                        // the "this" value.
                        EmitHelpers.EmitUndefined(generator);
                        break;
                    }
                }
                else
                {
                    var objectScope = (ObjectScope)scope;

                    // Check if the property exists by calling scope.ScopeObject.HasProperty(propertyName)
                    if (objectScope.ProvidesImplicitThisValue == false)
                        EmitHelpers.EmitUndefined(generator);
                    generator.LoadVariable(scopeVariable);
                    generator.CastClass(typeof(ObjectScope));
                    generator.Call(ReflectionHelpers.ObjectScope_ScopeObject);
                    if (objectScope.ProvidesImplicitThisValue == true)
                        generator.Duplicate();
                    generator.LoadString(this.Name);
                    generator.Call(ReflectionHelpers.ObjectInstance_HasProperty);
                    generator.BranchIfTrue(end);
                    generator.Pop();

                    // If the name is not defined, use undefined for the "this" value.
                    if (scope.ParentScope == null)
                    {
                        EmitHelpers.EmitUndefined(generator);
                    }
                }

                // Try the parent scope.
                if (scope.ParentScope != null && scope.ExistsAtRuntime == true)
                {
                    generator.LoadVariable(scopeVariable);
                    generator.Call(ReflectionHelpers.Scope_ParentScope);
                    generator.StoreVariable(scopeVariable);
                }
                scope = scope.ParentScope;

            } while (scope != null);

            // Release the temporary variable.
            generator.ReleaseTemporaryVariable(scopeVariable);

            // Define a label at the end.
            generator.DefineLabelPosition(end);
        }

        /// <summary>
        /// Calculates the hash code for this object.
        /// </summary>
        /// <returns> The hash code for this object. </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Scope.GetHashCode();
        }

        /// <summary>
        /// Determines if the given object is equal to this one.
        /// </summary>
        /// <param name="obj"> The object to compare. </param>
        /// <returns> <c>true</c> if the given object is equal to this one; <c>false</c> otherwise. </returns>
        public override bool Equals(object obj)
        {
            if ((obj is NameExpression) == false)
                return false;
            return this.Name == ((NameExpression)obj).Name && this.Scope == ((NameExpression)obj).Scope;
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}