using System;
using System.Collections.Generic;
using System.Text;

namespace Textile
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BlockModifierAttribute : Attribute
    {
        public BlockModifierAttribute()
        {
        }
    }
}
