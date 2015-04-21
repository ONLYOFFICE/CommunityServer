using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic
{

    // Silverlight does not support serialization.  The following two attributes prevent the need
    // to use conditional compilation to remove the [Serializable] attributes.

#if SILVERLIGHT

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class SerializableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NonSerializedAttribute : Attribute
    {
    }

#endif
}
