using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a local variable in CIL code.
    /// </summary>
    internal abstract class ILLocalVariable
    {
        /// <summary>
        /// Gets the zero-based index of the local variable within the method body.
        /// </summary>
        public abstract int Index
        {
            get;
        }

        /// <summary>
        /// Gets the type of the local variable.
        /// </summary>
        public abstract Type Type
        {
            get;
        }

        /// <summary>
        /// Gets the local variable name, or <c>null</c> if a name was not provided.
        /// </summary>
        public abstract string Name
        {
            get;
        }
    }

#if !SILVERLIGHT
    
    /// <summary>
    /// Represents a local variable in CIL code.
    /// </summary>
    internal class DynamicILLocalVariable : ILLocalVariable
    {
        private int index;
        private Type type;
        private string name;

        /// <summary>
        /// Creates a new local variable instance.
        /// </summary>
        /// <param name="generator"> The generator that created this variable. </param>
        /// <param name="index"> The index of the local variable within the method body. </param>
        /// <param name="type"> The type of the variable. </param>
        /// <param name="name"> The name of the local variable.  Can be <c>null</c>. </param>
        public DynamicILLocalVariable(DynamicILGenerator generator, int index, Type type, string name)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (type == null)
                throw new ArgumentNullException("type");
            this.ILGenerator = generator;
            this.index = index;
            this.type = type;
            this.name = name;
        }

        /// <summary>
        /// Gets the generator that created this variable.
        /// </summary>
        public DynamicILGenerator ILGenerator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the zero-based index of the local variable within the method body.
        /// </summary>
        public override int Index
        {
            get { return this.index; }
        }

        /// <summary>
        /// Gets the type of the local variable.
        /// </summary>
        public override Type Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Gets the local variable name, or <c>null</c> if a name was not provided.
        /// </summary>
        public override string Name
        {
            get { return this.name; }
        }
    }

#endif

    /// <summary>
    /// Represents a local variable in CIL code.
    /// </summary>
    internal class ReflectionEmitILLocalVariable : ILLocalVariable
    {
        private string name;

        /// <summary>
        /// Creates a new local variable instance.
        /// </summary>
        /// <param name="local"> The underlying local variable. </param>
        /// <param name="name"> The name of the local variable.  Can be <c>null</c>. </param>
        public ReflectionEmitILLocalVariable(System.Reflection.Emit.LocalBuilder local, string name)
        {
            if (local == null)
                throw new ArgumentNullException("local");
            this.UnderlyingLocal = local;
            this.name = name;
            //if (name != null)
            //    local.SetLocalSymInfo(name);
        }

        /// <summary>
        /// Gets the underlying local variable.
        /// </summary>
        public System.Reflection.Emit.LocalBuilder UnderlyingLocal
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the zero-based index of the local variable within the method body.
        /// </summary>
        public override int Index
        {
            get { return this.UnderlyingLocal.LocalIndex; }
        }

        /// <summary>
        /// Gets the type of the local variable.
        /// </summary>
        public override Type Type
        {
            get { return this.UnderlyingLocal.LocalType; }
        }

        /// <summary>
        /// Gets the local variable name, or <c>null</c> if a name was not provided.
        /// </summary>
        public override string Name
        {
            get { return this.name; }
        }
    }

}
