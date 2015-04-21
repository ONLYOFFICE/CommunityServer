using System;

namespace Jurassic.Library
{
    [Flags]
    public enum JSFunctionFlags
    {
        /// <summary>
        /// No flags were specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The first parameter to the function is the associated <c>ScriptEngine</c>.
        /// </summary>
        HasEngineParameter = 1,

        /// <summary>
        /// The first (or second, if <c>HasEngineParameter</c> is specified) parameter to the
        /// function is the <c>this</c> value.
        /// </summary>
        HasThisObject = 2,

        /// <summary>
        /// Indicates that the instance object may be modified by the function.
        /// </summary>
        MutatesThisObject = 4,

        /// <summary>
        /// A return value of null is converted to undefined immediately after control leaves the
        /// method.
        /// </summary>
        ConvertNullReturnValueToUndefined = 8,
    }
}
