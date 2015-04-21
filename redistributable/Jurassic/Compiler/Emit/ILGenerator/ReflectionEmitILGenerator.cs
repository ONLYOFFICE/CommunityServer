using System;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a generator of CIL bytes.
    /// </summary>
    internal class ReflectionEmitILGenerator : ILGenerator
    {
        private System.Reflection.Emit.ILGenerator generator;

        /// <summary>
        /// Creates a new ReflectionEmitILGenerator instance.
        /// </summary>
        /// <param name="generator"> The ILGenerator that is used to output the IL. </param>
        public ReflectionEmitILGenerator(System.Reflection.Emit.ILGenerator generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");
            this.generator = generator;
        }



        //     BUFFER MANAGEMENT
        //_________________________________________________________________________________________

        /// <summary>
        /// Emits a return statement and finalizes the generated code.  Do not emit any more
        /// instructions after calling this method.
        /// </summary>
        public override void Complete()
        {
            Return();
        }



        //     STACK MANAGEMENT
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops the value from the top of the stack.
        /// </summary>
        public override void Pop()
        {
            this.generator.Emit(OpCodes.Pop);
        }

        /// <summary>
        /// Duplicates the value on the top of the stack.
        /// </summary>
        public override void Duplicate()
        {
            this.generator.Emit(OpCodes.Dup);
        }



        //     BRANCHING AND LABELS
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a label without setting its position.
        /// </summary>
        /// <returns> A new label. </returns>
        public override ILLabel CreateLabel()
        {
            return new ReflectionEmitILLabel(this.generator.DefineLabel());
        }

        /// <summary>
        /// Defines the position of the given label.
        /// </summary>
        /// <param name="label"> The label to define. </param>
        public override void DefineLabelPosition(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.MarkLabel(((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Unconditionally branches to the given label.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void Branch(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Br, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is zero.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfZero(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Brfalse, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is non-zero, true or
        /// non-null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfNotZero(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Brtrue, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfEqual(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Beq, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are not equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfNotEqual(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Bne_Un, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThan(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Bgt, ((ReflectionEmitILLabel)label).UnderlyingLabel);
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
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Bgt_Un, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThanOrEqual(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Bge, ((ReflectionEmitILLabel)label).UnderlyingLabel);
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
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Bge_Un, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThan(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Blt, ((ReflectionEmitILLabel)label).UnderlyingLabel);
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
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Blt_Un, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThanOrEqual(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Ble, ((ReflectionEmitILLabel)label).UnderlyingLabel);
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
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Ble_Un, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// Returns from the current method.  A value is popped from the stack and used as the
        /// return value.
        /// </summary>
        public override void Return()
        {
            this.generator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates a jump table.  A value is popped from the stack - this value indicates the
        /// index of the label in the <paramref name="labels"/> array to jump to.
        /// </summary>
        /// <param name="labels"> A array of labels. </param>
        public override void Switch(ILLabel[] labels)
        {
            if (labels == null)
                throw new ArgumentNullException("labels");

            var reflectionLabels = new System.Reflection.Emit.Label[labels.Length];
            for (int i = 0; i < labels.Length; i++)
                reflectionLabels[i] = ((ReflectionEmitILLabel)labels[i]).UnderlyingLabel;
            this.generator.Emit(OpCodes.Switch, reflectionLabels);
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
            return new ReflectionEmitILLocalVariable(this.generator.DeclareLocal(type), name);
        }

        /// <summary>
        /// Pushes the value of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose value will be pushed. </param>
        public override void LoadVariable(ILLocalVariable variable)
        {
            if (variable as ReflectionEmitILLocalVariable == null)
                throw new ArgumentNullException("variable");
            this.generator.Emit(OpCodes.Ldloc, ((ReflectionEmitILLocalVariable)variable).UnderlyingLocal);
        }

        /// <summary>
        /// Pushes the address of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose address will be pushed. </param>
        public override void LoadAddressOfVariable(ILLocalVariable variable)
        {
            if (variable as ReflectionEmitILLocalVariable == null)
                throw new ArgumentNullException("variable");
            this.generator.Emit(OpCodes.Ldloca, ((ReflectionEmitILLocalVariable)variable).UnderlyingLocal);
        }

        /// <summary>
        /// Pops the value from the top of the stack and stores it in the given local variable.
        /// </summary>
        /// <param name="variable"> The variable to store the value. </param>
        public override void StoreVariable(ILLocalVariable variable)
        {
            if (variable as ReflectionEmitILLocalVariable == null)
                throw new ArgumentNullException("variable");
            this.generator.Emit(OpCodes.Stloc, ((ReflectionEmitILLocalVariable)variable).UnderlyingLocal);
        }

        /// <summary>
        /// Pushes the value of the method argument with the given index onto the stack.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to push onto the stack. </param>
        public override void LoadArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentOutOfRangeException("argumentIndex");
            switch (argumentIndex)
            {
                case 0:
                    this.generator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    this.generator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    this.generator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    this.generator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (argumentIndex < 256)
                        this.generator.Emit(OpCodes.Ldarg_S, (byte)argumentIndex);
                    else
                        this.generator.Emit(OpCodes.Ldarg, (short)argumentIndex);
                    break;
            }
        }

        /// <summary>
        /// Pops a value from the stack and stores it in the method argument with the given index.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to store into. </param>
        public override void StoreArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentOutOfRangeException("argumentIndex");
            if (argumentIndex < 256)
                this.generator.Emit(OpCodes.Starg_S, (byte)argumentIndex);
            else
                this.generator.Emit(OpCodes.Starg, (short)argumentIndex);
        }



        //     LOAD CONSTANT
        //_________________________________________________________________________________________

        /// <summary>
        /// Pushes <c>null</c> onto the stack.
        /// </summary>
        public override void LoadNull()
        {
            this.generator.Emit(OpCodes.Ldnull);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The integer to push onto the stack. </param>
        public override void LoadInt32(int value)
        {
            if (value >= -1 && value <= 8)
            {
                switch (value)
                {
                    case -1:
                        this.generator.Emit(OpCodes.Ldc_I4_M1);
                        break;
                    case 0:
                        this.generator.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        this.generator.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        this.generator.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        this.generator.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        this.generator.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        this.generator.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        this.generator.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        this.generator.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        this.generator.Emit(OpCodes.Ldc_I4_8);
                        break;
                }
                
            }
            else if (value >= -128 && value < 128)
                this.generator.Emit(OpCodes.Ldc_I4_S, (byte)value);
            else
                this.generator.Emit(OpCodes.Ldc_I4, value);
        }

        /// <summary>
        /// Pushes a 64-bit constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 64-bit integer to push onto the stack. </param>
        public override void LoadInt64(long value)
        {
            this.generator.Emit(OpCodes.Ldc_I8, value);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The number to push onto the stack. </param>
        public override void LoadDouble(double value)
        {
            this.generator.Emit(OpCodes.Ldc_R8, value);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The string to push onto the stack. </param>
        public override void LoadString(string value)
        {
            this.generator.Emit(OpCodes.Ldstr, value);
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
            this.generator.Emit(OpCodes.Ceq);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareGreaterThan()
        {
            this.generator.Emit(OpCodes.Cgt);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public override void CompareGreaterThanUnsigned()
        {
            this.generator.Emit(OpCodes.Cgt_Un);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareLessThan()
        {
            this.generator.Emit(OpCodes.Clt);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public override void CompareLessThanUnsigned()
        {
            this.generator.Emit(OpCodes.Clt_Un);
        }



        //     ARITHMETIC AND BITWISE OPERATIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops two values from the stack, adds them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void Add()
        {
            this.generator.Emit(OpCodes.Add);
        }

        /// <summary>
        /// Pops two values from the stack, subtracts the second from the first, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Subtract()
        {
            this.generator.Emit(OpCodes.Sub);
        }

        /// <summary>
        /// Pops two values from the stack, multiplies them together, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Multiply()
        {
            this.generator.Emit(OpCodes.Mul);
        }

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Divide()
        {
            this.generator.Emit(OpCodes.Div);
        }

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// remainder to the stack.
        /// </summary>
        public override void Remainder()
        {
            this.generator.Emit(OpCodes.Rem);
        }

        /// <summary>
        /// Pops a value from the stack, negates it, then pushes it back onto the stack.
        /// </summary>
        public override void Negate()
        {
            this.generator.Emit(OpCodes.Neg);
        }

        /// <summary>
        /// Pops two values from the stack, ANDs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseAnd()
        {
            this.generator.Emit(OpCodes.And);
        }

        /// <summary>
        /// Pops two values from the stack, ORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseOr()
        {
            this.generator.Emit(OpCodes.Or);
        }

        /// <summary>
        /// Pops two values from the stack, XORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseXor()
        {
            this.generator.Emit(OpCodes.Xor);
        }

        /// <summary>
        /// Pops a value from the stack, inverts it, then pushes the result to the stack.
        /// </summary>
        public override void BitwiseNot()
        {
            this.generator.Emit(OpCodes.Not);
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the left, then pushes the result
        /// to the stack.
        /// </summary>
        public override void ShiftLeft()
        {
            this.generator.Emit(OpCodes.Shl);
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is preserved.
        /// </summary>
        public override void ShiftRight()
        {
            this.generator.Emit(OpCodes.Shr);
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is not preserved.
        /// </summary>
        public override void ShiftRightUnsigned()
        {
            this.generator.Emit(OpCodes.Shr_Un);
        }



        //     CONVERSIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops a value from the stack, converts it to an object reference, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void Box(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsValueType == false)
                throw new ArgumentException("The type to box must be a value type.", "type");
            this.generator.Emit(OpCodes.Box, type);
        }

        /// <summary>
        /// Pops an object reference (representing a boxed value) from the stack, extracts the
        /// address, then pushes that address onto the stack.
        /// </summary>
        /// <param name="type"> The type of the boxed value.  This should be a value type. </param>
        public override void Unbox(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsValueType == false)
                throw new ArgumentException("The type of the boxed value must be a value type.", "type");
            this.generator.Emit(OpCodes.Unbox, type);
        }

        /// <summary>
        /// Pops an object reference (representing a boxed value) from the stack, extracts the value,
        /// then pushes the value onto the stack.
        /// </summary>
        /// <param name="type"> The type of the boxed value.  This should be a value type. </param>
        public override void UnboxAny(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsValueType == false)
                throw new ArgumentException("The type of the boxed value must be a value type.", "type");
            this.generator.Emit(OpCodes.Unbox_Any, type);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a signed integer, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertToInteger()
        {
            this.generator.Emit(OpCodes.Conv_I4);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to an unsigned integer, then pushes it back
        /// onto the stack.
        /// </summary>
        public override void ConvertToUnsignedInteger()
        {
            this.generator.Emit(OpCodes.Conv_U4);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a signed 64-bit integer, then pushes it
        /// back onto the stack.
        /// </summary>
        public override void ConvertToInt64()
        {
            this.generator.Emit(OpCodes.Conv_I8);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to an unsigned 64-bit integer, then pushes it
        /// back onto the stack.
        /// </summary>
        public override void ConvertToUnsignedInt64()
        {
            this.generator.Emit(OpCodes.Conv_U8);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertToDouble()
        {
            this.generator.Emit(OpCodes.Conv_R8);
        }

        /// <summary>
        /// Pops an unsigned integer from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertUnsignedToDouble()
        {
            this.generator.Emit(OpCodes.Conv_R_Un);
        }



        //     OBJECTS, METHODS, TYPES AND FIELDS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops the constructor arguments off the stack and creates a new instance of the object.
        /// </summary>
        /// <param name="constructor"> The constructor that is used to initialize the object. </param>
        public override void NewObject(System.Reflection.ConstructorInfo constructor)
        {
            this.generator.Emit(OpCodes.Newobj, constructor);
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
            if (method is System.Reflection.ConstructorInfo)
                this.generator.Emit(OpCodes.Call, (System.Reflection.ConstructorInfo)method);
            else if (method is System.Reflection.MethodInfo)
                this.generator.Emit(OpCodes.Call, (System.Reflection.MethodInfo)method);
            else
                throw new InvalidOperationException("Unsupported subtype of MethodBase.");
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
            if (method == null)
                throw new ArgumentNullException("method");
            if (method.IsStatic == true)
                throw new ArgumentException("Static methods cannot be called this method.", "method");
            if (method is System.Reflection.ConstructorInfo)
                this.generator.Emit(OpCodes.Callvirt, (System.Reflection.ConstructorInfo)method);
            else if (method is System.Reflection.MethodInfo)
                this.generator.Emit(OpCodes.Callvirt, (System.Reflection.MethodInfo)method);
            else
                throw new InvalidOperationException("Unsupported subtype of MethodBase.");
        }

        /// <summary>
        /// Pushes the value of the given field onto the stack.
        /// </summary>
        /// <param name="field"> The field whose value will be pushed. </param>
        public override void LoadField(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (field.IsStatic == true)
                this.generator.Emit(OpCodes.Ldsfld, field);
            else
                this.generator.Emit(OpCodes.Ldfld, field);
        }

        /// <summary>
        /// Pops a value off the stack and stores it in the given field.
        /// </summary>
        /// <param name="field"> The field to modify. </param>
        public override void StoreField(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (field.IsStatic == true)
                this.generator.Emit(OpCodes.Stsfld, field);
            else
                this.generator.Emit(OpCodes.Stfld, field);
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
            this.generator.Emit(OpCodes.Castclass, type);
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
            this.generator.Emit(OpCodes.Isinst, type);
        }

        /// <summary>
        /// Pushes a RuntimeTypeHandle corresponding to the given type onto the evaluation stack.
        /// </summary>
        /// <param name="type"> The type to convert to a RuntimeTypeHandle. </param>
        public override void LoadToken(Type type)
        {
            this.generator.Emit(OpCodes.Ldtoken, type);
        }

        /// <summary>
        /// Pushes a RuntimeMethodHandle corresponding to the given method onto the evaluation
        /// stack.
        /// </summary>
        /// <param name="method"> The method to convert to a RuntimeMethodHandle. </param>
        public override void LoadToken(System.Reflection.MethodBase method)
        {
            if (method is System.Reflection.ConstructorInfo)
                this.generator.Emit(OpCodes.Ldtoken, (System.Reflection.ConstructorInfo)method);
            else if (method is System.Reflection.MethodInfo)
                this.generator.Emit(OpCodes.Ldtoken, (System.Reflection.MethodInfo)method);
            else
                throw new InvalidOperationException("Unsupported subtype of MethodBase.");
        }

        /// <summary>
        /// Pushes a RuntimeFieldHandle corresponding to the given field onto the evaluation stack.
        /// </summary>
        /// <param name="field"> The type to convert to a RuntimeFieldHandle. </param>
        public override void LoadToken(System.Reflection.FieldInfo field)
        {
            this.generator.Emit(OpCodes.Ldtoken, field);
        }

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  The virtual qualifier will be ignored, if present.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        public override void LoadStaticMethodPointer(System.Reflection.MethodBase method)
        {
            if (method is System.Reflection.ConstructorInfo)
                this.generator.Emit(OpCodes.Ldftn, (System.Reflection.ConstructorInfo)method);
            else if (method is System.Reflection.MethodInfo)
                this.generator.Emit(OpCodes.Ldftn, (System.Reflection.MethodInfo)method);
            else
                throw new InvalidOperationException("Unsupported subtype of MethodBase.");
        }

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  This method cannot be used to retrieve a pointer to a static method.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        /// <exception cref="ArgumentException"> The method is static. </exception>
        public override void LoadVirtualMethodPointer(System.Reflection.MethodBase method)
        {
            if (method != null && method.IsStatic == true)
                throw new ArgumentException("The given method cannot be static.", "method");
            if (method is System.Reflection.ConstructorInfo)
                this.generator.Emit(OpCodes.Ldvirtftn, (System.Reflection.ConstructorInfo)method);
            else if (method is System.Reflection.MethodInfo)
                this.generator.Emit(OpCodes.Ldvirtftn, (System.Reflection.MethodInfo)method);
            else
                throw new InvalidOperationException("Unsupported subtype of MethodBase.");
        }

        /// <summary>
        /// Pops a managed or native pointer off the stack and initializes the referenced type with
        /// zeros.
        /// </summary>
        /// <param name="type"> The type the pointer on the top of the stack is pointing to. </param>
        public override void InitObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.generator.Emit(OpCodes.Initobj, type);
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
            this.generator.Emit(OpCodes.Newarr, type);
        }

        /// <summary>
        /// Pops the array and index off the stack and pushes the element value onto the stack.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void LoadArrayElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                    this.generator.Emit(OpCodes.Ldelem_I1);
                    break;
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    this.generator.Emit(OpCodes.Ldelem_I2);
                    break;
                case TypeCode.UInt32:
                case TypeCode.Int32:
                    this.generator.Emit(OpCodes.Ldelem_I4);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    this.generator.Emit(OpCodes.Ldelem_I8);
                    break;
                case TypeCode.Single:
                    this.generator.Emit(OpCodes.Ldelem_R4);
                    break;
                case TypeCode.Double:
                    this.generator.Emit(OpCodes.Ldelem_R8);
                    break;
                default:
                    if (type.IsClass == true)
                        this.generator.Emit(OpCodes.Ldelem_Ref);
                    else
                        this.generator.Emit(OpCodes.Ldelem, type);
                    break;
            }
        }

        /// <summary>
        /// Pops the array, index and value off the stack and stores the value in the array.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void StoreArrayElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                    this.generator.Emit(OpCodes.Stelem_I1);
                    break;
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    this.generator.Emit(OpCodes.Stelem_I2);
                    break;
                case TypeCode.UInt32:
                case TypeCode.Int32:
                    this.generator.Emit(OpCodes.Stelem_I4);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    this.generator.Emit(OpCodes.Stelem_I8);
                    break;
                case TypeCode.Single:
                    this.generator.Emit(OpCodes.Stelem_R4);
                    break;
                case TypeCode.Double:
                    this.generator.Emit(OpCodes.Stelem_R8);
                    break;
                default:
                    if (type.IsClass == true)
                        this.generator.Emit(OpCodes.Stelem_Ref);
                    else
                        this.generator.Emit(OpCodes.Stelem, type);
                    break;
            }
        }

        /// <summary>
        /// Pops an array off the stack and pushes the length of the array onto the stack.
        /// </summary>
        public override void LoadArrayLength()
        {
            this.generator.Emit(OpCodes.Ldlen);
        }



        //     EXCEPTION HANDLING
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops an exception object off the stack and throws the exception.
        /// </summary>
        public override void Throw()
        {
            this.generator.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Begins a try-catch-finally block.  After issuing this instruction any following
        /// instructions are conceptually within the try block.
        /// </summary>
        public override void BeginExceptionBlock()
        {
            this.generator.BeginExceptionBlock();
        }

        /// <summary>
        /// Ends a try-catch-finally block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void EndExceptionBlock()
        {
            this.generator.EndExceptionBlock();
        }

        /// <summary>
        /// Begins a catch block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        /// <param name="exceptionType"> The type of exception to handle. </param>
        public override void BeginCatchBlock(Type exceptionType)
        {
            this.generator.BeginCatchBlock(exceptionType);
        }

        /// <summary>
        /// Begins a finally block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void BeginFinallyBlock()
        {
            this.generator.BeginFinallyBlock();
        }

        /// <summary>
        /// Begins a filter block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void BeginFilterBlock()
        {
            this.generator.BeginExceptFilterBlock();
        }

        /// <summary>
        /// Begins a fault block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public override void BeginFaultBlock()
        {
            this.generator.BeginFaultBlock();
        }

        /// <summary>
        /// Unconditionally branches to the given label.  Unlike the regular branch instruction,
        /// this instruction can exit out of try, filter and catch blocks.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void Leave(ILLabel label)
        {
            if (label as ReflectionEmitILLabel == null)
                throw new ArgumentNullException("label");
            this.generator.Emit(OpCodes.Leave, ((ReflectionEmitILLabel)label).UnderlyingLabel);
        }

        /// <summary>
        /// This instruction can be used from within a finally block to resume the exception
        /// handling process.  It is the only valid way of leaving a finally block.
        /// </summary>
        public override void EndFinally()
        {
            this.generator.Emit(OpCodes.Endfinally);
        }

        /// <summary>
        /// This instruction can be used from within a filter block to indicate whether the
        /// exception will be handled.  It pops an integer from the stack which should be <c>0</c>
        /// to continue searching for an exception handler or <c>1</c> to use the handler
        /// associated with the filter.  EndFilter() must be called at the end of a filter block.
        /// </summary>
        public override void EndFilter()
        {
            this.generator.Emit(OpCodes.Endfilter);
        }



        //     DEBUGGING SUPPORT
        //_________________________________________________________________________________________

        /// <summary>
        /// Triggers a breakpoint in an attached debugger.
        /// </summary>
        public override void Breakpoint()
        {
            this.generator.Emit(OpCodes.Break);
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
            this.generator.Emit(OpCodes.Nop);
        }
    }

}
