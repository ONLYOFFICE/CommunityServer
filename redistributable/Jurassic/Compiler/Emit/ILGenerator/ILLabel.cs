using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a label in IL code.
    /// </summary>
    internal abstract class ILLabel
    {
    }

#if !SILVERLIGHT

    /// <summary>
    /// Represents a label in IL code.
    /// </summary>
    internal class DynamicILLabel : ILLabel
    {
        /// <summary>
        /// Creates a new label instance.
        /// </summary>
        /// <param name="generator"> The generator that created this label. </param>
        /// <param name="identifier"> The label identifier (must be unique within a method). </param>
        public DynamicILLabel(DynamicILGenerator generator, int identifier)
        {
            if (identifier < 0)
                throw new ArgumentOutOfRangeException("identifier");
            this.ILGenerator = generator;
            this.Identifier = identifier;
            this.ILOffset = -1;
#if !DEBUG
            this.EvaluationStackSize = -1;
#endif
        }

        /// <summary>
        /// Gets the generator that created this label.
        /// </summary>
        public DynamicILGenerator ILGenerator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the label identifier.
        /// </summary>
        public int Identifier
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the offset into the IL where the label resides.
        /// </summary>
        public int ILOffset
        {
            get;
            set;
        }

#if DEBUG

        /// <summary>
        /// Gets or sets a copy of the evaluation stack at the branch point.
        /// </summary>
        public object EvaluationStack
        {
            get;
            set;
        }

#else

        /// <summary>
        /// Gets or sets the size of the evaluation stack at the branch point.
        /// </summary>
        public int EvaluationStackSize
        {
            get;
            set;
        }

#endif
    }

#endif

    /// <summary>
    /// Represents a label in IL code.
    /// </summary>
    internal class ReflectionEmitILLabel : ILLabel
    {
        /// <summary>
        /// Creates a new label instance.
        /// </summary>
        /// <param name="label"> The underlying label. </param>
        public ReflectionEmitILLabel(System.Reflection.Emit.Label label)
        {
            if (label == null)
                throw new ArgumentNullException("label");
            this.UnderlyingLabel = label;
        }

        /// <summary>
        /// Gets the underlying label.
        /// </summary>
        public System.Reflection.Emit.Label UnderlyingLabel
        {
            get;
            private set;
        }
    }

}
