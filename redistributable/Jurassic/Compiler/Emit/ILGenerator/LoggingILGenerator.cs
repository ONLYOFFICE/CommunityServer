using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a generator that logs all operations.
    /// </summary>
    internal class LoggingILGenerator : ILGenerator
    {
        private ILGenerator generator;
        private StringBuilder header;
        private StringBuilder log;
        private int indent;
        private bool nextInstructionHasLabel;
        private int nextLabelNumber;
        private Dictionary<ILLabel, int> definedLabels;     // label -> label number
        private class LabelFixUp
        {
            public ILLabel Label;
            public int BufferOffset;
        }
        private List<LabelFixUp> fixUps;

        /// <summary>
        /// Creates a new LoggingILGenerator instance.
        /// </summary>
        /// <param name="generator"> The ILGenerator that is used to output the IL. </param>
        public LoggingILGenerator(ILGenerator generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");
            this.generator = generator;
            this.header = new StringBuilder();
            this.log = new StringBuilder();
            this.definedLabels = new Dictionary<ILLabel, int>();
            this.fixUps = new List<LabelFixUp>();
        }



        //     BUFFER MANAGEMENT
        //_________________________________________________________________________________________

        /// <summary>
        /// Emits a return statement and finalizes the generated code.  Do not emit any more
        /// instructions after calling this method.
        /// </summary>
        public override void Complete()
        {
            Log("ret");
            this.generator.Complete();
        }



        //     STACK MANAGEMENT
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops the value from the top of the stack.
        /// </summary>
        public override void Pop()
        {
            Log("pop");
            this.generator.Pop();
        }

        /// <summary>
        /// Duplicates the value on the top of the stack.
        /// </summary>
        public override void Duplicate()
        {
            Log("dup");
            this.generator.Duplicate();
        }



        //     BRANCHING AND LABELS
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a label without setting its position.
        /// </summary>
        /// <returns> A new label. </returns>
        public override ILLabel CreateLabel()
        {
            return this.generator.CreateLabel();
        }

        /// <summary>
        /// Defines the position of the given label.
        /// </summary>
        /// <param name="label"> The label to define. </param>
        public override void DefineLabelPosition(ILLabel label)
        {
            this.generator.DefineLabelPosition(label);

            // Output a label for the next instruction.
            this.nextInstructionHasLabel = true;

            // Fix up any previous references to the label.
            for (int i = 0; i < this.fixUps.Count; i++)
            {
                if (this.fixUps[i].Label == label)
                {
                    // Patch the label text.
                    int offset = this.fixUps[i].BufferOffset;
                    string labelText = string.Format("L{0:D3}", this.nextLabelNumber);
                    for (int j = 0; j < labelText.Length; j++)
                        this.log[offset + j] = labelText[j];

                    // Remove the fix up.
                    this.fixUps.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Unconditionally branches to the given label.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void Branch(ILLabel label)
        {
            Log("br", label);
            this.generator.Branch(label);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is zero.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfZero(ILLabel label)
        {
            Log("brfalse", label);
            this.generator.BranchIfZero(label);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is non-zero, true or
        /// non-null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfNotZero(ILLabel label)
        {
            Log("brtrue", label);
            this.generator.BranchIfNotZero(label);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfEqual(ILLabel label)
        {
            Log("beq", label);
            this.generator.BranchIfEqual(label);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are not equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfNotEqual(ILLabel label)
        {
            Log("bne", label);
            this.generator.BranchIfNotEqual(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThan(ILLabel label)
        {
            Log("bgt", label);
            this.generator.BranchIfGreaterThan(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than the second
        /// value on the stack.  If the operands are integers then they are treated as if they are
        /// unsigned.  If the operands are floating point numbers then a NaN value will trigger a
        /// branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThanUnsigned(ILLabel label)
        {
            Log("bgt.un", label);
            this.generator.BranchIfGreaterThanUnsigned(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThanOrEqual(ILLabel label)
        {
            Log("bge", label);
            this.generator.BranchIfGreaterThanOrEqual(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than or equal to
        /// the second value on the stack.  If the operands are integers then they are treated as
        /// if they are unsigned.  If the operands are floating point numbers then a NaN value will
        /// trigger a branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThanOrEqualUnsigned(ILLabel label)
        {
            Log("bge.un", label);
            this.generator.BranchIfGreaterThanOrEqualUnsigned(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThan(ILLabel label)
        {
            Log("blt", label);
            this.generator.BranchIfLessThan(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than the second
        /// value on the stack.  If the operands are integers then they are treated as if they are
        /// unsigned.  If the operands are floating point numbers then a NaN value will trigger a
        /// branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThanUnsigned(ILLabel label)
        {
            Log("blt.un", label);
            this.generator.BranchIfLessThanUnsigned(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThanOrEqual(ILLabel label)
        {
            Log("ble", label);
            this.generator.BranchIfLessThanOrEqual(label);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than or equal to
        /// the second value on the stack.  If the operands are integers then they are treated as
        /// if they are unsigned.  If the operands are floating point numbers then a NaN value will
        /// trigger a branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThanOrEqualUnsigned(ILLabel label)
        {
            Log("ble.un", label);
            this.generator.BranchIfLessThanOrEqualUnsigned(label);
        }

        /// <summary>
        /// Returns from the current method.  A value is popped from the stack and used as the
        /// return value.
        /// </summary>
        public override void Return()
        {
            Log("ret");
            this.generator.Return();
        }

        /// <summary>
        /// Creates a jump table.  A value is popped from the stack - this value indicates the
        /// index of the label in the <paramref name="labels"/> array to jump to.
        /// </summary>
        /// <param name="labels"> A array of labels. </param>
        public override void Switch(ILLabel[] labels)
        {
            Log("switch", labels);
            this.generator.Switch(labels);
        }



        //     LOCAL VARIABLES AND ARGUMENTS
        //_________________________________________________________________________________________

        /// <summary>
        /// Declares a new local variable.
        /// </summary>
        /// <param name="type"> The type of the local variable. </param>
        /// <param name="name"> The name of the local variable. Can be <c>null</c>. </param>
        /// <returns> A new local variable. </returns>
        public override ILLocalVariable DeclareVariable(Type type, string name = null)
        {
            var result = this.generator.DeclareVariable(type, name);
            if (name != null)
                this.header.AppendLine(string.Format(".local [{0}] {1} {2}", result.Index, result.Type, result.Name));
            else
                this.header.AppendLine(string.Format(".local [{0}] {1}", result.Index, result.Type));
            return result;
        }

        /// <summary>
        /// Pushes the value of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose value will be pushed. </param>
        public override void LoadVariable(ILLocalVariable variable)
        {
            Log("ldloc", variable);
            this.generator.LoadVariable(variable);
        }

        /// <summary>
        /// Pushes the address of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose address will be pushed. </param>
        public override void LoadAddressOfVariable(ILLocalVariable variable)
        {
            Log("ldloca", variable);
            this.generator.LoadAddressOfVariable(variable);
        }

        /// <summary>
        /// Pops the value from the top of the stack and stores it in the given local variable.
        /// </summary>
        /// <param name="variable"> The variable to store the value. </param>
        public override void StoreVariable(ILLocalVariable variable)
        {
            Log("stloc", variable);
            this.generator.StoreVariable(variable);
        }

        /// <summary>
        /// Pushes the value of the method argument with the given index onto the stack.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to push onto the stack. </param>
        public override void LoadArgument(int argumentIndex)
        {
            Log("ldarg", argumentIndex);
            this.generator.LoadArgument(argumentIndex);
        }

        /// <summary>
        /// Pops a value from the stack and stores it in the method argument with the given index.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to store into. </param>
        public override void StoreArgument(int argumentIndex)
        {
            Log("starg", argumentIndex);
            this.generator.StoreArgument(argumentIndex);
        }



        //     LOAD CONSTANT
        //_________________________________________________________________________________________

        /// <summary>
        /// Pushes <c>null</c> onto the stack.
        /// </summary>
        public override void LoadNull()
        {
            Log("ldnull");
            this.generator.LoadNull();
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The integer to push onto the stack. </param>
        public override void LoadInt32(int value)
        {
            Log("ldc.i4", value);
            this.generator.LoadInt32(value);
        }

        /// <summary>
        /// Pushes a 64-bit constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 64-bit integer to push onto the stack. </param>
        public override void LoadInt64(long value)
        {
            Log("ldc.i8", value);
            this.generator.LoadInt64(value);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The number to push onto the stack. </param>
        public override void LoadDouble(double value)
        {
            Log("ldc.r8", value);
            this.generator.LoadDouble(value);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The string to push onto the stack. </param>
        public override void LoadString(string value)
        {
            Log("ldstr", value);
            this.generator.LoadString(value);
        }



        //     RELATIONAL OPERATIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is equal to the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareEqual()
        {
            Log("ceq");
            this.generator.CompareEqual();
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareGreaterThan()
        {
            Log("cgt");
            this.generator.CompareGreaterThan();
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public override void CompareGreaterThanUnsigned()
        {
            Log("cgt.un");
            this.generator.CompareGreaterThanUnsigned();
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareLessThan()
        {
            Log("clt");
            this.generator.CompareLessThan();
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public override void CompareLessThanUnsigned()
        {
            Log("clt.un");
            this.generator.CompareLessThanUnsigned();
        }



        //     ARITHMETIC AND BITWISE OPERATIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops two values from the stack, adds them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void Add()
        {
            Log("add");
            this.generator.Add();
        }

        /// <summary>
        /// Pops two values from the stack, subtracts the second from the first, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Subtract()
        {
            Log("sub");
            this.generator.Subtract();
        }

        /// <summary>
        /// Pops two values from the stack, multiplies them together, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Multiply()
        {
            Log("mul");
            this.generator.Multiply();
        }

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Divide()
        {
            Log("div");
            this.generator.Divide();
        }

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// remainder to the stack.
        /// </summary>
        public override void Remainder()
        {
            Log("rem");
            this.generator.Remainder();
        }

        /// <summary>
        /// Pops a value from the stack, negates it, then pushes it back onto the stack.
        /// </summary>
        public override void Negate()
        {
            Log("neg");
            this.generator.Negate();
        }

        /// <summary>
        /// Pops two values from the stack, ANDs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseAnd()
        {
            Log("and");
            this.generator.BitwiseAnd();
        }

        /// <summary>
        /// Pops two values from the stack, ORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseOr()
        {
            Log("or");
            this.generator.BitwiseOr();
        }

        /// <summary>
        /// Pops two values from the stack, XORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseXor()
        {
            Log("xor");
            this.generator.BitwiseXor();
        }

        /// <summary>
        /// Pops a value from the stack, inverts it, then pushes the result to the stack.
        /// </summary>
        public override void BitwiseNot()
        {
            Log("not");
            this.generator.BitwiseNot();
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the left, then pushes the result
        /// to the stack.
        /// </summary>
        public override void ShiftLeft()
        {
            Log("shl");
            this.generator.ShiftLeft();
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is preserved.
        /// </summary>
        public override void ShiftRight()
        {
            Log("shr");
            this.generator.ShiftRight();
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is not preserved.
        /// </summary>
        public override void ShiftRightUnsigned()
        {
            Log("shr.un");
            this.generator.ShiftRightUnsigned();
        }



        //     CONVERSIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops a value from the stack, converts it to an object reference, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void Box(Type type)
        {
            Log("box", type);
            this.generator.Box(type);
        }

        /// <summary>
        /// Pops an object reference (representing a boxed value) from the stack, extracts the
        /// address, then pushes that address onto the stack.
        /// </summary>
        /// <param name="type"> The type of the boxed value.  This should be a value type. </param>
        public override void Unbox(Type type)
        {
            Log("unbox", type);
            this.generator.Unbox(type);
        }

        /// <summary>
        /// Pops an object reference (representing a boxed value) from the stack, extracts the value,
        /// then pushes the value onto the stack.
        /// </summary>
        /// <param name="type"> The type of the boxed value.  This should be a value type. </param>
        public override void UnboxAny(Type type)
        {
            Log("unbox.any", type);
            this.generator.UnboxAny(type);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a signed integer, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertToInteger()
        {
            Log("conv.i4");
            this.generator.ConvertToInteger();
        }

        /// <summary>
        /// Pops a value from the stack, converts it to an unsigned integer, then pushes it back
        /// onto the stack.
        /// </summary>
        public override void ConvertToUnsignedInteger()
        {
            Log("conv.u4");
            this.generator.ConvertToUnsignedInteger();
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a signed 64-bit integer, then pushes it
        /// back onto the stack.
        /// </summary>
        public override void ConvertToInt64()
        {
            Log("conv.i8");
            this.generator.ConvertToInt64();
        }

        /// <summary>
        /// Pops a value from the stack, converts it to an unsigned 64-bit integer, then pushes it
        /// back onto the stack.
        /// </summary>
        public override void ConvertToUnsignedInt64()
        {
            Log("conv.u8");
            this.generator.ConvertToUnsignedInt64();
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertToDouble()
        {
            Log("conv.u4");
            this.generator.ConvertToDouble();
        }

        /// <summary>
        /// Pops an unsigned integer from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertUnsignedToDouble()
        {
            Log("conv.r.un");
            this.generator.ConvertUnsignedToDouble();
        }




        //     OBJECTS, METHODS, TYPES AND FIELDS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops the constructor arguments off the stack and creates a new instance of the object.
        /// </summary>
        /// <param name="constructor"> The constructor that is used to initialize the object. </param>
        public override void NewObject(System.Reflection.ConstructorInfo constructor)
        {
            Log("newobj", constructor);
            this.generator.NewObject(constructor);
        }

        /// <summary>
        /// Pops the method arguments off the stack, calls the given method, then pushes the result
        /// to the stack (if there was one).  This operation can be used to call instance methods,
        /// but virtual overrides will not be called and a null check will not be performed at the
        /// callsite.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        public override void CallStatic(System.Reflection.MethodBase method)
        {
            Log("call", method);
            this.generator.CallStatic(method);
        }

        /// <summary>
        /// Pops the method arguments off the stack, calls the given method, then pushes the result
        /// to the stack (if there was one).  This operation cannot be used to call static methods.
        /// Virtual overrides are obeyed and a null check is performed.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        /// <exception cref="ArgumentException"> The method is static. </exception>
        public override void CallVirtual(System.Reflection.MethodBase method)
        {
            Log("callvirt", method);
            this.generator.CallVirtual(method);
        }

        /// <summary>
        /// Pushes the value of the given field onto the stack.
        /// </summary>
        /// <param name="field"> The field whose value will be pushed. </param>
        public override void LoadField(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (field.IsStatic == false)
                Log("ldfld", field);
            else
                Log("ldsfld", field);
            this.generator.LoadField(field);
        }

        /// <summary>
        /// Pops a value off the stack and stores it in the given field.
        /// </summary>
        /// <param name="field"> The field to modify. </param>
        public override void StoreField(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (field.IsStatic == false)
                Log("stfld", field);
            else
                Log("stsfld", field);
            this.generator.StoreField(field);
        }

        /// <summary>
        /// Pops an object off the stack, checks that the object inherits from or implements the
        /// given type, and pushes the object onto the stack if the check was successful or
        /// throws an InvalidCastException if the check failed.
        /// </summary>
        /// <param name="type"> The type of the class the object inherits from or the interface the
        /// object implements. </param>
        public override void CastClass(Type type)
        {
            Log("castclass", type);
            this.generator.CastClass(type);
        }

        /// <summary>
        /// Pops an object off the stack, checks that the object inherits from or implements the
        /// given type, and pushes either the object (if the check was successful) or <c>null</c>
        /// (if the check failed) onto the stack.
        /// </summary>
        /// <param name="type"> The type of the class the object inherits from or the interface the
        /// object implements. </param>
        public override void IsInstance(Type type)
        {
            Log("ininst", type);
            this.generator.IsInstance(type);
        }

        /// <summary>
        /// Pushes a RuntimeTypeHandle corresponding to the given type onto the evaluation stack.
        /// </summary>
        /// <param name="type"> The type to convert to a RuntimeTypeHandle. </param>
        public override void LoadToken(Type type)
        {
            Log("ldtoken", type);
            this.generator.LoadToken(type);
        }

        /// <summary>
        /// Pushes a RuntimeMethodHandle corresponding to the given method onto the evaluation
        /// stack.
        /// </summary>
        /// <param name="method"> The method to convert to a RuntimeMethodHandle. </param>
        public override void LoadToken(System.Reflection.MethodBase method)
        {
            Log("ldtoken", method);
            this.generator.LoadToken(method);
        }

        /// <summary>
        /// Pushes a RuntimeFieldHandle corresponding to the given field onto the evaluation stack.
        /// </summary>
        /// <param name="field"> The type to convert to a RuntimeFieldHandle. </param>
        public override void LoadToken(System.Reflection.FieldInfo field)
        {
            Log("ldtoken", field);
            this.generator.LoadToken(field);
        }

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  The virtual qualifier will be ignored, if present.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        public override void LoadStaticMethodPointer(System.Reflection.MethodBase method)
        {
            Log("ldftn", method);
            this.generator.LoadStaticMethodPointer(method);
        }

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  This method cannot be used to retrieve a pointer to a static method.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        /// <exception cref="ArgumentException"> The method is static. </exception>
        public override void LoadVirtualMethodPointer(System.Reflection.MethodBase method)
        {
            Log("ldvirtftn", method);
            this.generator.LoadVirtualMethodPointer(method);
        }

        /// <summary>
        /// Pops a managed or native pointer off the stack and initializes the referenced type with
        /// zeros.
        /// </summary>
        /// <param name="type"> The type the pointer on the top of the stack is pointing to. </param>
        public override void InitObject(Type type)
        {
            Log("initobj", type);
            this.generator.InitObject(type);
        }




        //     ARRAYS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops the size of the array off the stack and pushes a new array of the given type onto
        /// the stack.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void NewArray(Type type)
        {
            Log("newarr", type);
            this.generator.NewArray(type);
        }

        /// <summary>
        /// Pops the array and index off the stack and pushes the element value onto the stack.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void LoadArrayElement(Type type)
        {
            Log("ldelem", type);
            this.generator.LoadArrayElement(type);
        }

        /// <summary>
        /// Pops the array, index and value off the stack and stores the value in the array.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void StoreArrayElement(Type type)
        {
            Log("stelem", type);
            this.generator.StoreArrayElement(type);
        }

        /// <summary>
        /// Pops an array off the stack and pushes the length of the array onto the stack.
        /// </summary>
        public override void LoadArrayLength()
        {
            Log("ldlen");
            this.generator.LoadArrayLength();
        }



        //     EXCEPTION HANDLING
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops an exception object off the stack and throws the exception.
        /// </summary>
        public override void Throw()
        {
            Log("throw");
            this.generator.Throw();
        }

        /// <summary>
        /// Begins a try-catch-finally block.  After issuing this instruction any following
        /// instructions are conceptually within the try block.
        /// </summary>
        public override void BeginExceptionBlock()
        {
            LogCore(".try");
            LogCore("{");
            this.indent ++;
            this.generator.BeginExceptionBlock();
        }

        /// <summary>
        /// Ends a try-catch-finally block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void EndExceptionBlock()
        {
            this.indent --;
            LogCore("}");
            this.generator.EndExceptionBlock();
        }

        /// <summary>
        /// Begins a catch block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        /// <param name="exceptionType"> The type of exception to handle. </param>
        public override void BeginCatchBlock(Type exceptionType)
        {
            this.indent--;
            LogCore("}");
            LogCore(string.Format(".catch ({0})", exceptionType));
            LogCore("{");
            this.indent++;
            this.generator.BeginCatchBlock(exceptionType);
        }

        /// <summary>
        /// Begins a finally block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void BeginFinallyBlock()
        {
            this.indent--;
            LogCore("}");
            LogCore(".finally");
            LogCore("{");
            this.indent++;
            this.generator.BeginFinallyBlock();
        }

        /// <summary>
        /// Begins a filter block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void BeginFilterBlock()
        {
            LogCore("}");
            LogCore(".filter");
            LogCore("{");
            this.generator.BeginFilterBlock();
        }

        /// <summary>
        /// Begins a fault block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void BeginFaultBlock()
        {
            LogCore("}");
            LogCore(".fault");
            LogCore("{");
            this.generator.BeginFaultBlock();
        }

        /// <summary>
        /// Unconditionally branches to the given label.  Unlike the regular branch instruction,
        /// this instruction can exit out of try, filter and catch blocks.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void Leave(ILLabel label)
        {
            Log("leave", label);
            this.generator.Leave(label);
        }

        /// <summary>
        /// This instruction can be used from within a finally block to resume the exception
        /// handling process.  It is the only valid way of leaving a finally block.
        /// </summary>
        public override void EndFinally()
        {
            Log("endfinally");
            this.generator.EndFinally();
        }

        /// <summary>
        /// This instruction can be used from within a filter block to indicate whether the
        /// exception will be handled.  It pops an integer from the stack which should be <c>0</c>
        /// to continue searching for an exception handler or <c>1</c> to use the handler
        /// associated with the filter.  EndFilter() must be called at the end of a filter block.
        /// </summary>
        public override void EndFilter()
        {
            Log("endfilter");
            this.generator.EndFilter();
        }



        //     DEBUGGING SUPPORT
        //_________________________________________________________________________________________

        /// <summary>
        /// Triggers a breakpoint in an attached debugger.
        /// </summary>
        public override void Breakpoint()
        {
            Log("break");
            this.generator.Breakpoint();
        }

        /// <summary>
        /// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
        /// </summary>
        /// <param name="document"> The document for which the sequence point is being defined. </param>
        /// <param name="startLine"> The line where the sequence point begins. </param>
        /// <param name="startColumn"> The column in the line where the sequence point begins. </param>
        /// <param name="endLine"> The line where the sequence point ends. </param>
        /// <param name="endColumn"> The column in the line where the sequence point ends. </param>
        public override void MarkSequencePoint(System.Diagnostics.SymbolStore.ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            this.generator.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }



        //     MISC
        //_________________________________________________________________________________________

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void NoOperation()
        {
            Log("nop");
            this.generator.NoOperation();
        }



        //     OBJECT OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Converts the object to a string.
        /// </summary>
        /// <returns> A string containing the IL generated by this object. </returns>
        public override string ToString()
        {
            return this.header.ToString() + this.log.ToString();
        }



        //     PRIVATE HELPER METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Outputs an instruction to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        private void Log(string instruction)
        {
            LogInstruction(instruction, null);
        }

        /// <summary>
        /// Outputs an instruction and a label to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="label"> The label to output. </param>
        private void Log(string instruction, ILLabel label)
        {
            LogInstruction(instruction, string.Empty);
            AppendLabel(label);
        }

        /// <summary>
        /// Outputs an instruction and a number of labels to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="labels"> The labels to output. </param>
        private void Log(string instruction, ILLabel[] labels)
        {
            LogInstruction(instruction, string.Empty);
            bool first = true;
            foreach (var label in labels)
            {
                if (first == false)
                    this.log.Append(", ");
                first = false;
                AppendLabel(label);
            }
        }

        /// <summary>
        /// Appends the name of a label to the log.
        /// </summary>
        /// <param name="label"> The label to convert. </param>
        /// <returns> A string representation of the label. </returns>
        private void AppendLabel(ILLabel label)
        {
            // See if the label has been defined already.
            int labelNumber;
            if (this.definedLabels.TryGetValue(label, out labelNumber) == true)
            {
                // The label is defined.
                this.log.AppendFormat("L{0:D3}", labelNumber);
            }

            // The label is not defined.
            this.fixUps.Add(new LabelFixUp() { Label = label, BufferOffset = this.log.Length });
            this.log.Append(' ', 4);
        }

        /// <summary>
        /// Outputs an instruction and a variable to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="variable"> The variable to output. </param>
        private void Log(string instruction, ILLocalVariable variable)
        {
            if (variable.Name == null)
                LogInstruction(instruction, string.Format("V{0}", variable.Index));
            else
                LogInstruction(instruction, string.Format("V{0} ({1})", variable.Index, variable.Name));
        }

        /// <summary>
        /// Outputs an instruction and an integer to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="indexOrValue"> The integer to output. </param>
        private void Log(string instruction, int indexOrValue)
        {
            LogInstruction(instruction, indexOrValue.ToString());
        }

        /// <summary>
        /// Outputs an instruction and a 64-bit integer to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="numericValue"> The 64-bit integer to output. </param>
        private void Log(string instruction, long numericValue)
        {
            LogInstruction(instruction, numericValue.ToString());
        }

        /// <summary>
        /// Outputs an instruction and a floating-point value to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="numericValue"> The floating-point vaue to output. </param>
        private void Log(string instruction, double numericValue)
        {
            LogInstruction(instruction, numericValue.ToString());
        }

        /// <summary>
        /// Outputs an instruction and a string literal to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="stringConstant"> The string literal to output. </param>
        private void Log(string instruction, string stringConstant)
        {
            LogInstruction(instruction, Jurassic.Library.StringInstance.Quote(stringConstant));
        }

        /// <summary>
        /// Outputs an instruction and a type to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="type"> The type to output. </param>
        private void Log(string instruction, Type type)
        {
            LogInstruction(instruction, type.ToString());
        }

        /// <summary>
        /// Outputs an instruction and a method to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="method"> The method to output. </param>
        private void Log(string instruction, System.Reflection.MethodBase method)
        {
            LogInstruction(instruction, string.Format("{0}/{1}", method, method.DeclaringType));
        }
        
        /// <summary>
        /// Outputs an instruction and a field to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="field"> The field to output. </param>
        private void Log(string instruction, System.Reflection.FieldInfo field)
        {
            LogInstruction(instruction, string.Format("{0}/{1}", field, field.DeclaringType));
        }

        /// <summary>
        /// Outputs an instruction and a constructor to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="constructor"> The constructor to output. </param>
        private void Log(string instruction, System.Reflection.ConstructorInfo constructor)
        {
            LogInstruction(instruction, string.Format("{0}/{1}", constructor, constructor.DeclaringType));
        }

        /// <summary>
        /// Outputs an instruction and an arbitrary suffix to the log.
        /// </summary>
        /// <param name="instruction"> The instruction to output. </param>
        /// <param name="suffix"> A suffix to output. </param>
        private void LogInstruction(string instruction, string suffix)
        {
            // Output a new line.
            if (this.log.Length > 0)
                this.log.Append(Environment.NewLine);

            // Output any indentation.
            if (this.indent > 0)
                this.log.Append(' ', this.indent * 4);

            // Output the label, if there is one.
            if (this.nextInstructionHasLabel == true)
            {
                this.log.AppendFormat("L{0:D3}: ", this.nextLabelNumber);
                this.nextInstructionHasLabel = false;
                this.nextLabelNumber++;
            }
            else
                this.log.Append(' ', 6);

            // Output the instruction.
            this.log.Append(instruction);

            if (suffix != null)
            {
                // Pad the instruction to eleven characters long.
                this.log.Append(' ', 11 - instruction.Length);

                // Output the suffix.
                if (suffix != string.Empty)
                    this.log.Append(suffix);
            }
        }

        /// <summary>
        /// Outputs arbitrary text to the log.
        /// </summary>
        /// <param name="text"> The text to output. </param>
        private void LogCore(string text)
        {
            // Output a new line.
            if (this.log.Length > 0)
                this.log.Append(Environment.NewLine);

            // Output any indentation.
            if (this.indent > 0)
                this.log.Append(' ', this.indent * 4);

            // Output the text.
            this.log.Append(text);
        }
    }

}
