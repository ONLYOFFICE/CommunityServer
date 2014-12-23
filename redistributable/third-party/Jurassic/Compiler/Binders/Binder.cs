using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a generic delegate that all method calls pass through.  For internal use only.
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="thisObject"> The value of the <c>this</c> keyword. </param>
    /// <param name="arguments"> The arguments that were passed to the function. </param>
    /// <returns> The result of calling the method. </returns>
    internal delegate object BinderDelegate(ScriptEngine engine, object thisObject, params object[] arguments);

    /// <summary>
    /// Selects a method from a list of candidates and performs type conversion from actual
    /// argument type to formal argument type.
    /// </summary>
    [Serializable]
    internal abstract class Binder
    {
        [NonSerialized]
        private BinderDelegate[] delegateCache;
        private const int MaximumCachedParameterCount = 8;




        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the name of the target methods.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the full name of the target methods, including the type name.
        /// </summary>
        public abstract string FullName
        {
            get;
        }

        /// <summary>
        /// Gets the maximum number of arguments of any of the target methods.  Used to set the
        /// length property on the function.
        /// </summary>
        public virtual int FunctionLength
        {
            get { return 0; }
        }




        //     METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Calls the bound method.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="thisObject"> The value of the <c>this</c> keyword. </param>
        /// <param name="arguments"> The arguments to pass to the function. </param>
        /// <returns> The result of calling the method. </returns>
        public object Call(ScriptEngine engine, object thisObject, params object[] arguments)
        {
            var binderDelegate = CreateDelegate(arguments.Length);
            return binderDelegate(engine, thisObject, arguments);
        }

        /// <summary>
        /// Creates a delegate that does type conversion and calls the bound method.
        /// </summary>
        /// <param name="argumentCount"> The number of arguments that will be passed to the delegate. </param>
        /// <returns> A delegate that does type conversion and calls the method represented by this
        /// object. </returns>
        /// <remarks> This method caches the result so calling CreateDelegate a second time with
        /// the same parameter count will be markedly quicker. </remarks>
        public BinderDelegate CreateDelegate(int argumentCount)
        {
            // If there are too many arguments, don't cache the delegate.
            if (argumentCount > MaximumCachedParameterCount)
                return CreateDelegateCore(argumentCount);

            // Save the delegate that is created into a cache so it doesn't have to be created again.
            if (this.delegateCache == null)
                this.delegateCache = new BinderDelegate[MaximumCachedParameterCount + 1];
            var binderDelegate = this.delegateCache[argumentCount];
            if (binderDelegate == null)
            {
                // Create a binding method.
                binderDelegate = CreateDelegateCore(argumentCount);

                // Store it in the cache.
                this.delegateCache[argumentCount] = binderDelegate;
            }
            return binderDelegate;
        }

        /// <summary>
        /// Creates a delegate that does type conversion and calls the bound method.
        /// </summary>
        /// <param name="argumentCount"> The number of arguments that will be passed to the delegate. </param>
        /// <returns> A delegate that does type conversion and calls the method represented by this
        /// object. </returns>
        /// <remarks> No caching of the result occurs. </remarks>
        private BinderDelegate CreateDelegateCore(int argumentCount)
        {
            // Create a new dynamic method.
            System.Reflection.Emit.DynamicMethod dm;
            ILGenerator generator;
#if !SILVERLIGHT
            if (ScriptEngine.LowPrivilegeEnvironment == false)
            {
                // Full trust only - skips visibility checks.
                dm = new System.Reflection.Emit.DynamicMethod(
                    string.Format("binder_for_{0}", this.FullName),                                                 // Name of the generated method.
                    typeof(object),                                                                                 // Return type of the generated method.
                    new Type[] { typeof(ScriptEngine), typeof(object), typeof(object[]) },                          // Parameter types of the generated method.
                    typeof(JSBinder),                                                                               // Owner type.
                    true);                                                                                          // Skips visibility checks.
				if (Environment.Version.Major >= 4 && Type.GetType ("Mono.Runtime") == null)
					generator = new DynamicILGenerator(dm);
				else
					generator = new ReflectionEmitILGenerator(dm.GetILGenerator());
            }
            else
            {
#endif
                // Partial trust / silverlight.
                dm = new System.Reflection.Emit.DynamicMethod(
                    string.Format("binder_for_{0}", this.FullName),                                                 // Name of the generated method.
                    typeof(object),                                                                                 // Return type of the generated method.
                    new Type[] { typeof(ScriptEngine), typeof(object), typeof(object[]) });                         // Parameter types of the generated method.
                generator = new ReflectionEmitILGenerator(dm.GetILGenerator());
#if !SILVERLIGHT
            }
#endif

            // Generate the body of the method.
            GenerateStub(generator, argumentCount);

            // Convert the DynamicMethod to a delegate.
            return (BinderDelegate)dm.CreateDelegate(typeof(BinderDelegate));
        }

        /// <summary>
        /// Generates a method that does type conversion and calls the bound method.
        /// </summary>
        /// <param name="generator"> The ILGenerator used to output the body of the method. </param>
        /// <param name="argumentCount"> The number of arguments that will be passed to the delegate. </param>
        /// <returns> A delegate that does type conversion and calls the method represented by this
        /// object. </returns>
        protected abstract void GenerateStub(ILGenerator generator, int argumentCount);




        //     OBJECT OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns a textual representation of this object.
        /// </summary>
        /// <returns> A textual representation of this object. </returns>
        public override string ToString()
        {
            return this.FullName;
        }
    }
}
