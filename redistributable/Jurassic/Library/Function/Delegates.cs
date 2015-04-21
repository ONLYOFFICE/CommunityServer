using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents a delegate that is used for user-defined functions.  For internal use only.
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="scope"> The scope (global or eval context) or the parent scope (function
    /// context). </param>
    /// <param name="thisObject"> The value of the <c>this</c> keyword. </param>
    /// <param name="functionObject"> The function object. </param>
    /// <param name="arguments"> The arguments that were passed to the function. </param>
    /// <returns> The result of calling the method. </returns>
    public delegate object FunctionDelegate(ScriptEngine engine, Compiler.Scope scope, object thisObject, Library.FunctionInstance functionObject, object[] arguments);

    /// <summary>
    /// Called once per element by Array.prototype.every, Array.prototype.some and Array.prototype.filter.
    /// </summary>
    /// <param name="thisObject"> The value of the <c>this</c> keyword.  Will be <c>null</c> if
    /// this value was not supplied. </param>
    /// <param name="value"> The value of the element in the array. </param>
    /// <param name="index"> The index of the element in the array. </param>
    /// <param name="array"> The array that is being operated on. </param>
    /// <returns> The result of a user-defined condition. </returns>
    public delegate bool ArrayConditionDelegate(ObjectInstance thisObject, object value, int index, ObjectInstance array);

    /// <summary>
    /// Called once per element by Array.prototype.forEach.
    /// </summary>
    /// <param name="thisObject"> The value of the <c>this</c> keyword.  Will be <c>null</c> if
    /// this value was not supplied. </param>
    /// <param name="value"> The value of the element in the array. </param>
    /// <param name="index"> The index of the element in the array. </param>
    /// <param name="array"> The array that is being operated on. </param>
    /// <returns> The result of a user-defined condition. </returns>
    public delegate void ArrayActionDelegate(ObjectInstance thisObject, object value, int index, ObjectInstance array);

    /// <summary>
    /// Called once per element by Array.prototype.map.
    /// </summary>
    /// <param name="thisObject"> The value of the <c>this</c> keyword.  Will be <c>null</c> if
    /// this value was not supplied. </param>
    /// <param name="value"> The value of the element in the array. </param>
    /// <param name="index"> The index of the element in the array. </param>
    /// <param name="array"> The array that is being operated on. </param>
    /// <returns> The result of a user-defined condition. </returns>
    public delegate object ArrayMapDelegate(ObjectInstance thisObject, object value, int index, ObjectInstance array);

    /// <summary>
    /// Called once per element by Array.prototype.reduce and Array.prototype.reduceRight.
    /// </summary>
    /// <param name="thisObject"> The value of the <c>this</c> keyword.  Will be <c>null</c> if
    /// this value was not supplied. </param>
    /// <param name="previousValue"> Either the initial value, the value of the last element in the
    /// array (if no initial value was provided) or the result of the last callback. </param>
    /// <param name="currentValue"> The value of the element in the array. </param>
    /// <param name="index"> The index of the element in the array. </param>
    /// <param name="array"> The array that is being operated on. </param>
    /// <returns> The reduced value. </returns>
    public delegate object ArrayReduceDelegate(ObjectInstance thisObject, object previousValue, object currentValue, int index, ObjectInstance array);
}
