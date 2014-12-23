using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
#if !SILVERLIGHT

    /// <summary>
    /// Represents a generator of CIL bytes.
    /// </summary>
    internal class DynamicILGenerator : ILGenerator
    {
        private System.Reflection.Emit.DynamicMethod dynamicMethod;
        private System.Reflection.Emit.DynamicILInfo dynamicILInfo;
        private byte[] bytes;
        private int offset;

        // Stack information.
        private int stackSize;
        private int maxStackSize;

        [Flags]
        private enum VESType
        {
            Int32 = 1,
            Int64 = 2,
            NativeInt = 4,
            Float = 8,
            Object = 16,
            ManagedPointer = 32,
        }
#pragma warning disable 0649
        private Stack<VESType> operands;
#pragma warning restore 0649
        private bool stackIsIndeterminate;

        // All of the local variables defined within this method.
        private List<DynamicILLocalVariable> localVariables;

        // The local variable signature blob.
        private System.Reflection.Emit.SignatureHelper signatureHelper;

        // All the labels defined within the method.
        private List<DynamicILLabel> labels;

        private struct Fixup
        {
            public int Position;                // The IL offset to fix up.
            public int Length;                  // The length of the fix up, in bytes.
            public int StartOfNextInstruction;  // The IL offset of the start of the next instruction.
            public DynamicILLabel Label;        // The label that is being jumped to.
        }
        private List<Fixup> fixups;

        private enum ExceptionClauseType
        {
            Catch,
            Finally,
            Filter,
            Fault,
        }
        private class ExceptionClause
        {
            public ExceptionClauseType Type;
            public int ILStart;
            public int ILLength;
            public int CatchToken;
            public int FilterHandlerStart;
        }
        private class ExceptionRegion
        {
            public int Start;
            public int TryLength;
            public List<ExceptionClause> Clauses;
            public ILLabel EndLabel;
        }
        private Stack<ExceptionRegion> activeExceptionRegions;
        private List<ExceptionRegion> exceptionRegions;

        /// <summary>
        /// Creates a new DynamicILGenerator instance.
        /// </summary>
        /// <param name="dynamicMethod"> The dynamic method to generate code for. </param>
        public DynamicILGenerator(System.Reflection.Emit.DynamicMethod dynamicMethod)
        {
            if (dynamicMethod == null)
                throw new ArgumentNullException("dynamicMethod");
            this.dynamicMethod = dynamicMethod;
            this.dynamicILInfo = dynamicMethod.GetDynamicILInfo();
            this.bytes = new byte[100];
            this.localVariables = new List<DynamicILLocalVariable>();
            this.signatureHelper = System.Reflection.Emit.SignatureHelper.GetLocalVarSigHelper(null);
            this.labels = new List<DynamicILLabel>();
            this.fixups = new List<Fixup>();

#if DEBUG
            this.operands = new Stack<VESType>();
#endif
        }



        //     BUFFER MANAGEMENT
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the bytes that define the IL stream.
        /// </summary>
        public byte[] CodeBytes
        {
            get { return this.bytes; }
        }

        /// <summary>
        /// Emits a return statement and finalizes the generated code.  Do not emit any more
        /// instructions after calling this method.
        /// </summary>
        public override unsafe void Complete()
        {
            // Check there aren't any outstanding exception blocks.
            if (this.activeExceptionRegions != null && this.activeExceptionRegions.Count > 0)
                throw new InvalidOperationException("The current method contains unclosed exception blocks.");

            Return();
            FixLabels();
            fixed (byte* bytes = this.bytes)
                this.dynamicILInfo.SetCode(bytes, this.offset, this.maxStackSize);
            this.dynamicILInfo.SetLocalSignature(this.LocalSignature);

            if (this.exceptionRegions != null && this.exceptionRegions.Count > 0)
            {
                // Count the number of exception clauses.
                int clauseCount = 0;
                foreach (var exceptionRegion in this.exceptionRegions)
                    clauseCount += exceptionRegion.Clauses.Count;

                var exceptionBytes = new byte[4 + 24 * clauseCount];
                var writer = new System.IO.BinaryWriter(new System.IO.MemoryStream(exceptionBytes));

                // 4-byte header, see Partition II, section 25.4.5.
                writer.Write((byte)0x41);               // Flags: CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat
                writer.Write(exceptionBytes.Length);    // 3-byte data size.
                writer.Flush();
                writer.BaseStream.Seek(4, System.IO.SeekOrigin.Begin);

                // Exception clauses, see Partition II, section 25.4.6.
                foreach (var exceptionRegion in this.exceptionRegions)
                {
                    foreach (var clause in exceptionRegion.Clauses)
                    {
                        switch (clause.Type)
                        {
                            case ExceptionClauseType.Catch:
                                writer.Write(0);                                // Flags
                                break;
                            case ExceptionClauseType.Filter:
                                writer.Write(1);                                // Flags
                                break;
                            case ExceptionClauseType.Finally:
                                writer.Write(2);                                // Flags
                                break;
                            case ExceptionClauseType.Fault:
                                writer.Write(4);                                // Flags
                                break;
                        }
                        writer.Write(exceptionRegion.Start);                    // TryOffset
                        writer.Write(clause.ILStart - exceptionRegion.Start);   // TryLength
                        writer.Write(clause.ILStart);                           // HandlerOffset
                        writer.Write(clause.ILLength);                          // HandlerLength
                        if (clause.Type == ExceptionClauseType.Catch)
                            writer.Write(clause.CatchToken);                    // ClassToken
                        else if (clause.Type == ExceptionClauseType.Filter)
                            writer.Write(clause.FilterHandlerStart);            // FilterOffset
                        else
                            writer.Write(0);
                    }
                }
                writer.Flush();
                this.dynamicILInfo.SetExceptions(exceptionBytes);
            }
        }

        /// <summary>
        /// Enlarges the internal IL buffer.
        /// </summary>
        /// <param name="instructionSize"> The size of the instruction that triggered the resize. </param>
        private void EnlargeArray(int instructionSize)
        {
            Array.Resize(ref this.bytes, this.bytes.Length * 2);
        }

        /// <summary>
        /// Emits a 16-bit integer and increments the offset member variable.
        /// </summary>
        /// <param name="value"> The integer to emit. </param>
        private void EmitInt16(int value)
        {
            int offset = this.offset;
            this.bytes[offset++] = (byte)value;
            this.bytes[offset++] = (byte)(value >> 8);
            this.offset = offset;
        }

        /// <summary>
        /// Emits a 32-bit integer and increments the offset member variable.
        /// </summary>
        /// <param name="value"> The integer to emit. </param>
        private void EmitInt32(int value)
        {
            int offset = this.offset;
            this.bytes[offset++] = (byte)value;
            this.bytes[offset++] = (byte)(value >> 8);
            this.bytes[offset++] = (byte)(value >> 16);
            this.bytes[offset++] = (byte)(value >> 24);
            this.offset = offset;
        }

        /// <summary>
        /// Emits a 64-bit integer and increments the offset member variable.
        /// </summary>
        /// <param name="value"> The 64-bit integer to emit. </param>
        private void EmitInt64(long value)
        {
            int offset = this.offset;
            this.bytes[offset++] = (byte)value;
            this.bytes[offset++] = (byte)(value >> 8);
            this.bytes[offset++] = (byte)(value >> 16);
            this.bytes[offset++] = (byte)(value >> 24);
            this.bytes[offset++] = (byte)(value >> 32);
            this.bytes[offset++] = (byte)(value >> 40);
            this.bytes[offset++] = (byte)(value >> 48);
            this.bytes[offset++] = (byte)(value >> 56);
            this.offset = offset;
        }

        /// <summary>
        /// Emits a one byte opcode.
        /// </summary>
        /// <param name="opCode"> The opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        private void Emit1ByteOpCode(byte opCode, int popCount, int pushCount)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 1;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode;

            // Update the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a one byte opcode plus a 1-byte operand.
        /// </summary>
        /// <param name="opCode"> The opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        /// <param name="emitInt8"> An 8-bit integer to emit. </param>
        private void Emit1ByteOpCodeInt8(byte opCode, int popCount, int pushCount, int emitInt8)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 2;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode;
            this.bytes[this.offset++] = (byte)emitInt8;

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a one byte opcode plus a 2-byte operand.
        /// </summary>
        /// <param name="opCode"> The opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        /// <param name="emitInt16"> A 16-bit integer to emit. </param>
        private void Emit1ByteOpCodeInt16(byte opCode, int popCount, int pushCount, int emitInt16)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 3;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode;
            EmitInt16(emitInt16);

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a one byte opcode plus a 4-byte operand.
        /// </summary>
        /// <param name="opCode"> The opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        /// <param name="emitInt32"> A 32-bit integer to emit. </param>
        private void Emit1ByteOpCodeInt32(byte opCode, int popCount, int pushCount, int emitInt32)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 5;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode;
            EmitInt32(emitInt32);

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a one byte opcode plus a 8-byte operand.
        /// </summary>
        /// <param name="opCode"> The opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        /// <param name="emitInt64"> A 64-bit integer to emit. </param>
        private void Emit1ByteOpCodeInt64(byte opCode, int popCount, int pushCount, long emitInt64)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 9;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode;
            EmitInt64(emitInt64);

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a two byte opcode.
        /// </summary>
        /// <param name="opCode1"> The first byte of the opcode to emit. </param>
        /// <param name="opCode2"> The second byte of the opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        private void Emit2ByteOpCode(byte opCode1, byte opCode2, int popCount, int pushCount)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 2;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode1;
            this.bytes[this.offset++] = opCode2;

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a two byte opcode plus a 2-byte operand.
        /// </summary>
        /// <param name="opCode1"> The first byte of the opcode to emit. </param>
        /// <param name="opCode2"> The second byte of the opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        /// <param name="emitInt16"> A 16-bit integer to emit. </param>
        private void Emit2ByteOpCodeInt16(byte opCode1, byte opCode2, int popCount, int pushCount, int emitInt16)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 4;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode1;
            this.bytes[this.offset++] = opCode2;
            EmitInt16(emitInt16);

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }

        /// <summary>
        /// Emits a two byte opcode plus a 4-byte operand.
        /// </summary>
        /// <param name="opCode1"> The first byte of the opcode to emit. </param>
        /// <param name="opCode2"> The second byte of the opcode to emit. </param>
        /// <param name="popCount"> The number of items to pop from the stack. </param>
        /// <param name="pushCount"> The number of items to push onto the stack. </param>
        /// <param name="emitInt32"> A 32-bit integer to emit. </param>
        private void Emit2ByteOpCodeInt32(byte opCode1, byte opCode2, int popCount, int pushCount, int emitInt32)
        {
            // Enlarge the array if necessary.
            const int instructionSize = 5;
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // Emit the instruction bytes.
            this.bytes[this.offset++] = opCode1;
            this.bytes[this.offset++] = opCode2;
            EmitInt32(emitInt32);

            // The instruction pops two values and pushes a value to the stack.
            if (this.stackSize < popCount)
                throw new InvalidOperationException("Stack underflow");
            this.stackSize += pushCount - popCount;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
        }



        //     METADATA TOKENS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets a metadata token for the given method.
        /// </summary>
        /// <param name="method"> The method to get a token for. </param>
        /// <returns> A metadata token. </returns>
        private int GetToken(System.Reflection.MethodBase method)
        {
            if (method is System.Reflection.Emit.DynamicMethod)
                return this.dynamicILInfo.GetTokenFor((System.Reflection.Emit.DynamicMethod)method);
            if (method.DeclaringType == null)
                throw new ArgumentException("The provided method cannot be that of an RTDynamicMethod. Use the DynamicMethod instead.");
            if (method.DeclaringType.IsGenericType == true)
                return this.dynamicILInfo.GetTokenFor(method.MethodHandle, method.DeclaringType.TypeHandle);
            return this.dynamicILInfo.GetTokenFor(method.MethodHandle);
        }

        /// <summary>
        /// Gets a metadata token for the given type.
        /// </summary>
        /// <param name="type"> The type to get a token for. </param>
        /// <returns> A metadata token. </returns>
        private int GetToken(Type type)
        {
            return this.dynamicILInfo.GetTokenFor(type.TypeHandle);
        }

        /// <summary>
        /// Gets a metadata token for the given field.
        /// </summary>
        /// <param name="field"> The field to get a token for. </param>
        /// <returns> A metadata token. </returns>
        private int GetToken(System.Reflection.FieldInfo field)
        {
            return this.dynamicILInfo.GetTokenFor(field.FieldHandle);
        }

        /// <summary>
        /// Gets a metadata token for the given string.
        /// </summary>
        /// <param name="string"> The string to get a token for. </param>
        /// <returns> A metadata token. </returns>
        private int GetToken(string str)
        {
            return this.dynamicILInfo.GetTokenFor(str);
        }



        //     STACK MANAGEMENT
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the number of values on the stack.
        /// </summary>
        public int StackSize
        {
            get { return this.stackSize; }
        }

        /// <summary>
        /// Gets the maximum size of the stack.
        /// </summary>
        public int MaxStackSize
        {
            get { return this.maxStackSize; }
        }

        /// <summary>
        /// Gets a value that indicates whether the stack is in an indeterminate state.
        /// </summary>
        public bool StackIsIndeterminate
        {
            get { return this.stackIsIndeterminate; }
        }

        /// <summary>
        /// Pops the value from the top of the stack.
        /// </summary>
        public override void Pop()
        {
            Emit1ByteOpCode(0x26, 1, 0);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.Float | VESType.NativeInt | VESType.Object | VESType.ManagedPointer);
        }

        /// <summary>
        /// Duplicates the value on the top of the stack.
        /// </summary>
        public override void Duplicate()
        {
            Emit1ByteOpCode(0x25, 1, 2);
#if DEBUG
            PushStackOperand(this.operands.Peek());
#endif
        }

        /// <summary>
        /// Checks the stack operands are the correct type.
        /// </summary>
        /// <param name="expectedTypes"> The operand types that are expected. </param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void PopStackOperands(params VESType[] expectedTypes)
        {
            List<VESType> actualTypes = new List<VESType>();
            for (int i = 0; i < expectedTypes.Length; i++)
                actualTypes.Add(this.operands.Pop());
            actualTypes.Reverse();
            for (int i = 0; i < expectedTypes.Length; i++)
                if ((actualTypes[i] & expectedTypes[i]) == 0)
                    throw new InvalidOperationException(string.Format("Expected argument #{0} to be: {1} but was: {2}", i + 1, expectedTypes[i], actualTypes[i]));
        }

        /// <summary>
        /// Adds the given stack operand type to the stack.
        /// </summary>
        /// <param name="type"> The operand type that were produced by the instruction. </param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void PushStackOperand(VESType type)
        {
            this.operands.Push(type);
            if (this.operands.Count != this.stackSize)
                throw new InvalidOperationException("Inconsistant internal stack sizes.");
        }

        /// <summary>
        /// Removes all values from the evaluation stack.
        /// </summary>
        private void ClearEvaluationStack()
        {
            ReplaceEvaluationStack();
        }

        /// <summary>
        /// Removes all values from the evaluation stack and adds the given operand types back.
        /// </summary>
        private void ReplaceEvaluationStack(params VESType[] operandTypes)
        {
            this.stackSize = operandTypes.Length;
            this.maxStackSize = Math.Max(this.stackSize, this.maxStackSize);
#if DEBUG
            this.operands.Clear();
            foreach (var operandType in operandTypes)
                this.operands.Push(operandType);
#endif
        }

        private enum ArithmeticOperator
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Remainder,
        }

        /// <summary>
        /// Checks that stack operands are valid for a binary arithmetic operation.
        /// </summary>
        /// <param name="operator"> The type of operation to check. </param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckArithmeticOperands(ArithmeticOperator @operator)
        {
            // Pop the operand types from the stack.
            var right = this.operands.Pop();
            var left = this.operands.Pop();

            // Enumerate all the possible combinations.
            switch (left)
            {
                case VESType.Int32:
                    if (right != VESType.Int32 && right != VESType.NativeInt && (right != VESType.ManagedPointer || @operator != ArithmeticOperator.Add))
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    PushStackOperand(right);
                    break;

                case VESType.Int64:
                    if (right != VESType.Int64)
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    PushStackOperand(VESType.Int64);
                    break;

                case VESType.NativeInt:
                    if (right != VESType.Int32 && right != VESType.NativeInt && (right != VESType.ManagedPointer || @operator != ArithmeticOperator.Add))
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    PushStackOperand(right == VESType.ManagedPointer ? VESType.ManagedPointer : VESType.NativeInt);
                    break;

                case VESType.Float:
                    if (right != VESType.Float)
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    PushStackOperand(VESType.Float);
                    break;

                case VESType.ManagedPointer:
                    if (@operator == ArithmeticOperator.Add)
                    {
                        if (right != VESType.Int32 && right != VESType.NativeInt)
                            throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    }
                    else if (@operator == ArithmeticOperator.Subtract)
                    {
                        if (right != VESType.Int32 && right != VESType.NativeInt && right != VESType.ManagedPointer)
                            throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    }
                    else
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    PushStackOperand(right == VESType.ManagedPointer ? VESType.NativeInt : VESType.ManagedPointer);
                    break;

                case VESType.Object:
                    throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
            }
        }

        private enum ComparisonOperator
        {
            Equal,
            NotEqual,
            LessThan,
            LessThanUnsigned,
            LessThanOrEqual,
            LessThanOrEqualUnsigned,
            GreaterThan,
            GreaterThanUnsigned,
            GreaterThanOrEqual,
            GreaterThanOrEqualUnsigned,
        }

        /// <summary>
        /// Checks that stack operands are valid for a binary comparison operation.
        /// </summary>
        /// <param name="operator"> The type of operation to check. </param>
        /// <param name="branchOperation"> The operation is part of a branch. </param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckComparisonOperands(ComparisonOperator @operator, bool branchOperation)
        {
            // Pop the operand types from the stack.
            var right = this.operands.Pop();
            var left = this.operands.Pop();


            // Enumerate all the possible combinations.
            switch (left)
            {
                case VESType.Int32:
                    if (right != VESType.Int32 && right != VESType.NativeInt)
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    break;

                case VESType.Int64:
                    if (right != VESType.Int64)
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    break;

                case VESType.NativeInt:
                    if (right != VESType.Int32 && right != VESType.NativeInt && (right != VESType.ManagedPointer || (@operator != ComparisonOperator.Equal && @operator != ComparisonOperator.NotEqual)))
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    break;

                case VESType.Float:
                    if (right != VESType.Float)
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    break;

                case VESType.ManagedPointer:
                    if (right != VESType.ManagedPointer && (right != VESType.NativeInt || (@operator != ComparisonOperator.Equal && @operator != ComparisonOperator.NotEqual)))
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    break;

                case VESType.Object:
                    if (right != VESType.Object || (@operator != ComparisonOperator.Equal && @operator != ComparisonOperator.NotEqual && @operator != ComparisonOperator.GreaterThanUnsigned))
                        throw new InvalidOperationException(string.Format("Invalid stack operand(s) ({0} {1} {2}).", @operator, left, right));
                    break;
            }

            if (branchOperation == false)
            {
                // All comparison operations produce an int32 as the output.
                PushStackOperand(VESType.Int32);
            }
        }

        /// <summary>
        /// Puts the stack into an indeterminate state.
        /// </summary>
        private void UnconditionalBranch()
        {
            // Unconditional branches mean the contents of the stack are indeterminate.
            this.stackIsIndeterminate = true;
        }


        //     BRANCHING AND LABELS
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a label without setting its position.
        /// </summary>
        /// <returns> A new label. </returns>
        public override ILLabel CreateLabel()
        {
            var result = new DynamicILLabel(this, this.labels.Count);
            this.labels.Add(result);
            return result;
        }

        /// <summary>
        /// Defines the position of the given label.
        /// </summary>
        /// <param name="label"> The label to define. </param>
        public override void DefineLabelPosition(ILLabel label)
        {
            if (label as DynamicILLabel == null)
                throw new ArgumentNullException("label");
            var label2 = (DynamicILLabel)label;
            if (label2.ILGenerator != this)
                throw new ArgumentException("The label wasn't created by this generator.", "label");
            if (label2.ILOffset != -1)
                throw new ArgumentException("The label position has already been defined.", "label");
            label2.ILOffset = this.offset;

#if DEBUG
            if (label2.EvaluationStack != null)
            {
                var previousStack = (VESType[])label2.EvaluationStack;
                if (this.stackIsIndeterminate == false)
                {
                    // Check the evaluation stack matches that in the label.
                    var currentStack = this.operands.ToArray();
                    if (previousStack.Length != currentStack.Length)
                        throw new InvalidOperationException(string.Format("Stack mismatch from a previous branch.  Expected: '{0}' but was: '{1}'",
                            StringHelpers.Join(", ", previousStack), StringHelpers.Join(", ", currentStack)));
                    for (int i = 0; i < previousStack.Length; i++)
                        if (previousStack[i] != currentStack[i])
                            throw new InvalidOperationException(string.Format("Stack mismatch from a previous branch.  Expected: '{0}' but was: '{1}'",
                                StringHelpers.Join(", ", previousStack), StringHelpers.Join(", ", currentStack)));
                }
                else
                {
                    // Replace the evaluation stack with the one from the label.
                    this.stackSize = previousStack.Length;
                    this.operands.Clear();
                    for (int i = previousStack.Length - 1; i >= 0; i--)
                        this.operands.Push(previousStack[i]);
                    this.stackIsIndeterminate = false;
                }
            }
#else
            if (label2.EvaluationStackSize >= 0)
            {
                if (this.stackIsIndeterminate == false)
                {
                    // Check the number of items matches.
                    if (label2.EvaluationStackSize != this.stackSize)
                        throw new InvalidOperationException(string.Format("Stack size mismatch from a previous branch.  Expected {0} items but found {1} items.",
                            label2.EvaluationStackSize, this.stackSize));
                }
                else
                {
                    // Replace the evaluation stack with the one from the label.
                    this.stackSize = label2.EvaluationStackSize;
                    this.stackIsIndeterminate = false;
                }
            }
#endif
        }

        /// <summary>
        /// Patch any undefined labels.
        /// </summary>
        private void FixLabels()
        {
            foreach (var fix in this.fixups)
            {
                // Get the IL offset of the label.
                int jumpOffset = fix.Label.ILOffset;
                if (jumpOffset < 0)
                    throw new InvalidOperationException("Undefined label.");

                // Jump offsets are relative to the next instruction.
                jumpOffset -= fix.StartOfNextInstruction;

                // Patch the jump offset;
                var position = fix.Position;
                if (fix.Length != 4)
                    throw new NotImplementedException("Short jumps are not supported.");
                this.bytes[position++] = (byte)jumpOffset;
                this.bytes[position++] = (byte)(jumpOffset >> 8);
                this.bytes[position++] = (byte)(jumpOffset >> 16);
                this.bytes[position++] = (byte)(jumpOffset >> 24);
            }
            this.fixups.Clear();
        }

        /// <summary>
        /// Unconditionally branches to the given label.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        /// <param name="opCode"> The one-byte operation identifier. </param>
        /// <param name="popCount"> The number of operands to pop from the stack. </param>
        /// <param name="popType"> The type of operand to pop from the stack. </param>
        private void BranchCore(ILLabel label, byte opCode)
        {
            // Emit the branch opcode.
            Emit1ByteOpCode(opCode, 0, 0);

            // Emit the label.
            EmitLabel(label, this.offset + 4);

            // This is an unconditional branch.
            UnconditionalBranch();
        }

        /// <summary>
        /// Conditionally branches to the given label, popping one operand from the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        /// <param name="opCode"> The one-byte operation identifier. </param>
        /// <param name="operandType"> The type of operand to pop from the stack. </param>
        private void BranchCore(ILLabel label, byte opCode, VESType operandType)
        {
            // Emit the branch opcode.
            Emit1ByteOpCode(opCode, 1, 0);

            // The instruction pops one value from the stack and pushes none.
            PopStackOperands(operandType);

            // Emit the label.
            EmitLabel(label, this.offset + 4);
        }

        /// <summary>
        /// Conditionally branches to the given label, popping two operands from the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        /// <param name="opCode"> The one-byte operation identifier. </param>
        /// <param name="@operator"> The type of comparison operation. </param>
        private void BranchCore(ILLabel label, byte opCode, ComparisonOperator @operator)
        {
            // Emit the branch opcode.
            Emit1ByteOpCode(opCode, 2, 0);

            // The instruction pops two values from the stack and pushes none.
            CheckComparisonOperands(@operator, branchOperation: true);

            // Emit the label.
            EmitLabel(label, this.offset + 4);
        }

        /// <summary>
        /// Emits a single label.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        /// <param name="startOfNextInstruction"> The IL offset of the start of the next instruction. </param>
        private void EmitLabel(ILLabel label, int startOfNextInstruction)
        {
            if (label as DynamicILLabel == null)
                throw new ArgumentNullException("label");
            var label2 = (DynamicILLabel)label;
            if (label2.ILGenerator != this)
                throw new ArgumentException("The label wasn't created by this generator.", "label");

            // Enlarge the array if necessary.
            if (this.offset + 4 >= this.bytes.Length)
                EnlargeArray(4);

            if (label2.ILOffset >= 0)
            {
                // The label is defined.
                EmitInt32(label2.ILOffset - startOfNextInstruction);
            }
            else
            {
                // The label is not defined.  Add a fix up.
                EmitInt32(0);
                this.fixups.Add(new Fixup() { Position = this.offset - 4, Length = 4, StartOfNextInstruction = startOfNextInstruction, Label = label2 });
            }

#if DEBUG
            if (label2.EvaluationStack == null)
            {
                // Copy the evaluation stack.
                label2.EvaluationStack = this.operands.ToArray();
            }
            else
            {
                // Check the evaluation stack.
                var previousStack = (VESType[])label2.EvaluationStack;
                var currentStack = this.operands.ToArray();
                if (previousStack.Length != currentStack.Length)
                    throw new InvalidOperationException(string.Format("Stack mismatch from a previous branch.  Expected: '{0}' but was: '{1}'",
                        StringHelpers.Join(", ", previousStack), StringHelpers.Join(", ", currentStack)));
                for (int i = 0; i < previousStack.Length; i++)
                    if (previousStack[i] != currentStack[i])
                        throw new InvalidOperationException(string.Format("Stack mismatch from a previous branch.  Expected: '{0}' but was: '{1}'",
                            StringHelpers.Join(", ", previousStack), StringHelpers.Join(", ", currentStack)));
            }
#else
            if (label2.EvaluationStackSize < 0)
            {
                // Record the number of items on the evaluation stack.
                label2.EvaluationStackSize = this.stackSize;
            }
            else
            {
                // Check the number of items matches.
                if (label2.EvaluationStackSize != this.stackSize)
                    throw new InvalidOperationException(string.Format("Stack size mismatch from a previous branch.  Expected {0} items but was {1} items.",
                        label2.EvaluationStackSize, this.stackSize));
            }
#endif
        }

        /// <summary>
        /// Unconditionally branches to the given label.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void Branch(ILLabel label)
        {
            BranchCore(label, 0x38);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is zero.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfZero(ILLabel label)
        {
            BranchCore(label, 0x39, VESType.Int32 | VESType.Int64 | VESType.Float |
                VESType.ManagedPointer | VESType.NativeInt | VESType.Object);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is non-zero, true or
        /// non-null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfNotZero(ILLabel label)
        {
            BranchCore(label, 0x3A, VESType.Int32 | VESType.Int64 | VESType.Float |
                VESType.ManagedPointer | VESType.NativeInt | VESType.Object);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfEqual(ILLabel label)
        {
            BranchCore(label, 0x3B, ComparisonOperator.Equal);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are not equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfNotEqual(ILLabel label)
        {
            BranchCore(label, 0x40, ComparisonOperator.NotEqual);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThan(ILLabel label)
        {
            BranchCore(label, 0x3D, ComparisonOperator.GreaterThan);
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
            BranchCore(label, 0x42, ComparisonOperator.GreaterThanUnsigned);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfGreaterThanOrEqual(ILLabel label)
        {
            BranchCore(label, 0x3C, ComparisonOperator.GreaterThanOrEqual);
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
            BranchCore(label, 0x41, ComparisonOperator.GreaterThanOrEqual);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThan(ILLabel label)
        {
            BranchCore(label, 0x3F, ComparisonOperator.LessThan);
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
            BranchCore(label, 0x44, ComparisonOperator.LessThanUnsigned);
        }

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void BranchIfLessThanOrEqual(ILLabel label)
        {
            BranchCore(label, 0x3E, ComparisonOperator.LessThanOrEqual);
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
            BranchCore(label, 0x3E, ComparisonOperator.LessThanOrEqualUnsigned);
        }

        /// <summary>
        /// Returns from the current method.  A value is popped from the stack and used as the
        /// return value.
        /// </summary>
        public override void Return()
        {
            // This instruction pops a value from the stack if the method returns a value.
            int popCount = this.dynamicMethod.ReturnType == typeof(void) ? 0 : 1;

            // ret = 2A
            Emit1ByteOpCode(0x2A, popCount, 0);

            if (this.stackIsIndeterminate == false)
            {
                // The instruction might pop a value from the stack.
                if (popCount > 0)
                    PopStackOperands(ToVESType(this.dynamicMethod.ReturnType));
                if (this.stackSize != 0)
                {
#if DEBUG
                    throw new InvalidOperationException(string.Format("The evaluation stack should be empty.  Types still on stack: {0}.", StringHelpers.Join(", ", this.operands)));
#else
                    throw new InvalidOperationException("The evaluation stack should be empty.");
#endif
                }
            }
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

            // Calculate the size of the instruction and the position of the start of the next instruction.
            int instructionSize = 1 + 4 + labels.Length * 4;
            int startOfNextInstruction = this.offset + instructionSize;

            // Enlarge the array if necessary.
            if (this.offset + instructionSize >= this.bytes.Length)
                EnlargeArray(instructionSize);

            // switch = 45
            Emit1ByteOpCode(0x45, 1, 0);

            // Emit the number of labels.
            EmitInt32(labels.Length);

            // Emit the labels.
            foreach (var label in labels)
                EmitLabel(label, startOfNextInstruction);
        }



        //     LOCAL VARIABLES AND ARGUMENTS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the local signature blob.
        /// </summary>
        public byte[] LocalSignature
        {
            get { return this.signatureHelper.GetSignature(); }
        }

        /// <summary>
        /// Declares a new local variable.
        /// </summary>
        /// <param name="type"> The type of the local variable. </param>
        /// <param name="name"> The name of the local variable. Can be <c>null</c>. </param>
        /// <returns> A new local variable. </returns>
        public override ILLocalVariable DeclareVariable(Type type, string name = null)
        {
            var result = new DynamicILLocalVariable(this, this.localVariables.Count, type, name);
            this.localVariables.Add(result);
            this.signatureHelper.AddArgument(type, false);
            return result;
        }

        /// <summary>
        /// Pushes the value of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose value will be pushed. </param>
        public override void LoadVariable(ILLocalVariable variable)
        {
            if (variable as DynamicILLocalVariable == null)
                throw new ArgumentNullException("variable");
            if (((DynamicILLocalVariable)variable).ILGenerator != this)
                throw new ArgumentException("The variable wasn't created by this generator.", "variable");

            if (variable.Index <= 3)
            {
                // ldloc.0 = 06
                // ldloc.1 = 07
                // ldloc.2 = 08
                // ldloc.3 = 09
                Emit1ByteOpCode((byte)(0x06 + variable.Index), 0, 1);
            }
            else if (variable.Index < 256)
            {
                // ldloc.s index = 11 <unsigned int8>
                Emit1ByteOpCodeInt8(0x11, 0, 1, variable.Index);
            }
            else if (variable.Index < 65535)
            {
                // ldloc index = FE 0C <unsigned int16>
                Emit2ByteOpCodeInt16(0xFE, 0x0C, 0, 1, variable.Index);
            }
            else
                throw new InvalidOperationException("Too many local variables.");

            PushStackOperand(ToVESType(variable.Type));
        }

        /// <summary>
        /// Pushes the address of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose address will be pushed. </param>
        public override void LoadAddressOfVariable(ILLocalVariable variable)
        {
            if (variable as DynamicILLocalVariable == null)
                throw new ArgumentNullException("variable");
            if (((DynamicILLocalVariable)variable).ILGenerator != this)
                throw new ArgumentException("The variable wasn't created by this generator.", "variable");

            if (variable.Index < 256)
            {
                // ldloca.s index = 12 <unsigned int8>
                Emit1ByteOpCodeInt8(0x12, 0, 1, variable.Index);
            }
            else if (variable.Index < 65535)
            {
                // ldloca index = FE 0D <unsigned int16>
                Emit2ByteOpCodeInt16(0xFE, 0x0D, 0, 1, variable.Index);
            }
            else
                throw new InvalidOperationException("Too many local variables.");

            PushStackOperand(VESType.ManagedPointer);
        }

        /// <summary>
        /// Pops the value from the top of the stack and stores it in the given local variable.
        /// </summary>
        /// <param name="variable"> The variable to store the value. </param>
        public override void StoreVariable(ILLocalVariable variable)
        {
            if (variable as DynamicILLocalVariable == null)
                throw new ArgumentNullException("variable");
            if (((DynamicILLocalVariable)variable).ILGenerator != this)
                throw new ArgumentException("The variable wasn't created by this generator.", "variable");

            if (variable.Index <= 3)
            {
                // stloc.0 = 0A
                // stloc.1 = 0B
                // stloc.2 = 0C
                // stloc.3 = 0D
                Emit1ByteOpCode((byte)(0x0A + variable.Index), 1, 0);
            }
            else if (variable.Index < 256)
            {
                // stloc.s index = 13 <unsigned int8>
                Emit1ByteOpCodeInt8(0x13, 1, 0, variable.Index);
            }
            else if (variable.Index < 65535)
            {
                // stloc index = FE 0E <unsigned int16>
                Emit2ByteOpCodeInt16(0xFE, 0x0E, 1, 0, variable.Index);
            }
            else
                throw new InvalidOperationException("Too many local variables.");

            PopStackOperands(ToVESType(variable.Type));
        }

        /// <summary>
        /// Pushes the value of the method argument with the given index onto the stack.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to push onto the stack. </param>
        public override void LoadArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentOutOfRangeException("argumentIndex");
            if (argumentIndex < 4)
                Emit1ByteOpCode((byte)(argumentIndex + 2), 0, 1);
            else if (argumentIndex < 256)
                Emit1ByteOpCodeInt8(0x0E, 0, 1, argumentIndex);
            else
                Emit2ByteOpCodeInt16(0xFE, 0x09, 0, 1, argumentIndex);
            PushStackOperand(ToVESType(this.dynamicMethod.GetParameters()[argumentIndex].ParameterType));
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
                Emit1ByteOpCodeInt8(0x10, 1, 0, argumentIndex);
            else
                Emit2ByteOpCodeInt16(0xFE, 0x0B, 1, 0, argumentIndex);
            PopStackOperands(ToVESType(this.dynamicMethod.GetParameters()[argumentIndex].ParameterType));
        }



        //     LOAD CONSTANT
        //_________________________________________________________________________________________

        /// <summary>
        /// Pushes <c>null</c> onto the stack.
        /// </summary>
        public override void LoadNull()
        {
            Emit1ByteOpCode(0x14, 0, 1);
            PushStackOperand(VESType.Object);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The integer to push onto the stack. </param>
        public override void LoadInt32(int value)
        {
            if (value >= -1 && value <= 8)
            {
                // ldc.i4.m1 = 15
                // ldc.i4.0 = 16
                // ldc.i4.1 = 17
                // ldc.i4.2 = 18
                // ldc.i4.3 = 19
                // ldc.i4.4 = 1A
                // ldc.i4.5 = 1B
                // ldc.i4.6 = 1C
                // ldc.i4.7 = 1D
                // ldc.i4.8 = 1E
                Emit1ByteOpCode((byte)(0x16 + value), 0, 1);
            }
            else if (value >= -128 && value <= 127)
            {
                // ldc.i4.s value = 1F <unsigned int8>
                Emit1ByteOpCodeInt8(0x1F, 0, 1, value);
            }
            else
            {
                // ldc.i4 value = 20 <int32>
                Emit1ByteOpCodeInt32(0x20, 0, 1, value);
            }

            // The instruction pushes a value onto the stack.
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pushes a 64-bit constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 64-bit integer to push onto the stack. </param>
        public override void LoadInt64(long value)
        {
            // ldc.i8 value = 21 <int64>
            Emit1ByteOpCodeInt64(0x21, 0, 1, value);

            // The instruction pushes a value onto the stack.
            PushStackOperand(VESType.Int64);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The number to push onto the stack. </param>
        public override void LoadDouble(double value)
        {
            // ldc.r8 = 23 <float64>
            Emit1ByteOpCodeInt64(0x23, 0, 1, BitConverter.DoubleToInt64Bits(value));

            // The instruction pushes a value onto the stack.
            PushStackOperand(VESType.Float);
        }

        /// <summary>
        /// Pushes a constant value onto the stack.
        /// </summary>
        /// <param name="value"> The string to push onto the stack. </param>
        public override void LoadString(string value)
        {
            // Get a token for the string.
            int token = this.GetToken(value);

            // ldstr = 72 <token>
            Emit1ByteOpCodeInt32(0x72, 0, 1, token);

            // The instruction pushes a value onto the stack.
            PushStackOperand(VESType.Object);
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
            Emit2ByteOpCode(0xFE, 0x01, 2, 1);
            CheckComparisonOperands(ComparisonOperator.Equal, branchOperation: false);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareGreaterThan()
        {
            Emit2ByteOpCode(0xFE, 0x02, 2, 1);
            CheckComparisonOperands(ComparisonOperator.GreaterThan, branchOperation: false);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public override void CompareGreaterThanUnsigned()
        {
            Emit2ByteOpCode(0xFE, 0x03, 2, 1);
            CheckComparisonOperands(ComparisonOperator.GreaterThanUnsigned, branchOperation: false);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public override void CompareLessThan()
        {
            Emit2ByteOpCode(0xFE, 0x04, 2, 1);
            CheckComparisonOperands(ComparisonOperator.LessThan, branchOperation: false);
        }

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public override void CompareLessThanUnsigned()
        {
            Emit2ByteOpCode(0xFE, 0x05, 2, 1);
            CheckComparisonOperands(ComparisonOperator.LessThanUnsigned, branchOperation: false);
        }



        //     ARITHMETIC AND BITWISE OPERATIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops two values from the stack, adds them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void Add()
        {
            Emit1ByteOpCode(0x58, 2, 1);
            CheckArithmeticOperands(ArithmeticOperator.Add);
        }

        /// <summary>
        /// Pops two values from the stack, subtracts the second from the first, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Subtract()
        {
            Emit1ByteOpCode(0x59, 2, 1);
            CheckArithmeticOperands(ArithmeticOperator.Subtract);
        }

        /// <summary>
        /// Pops two values from the stack, multiplies them together, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Multiply()
        {
            Emit1ByteOpCode(0x5A, 2, 1);
            CheckArithmeticOperands(ArithmeticOperator.Multiply);
        }

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// result to the stack.
        /// </summary>
        public override void Divide()
        {
            Emit1ByteOpCode(0x5B, 2, 1);
            CheckArithmeticOperands(ArithmeticOperator.Divide);
        }

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// remainder to the stack.
        /// </summary>
        public override void Remainder()
        {
            Emit1ByteOpCode(0x5D, 2, 1);
            CheckArithmeticOperands(ArithmeticOperator.Remainder);
        }

        /// <summary>
        /// Pops a value from the stack, negates it, then pushes it back onto the stack.
        /// </summary>
        public override void Negate()
        {
            Emit1ByteOpCode(0x65, 1, 1);

#if DEBUG
            var topOfStack = this.operands.Peek();
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt | VESType.Float);
            PushStackOperand(topOfStack);
#endif
        }

        /// <summary>
        /// Pops two values from the stack, ANDs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseAnd()
        {
            Emit1ByteOpCode(0x5F, 2, 1);
            PopStackOperands(VESType.Int32, VESType.Int32);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops two values from the stack, ORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseOr()
        {
            Emit1ByteOpCode(0x60, 2, 1);
            PopStackOperands(VESType.Int32, VESType.Int32);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops two values from the stack, XORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public override void BitwiseXor()
        {
            Emit1ByteOpCode(0x61, 2, 1);
            PopStackOperands(VESType.Int32, VESType.Int32);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops a value from the stack, inverts it, then pushes the result to the stack.
        /// </summary>
        public override void BitwiseNot()
        {
            Emit1ByteOpCode(0x66, 1, 1);
            PopStackOperands(VESType.Int32);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the left, then pushes the result
        /// to the stack.
        /// </summary>
        public override void ShiftLeft()
        {
            Emit1ByteOpCode(0x62, 2, 1);
            PopStackOperands(VESType.Int32, VESType.Int32);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is preserved.
        /// </summary>
        public override void ShiftRight()
        {
            Emit1ByteOpCode(0x63, 2, 1);
            PopStackOperands(VESType.Int32, VESType.Int32);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is not preserved.
        /// </summary>
        public override void ShiftRightUnsigned()
        {
            Emit1ByteOpCode(0x64, 2, 1);
            PopStackOperands(VESType.Int32, VESType.Int32);
            PushStackOperand(VESType.Int32);
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

            // Get the token for the type.
            int token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0x8C, 1, 1, token);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.Float | VESType.ManagedPointer | VESType.Object);
            PushStackOperand(VESType.Object);
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

            // Get the token for the type.
            int token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0x79, 1, 1, token);
            PopStackOperands(VESType.Object);
            PushStackOperand(VESType.ManagedPointer);
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

            // Get the token for the type.
            int token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0xA5, 1, 1, token);
            PopStackOperands(VESType.Object);
            PushStackOperand(ToVESType(type));
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a signed integer, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertToInteger()
        {
            Emit1ByteOpCode(0x69, 1, 1);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt | VESType.Float);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to an unsigned integer, then pushes it back
        /// onto the stack.
        /// </summary>
        public override void ConvertToUnsignedInteger()
        {
            Emit1ByteOpCode(0x6D, 1, 1);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt | VESType.Float);
            PushStackOperand(VESType.Int32);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a signed 64-bit integer, then pushes it
        /// back onto the stack.
        /// </summary>
        public override void ConvertToInt64()
        {
            Emit1ByteOpCode(0x6A, 1, 1);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt | VESType.Float);
            PushStackOperand(VESType.Int64);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to an unsigned 64-bit integer, then pushes it
        /// back onto the stack.
        /// </summary>
        public override void ConvertToUnsignedInt64()
        {
            Emit1ByteOpCode(0x6E, 1, 1);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt | VESType.Float);
            PushStackOperand(VESType.Int64);
        }

        /// <summary>
        /// Pops a value from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertToDouble()
        {
            Emit1ByteOpCode(0x6C, 1, 1);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt | VESType.Float);
            PushStackOperand(VESType.Float);
        }

        /// <summary>
        /// Pops an unsigned integer from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public override void ConvertUnsignedToDouble()
        {
            Emit1ByteOpCode(0x76, 1, 1);
            PopStackOperands(VESType.Int32 | VESType.Int64 | VESType.NativeInt);
            PushStackOperand(VESType.Float);
        }



        //     OBJECTS, METHODS, TYPES AND FIELDS
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops the constructor arguments off the stack and creates a new instance of the object.
        /// </summary>
        /// <param name="constructor"> The constructor that is used to initialize the object. </param>
        public override void NewObject(System.Reflection.ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            // Get the argument details.
            var parameters = constructor.GetParameters();

            var token = this.GetToken(constructor);
            Emit1ByteOpCodeInt32(0x73, parameters.Length, 1, token);

#if DEBUG
            // Check the stack.
            var operandTypes = new List<VESType>(parameters.Length);
            foreach (var parameter in parameters)
                operandTypes.Add(ToVESType(parameter.ParameterType));
            PopStackOperands(operandTypes.ToArray());
            PushStackOperand(ToVESType(constructor.DeclaringType));
#endif
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
            if (method == null)
                throw new ArgumentNullException("method");

            // Emit the call instruction.
            EmitCall(0x28, method);
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
                throw new ArgumentException("Static methods cannot be called using this method.", "method");

            // Emit the callvirt instruction.
            EmitCall(0x6F, method);
        }

        /// <summary>
        /// Pops the method arguments off the stack, calls the given method, then pushes the result
        /// to the stack (if there was one).
        /// </summary>
        /// <param name="method"> The method to call. </param>
        private void EmitCall(byte opcode, System.Reflection.MethodBase method)
        {
            // Get the argument and return type details.
            var parameters = method.GetParameters();
            Type returnType;
            if (method is System.Reflection.ConstructorInfo)
                returnType = method.DeclaringType;
            else if (method is System.Reflection.MethodInfo)
                returnType = ((System.Reflection.MethodInfo)method).ReturnType;
            else
                throw new InvalidOperationException("Unsupported subtype of MethodBase.");

            // Get a token for the method.
            int token = this.GetToken(method);

            // Call the method.
            Emit1ByteOpCodeInt32(opcode, parameters.Length + (method.IsStatic ? 0 : 1), returnType == typeof(void) ? 0 : 1, token);

#if DEBUG
            // Check the stack.
            var operandTypes = new List<VESType>(parameters.Length);
            if (method.IsStatic == false)
            {
                var declaringType = method.DeclaringType;
                if (declaringType.IsValueType == true)
                    operandTypes.Add(VESType.ManagedPointer);
                else
                    operandTypes.Add(VESType.Object);
            }
            foreach (var parameterInfo in parameters)
            {
                if (parameterInfo.ParameterType.IsByRef == true)
                    operandTypes.Add(VESType.ManagedPointer);
                else
                    operandTypes.Add(ToVESType(parameterInfo.ParameterType));
            }
            PopStackOperands(operandTypes.ToArray());
            if (returnType != typeof(void))
                PushStackOperand(ToVESType(returnType));
#endif
        }

        /// <summary>
        /// Converts a .NET type into an IL operand type.
        /// </summary>
        /// <param name="type"> The type to convert. </param>
        /// <returns> The corresponding operand type. </returns>
        private static VESType ToVESType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return VESType.Int32;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return VESType.Int64;
                case TypeCode.Single:
                case TypeCode.Double:
                    return VESType.Float;
                case TypeCode.String:
                    return VESType.Object;
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Object:
                    if (type == typeof(IntPtr))
                        return VESType.NativeInt;
                    else if (type.IsValueType == true)
                        return VESType.ManagedPointer;
                    return VESType.Object;
                default:
                    throw new NotImplementedException(string.Format("Unsupported type {0}", type));
            }
        }

        /// <summary>
        /// Pushes the value of the given field onto the stack.
        /// </summary>
        /// <param name="field"> The field whose value will be pushed. </param>
        public override void LoadField(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            int token = this.GetToken(field);
            if (field.IsStatic == true)
            {
                // ldsfld = 7E <token>
                Emit1ByteOpCodeInt32(0x7E, 0, 1, token);
            }
            else
            {
                // ldfld = 7B <token>
                PopStackOperands(VESType.Object | VESType.ManagedPointer | VESType.NativeInt);
                Emit1ByteOpCodeInt32(0x7B, 1, 1, token);
            }
            PushStackOperand(ToVESType(field.FieldType));
        }

        /// <summary>
        /// Pops a value off the stack and stores it in the given field.
        /// </summary>
        /// <param name="field"> The field to modify. </param>
        public override void StoreField(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            int token = this.GetToken(field);
            if (field.IsStatic == true)
            {
                // stsfld = 80 <token>
                PopStackOperands(ToVESType(field.FieldType));
                Emit1ByteOpCodeInt32(0x80, 1, 0, token);
            }
            else
            {
                // stfld = 7D <token>
                PopStackOperands(VESType.Object | VESType.ManagedPointer | VESType.NativeInt, ToVESType(field.FieldType));
                Emit1ByteOpCodeInt32(0x7D, 2, 0, token);
            }
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
            if (type == null)
                throw new ArgumentNullException("type");

            //if (this.EnableDiagnostics == true && ScriptEngine.LowPrivilegeEnvironment == false)
            //{
            //    // Provides more information if a cast fails.
            //    var endOfCheckLabel = this.CreateLabel();
            //    this.Duplicate();
            //    this.IsInstance(type);
            //    this.BranchIfNotNull(endOfCheckLabel);
            //    this.LoadToken(type);
            //    this.Call(ReflectionHelpers.Type_GetTypeFromHandle);
            //    this.LoadString(Environment.StackTrace);
            //    this.Call(typeof(DynamicILGenerator).GetMethod("OnFailedCast", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            //    this.Throw();
            //    this.DefineLabelPosition(endOfCheckLabel);
            //}

            int token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0x74, 1, 1, token);
            PopStackOperands(VESType.Object);
            PushStackOperand(VESType.Object);
        }

        ///// <summary>
        ///// Called if a cast fails.
        ///// </summary>
        ///// <param name="obj"> The object instance to cast. </param>
        ///// <param name="expectedType"> The expected type of the instance. </param>
        ///// <param name="stackTrace"> The stack trace when CastClass was called. </param>
        ///// <returns> An exception to throw. </returns>
        //private static Exception OnFailedCast(object obj, Type expectedType, string stackTrace)
        //{
        //    return new InvalidCastException(string.Format("Could not cast {0} to {1} --> \r\n{2} --> \r\n{3} -->",
        //        obj == null ? "null" : obj.GetType().ToString(), expectedType, stackTrace, new System.Diagnostics.StackTrace(0)));
        //}

        /// <summary>
        /// Pops an object off the stack, checks that the object inherits from or implements the
        /// given type, and pushes either the object (if the check was successful) or <c>null</c>
        /// (if the check failed) onto the stack.
        /// </summary>
        /// <param name="type"> The type of the class the object inherits from or the interface the
        /// object implements. </param>
        public override void IsInstance(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            int token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0x75, 1, 1, token);
            PopStackOperands(VESType.Object);
            PushStackOperand(VESType.Object);
        }

        /// <summary>
        /// Pushes a RuntimeTypeHandle corresponding to the given type onto the evaluation stack.
        /// </summary>
        /// <param name="type"> The type to convert to a RuntimeTypeHandle. </param>
        public override void LoadToken(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            var token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0xD0, 0, 1, token);
            PushStackOperand(VESType.ManagedPointer);
        }

        /// <summary>
        /// Pushes a RuntimeMethodHandle corresponding to the given method onto the evaluation
        /// stack.
        /// </summary>
        /// <param name="method"> The method to convert to a RuntimeMethodHandle. </param>
        public override void LoadToken(System.Reflection.MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            var token = this.GetToken(method);
            Emit1ByteOpCodeInt32(0xD0, 0, 1, token);
            PushStackOperand(VESType.ManagedPointer);
        }

        /// <summary>
        /// Pushes a RuntimeFieldHandle corresponding to the given field onto the evaluation stack.
        /// </summary>
        /// <param name="field"> The type to convert to a RuntimeFieldHandle. </param>
        public override void LoadToken(System.Reflection.FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            var token = this.GetToken(field);
            Emit1ByteOpCodeInt32(0xD0, 0, 1, token);
            PushStackOperand(VESType.ManagedPointer);
        }

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  The virtual qualifier will be ignored, if present.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        public override void LoadStaticMethodPointer(System.Reflection.MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            var token = this.GetToken(method);
            Emit2ByteOpCodeInt32(0xFE, 0x06, 0, 1, token);
            PushStackOperand(VESType.NativeInt);
        }

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  This method cannot be used to retrieve a pointer to a static method.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        /// <exception cref="ArgumentException"> The method is static. </exception>
        public override void LoadVirtualMethodPointer(System.Reflection.MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (method.IsStatic == true)
                throw new ArgumentException("The given method cannot be static.", "method");
            var token = this.GetToken(method);
            Emit2ByteOpCodeInt32(0xFE, 0x07, 0, 1, token);
            PushStackOperand(VESType.NativeInt);
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
            var token = this.GetToken(type);
            Emit2ByteOpCodeInt32(0xFE, 0x15, 1, 0, token);
            PopStackOperands(VESType.NativeInt | VESType.ManagedPointer);
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
            if (type == null)
                throw new ArgumentNullException("type");
            var token = this.GetToken(type);
            Emit1ByteOpCodeInt32(0x8D, 1, 1, token);
            PopStackOperands(VESType.Int32);
            PushStackOperand(VESType.Object);
        }

        /// <summary>
        /// Pops the array and index off the stack and pushes the element value onto the stack.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void LoadArrayElement(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    Emit1ByteOpCode(0x90, 2, 1);
                    break;
                case TypeCode.Int16:
                    Emit1ByteOpCode(0x92, 2, 1);
                    break;
                case TypeCode.Int32:
                    Emit1ByteOpCode(0x94, 2, 1);
                    break;
                case TypeCode.Int64:
                    Emit1ByteOpCode(0x96, 2, 1);
                    break;
                case TypeCode.Byte:
                    Emit1ByteOpCode(0x91, 2, 1);
                    break;
                case TypeCode.UInt16:
                    Emit1ByteOpCode(0x93, 2, 1);
                    break;
                case TypeCode.UInt32:
                    Emit1ByteOpCode(0x95, 2, 1);
                    break;
                case TypeCode.UInt64:
                    Emit1ByteOpCode(0x96, 2, 1);
                    break;
                case TypeCode.Single:
                    Emit1ByteOpCode(0x98, 2, 1);
                    break;
                case TypeCode.Double:
                    Emit1ByteOpCode(0x99, 2, 1);
                    break;
                default:
                    if (type.IsClass == true)
                        Emit1ByteOpCode(0x9A, 2, 1);
                    else
                    {
                        int token = this.GetToken(type);
                        Emit1ByteOpCodeInt32(0xA3, 2, 1, token);
                    }
                    break;
            }

            PopStackOperands(VESType.Object, VESType.Int32);
            PushStackOperand(ToVESType(type));
        }

        /// <summary>
        /// Pops the array, index and value off the stack and stores the value in the array.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public override void StoreArrayElement(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                    Emit1ByteOpCode(0x9C, 3, 0);
                    break;
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    Emit1ByteOpCode(0x9D, 3, 0);
                    break;
                case TypeCode.UInt32:
                case TypeCode.Int32:
                    Emit1ByteOpCode(0x9E, 3, 0);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    Emit1ByteOpCode(0x9F, 3, 0);
                    break;
                case TypeCode.Single:
                    Emit1ByteOpCode(0xA0, 3, 0);
                    break;
                case TypeCode.Double:
                    Emit1ByteOpCode(0xA1, 3, 0);
                    break;
                default:
                    if (type.IsClass == true)
                        Emit1ByteOpCode(0xA2, 3, 0);
                    else
                    {
                        int token = this.GetToken(type);
                        Emit1ByteOpCodeInt32(0xA4, 3, 0, token);
                    }
                    break;
            }
            PopStackOperands(VESType.Object, VESType.Int32, ToVESType(type));
        }

        /// <summary>
        /// Pops an array off the stack and pushes the length of the array onto the stack.
        /// </summary>
        public override void LoadArrayLength()
        {
            Emit1ByteOpCode(0x8E, 1, 1);
            PopStackOperands(VESType.Object);
            PushStackOperand(VESType.NativeInt);
        }



        //     EXCEPTION HANDLING
        //_________________________________________________________________________________________

        /// <summary>
        /// Pops an exception object off the stack and throws the exception.
        /// </summary>
        public override void Throw()
        {
            Emit1ByteOpCode(0x7A, 1, 0);
            PopStackOperands(VESType.Object);
            UnconditionalBranch();
        }

        /// <summary>
        /// Begins a try-catch-finally block.
        /// </summary>
        public override void BeginExceptionBlock()
        {
            // Create a new active exception region stack, if necessary.
            if (this.activeExceptionRegions == null)
                this.activeExceptionRegions = new Stack<ExceptionRegion>();

            // Create a new exception region.
            var region = new ExceptionRegion();
            region.Start = this.offset;
            region.EndLabel = this.CreateLabel();
            region.Clauses = new List<ExceptionClause>(3);

            // Push the exception region to the stack.
            this.activeExceptionRegions.Push(region);
        }

        /// <summary>
        /// Ends a try-catch-finally block.
        /// </summary>
        public override void EndExceptionBlock()
        {
            if (this.activeExceptionRegions == null || this.activeExceptionRegions.Count == 0)
                throw new InvalidOperationException("BeginExceptionBlock() must have been called before calling this method.");

            // Remove the top-most exception region from the stack.
            var exceptionRegion = this.activeExceptionRegions.Pop();
            if (exceptionRegion.Clauses.Count == 0)
                throw new InvalidOperationException("At least one catch, finally, fault or filter block is required.");

            // Close off the current exception clause.
            EndCurrentClause(exceptionRegion);

            // Define a label for the end of the exception region.
            this.DefineLabelPosition(exceptionRegion.EndLabel);

            // Add the exception region to a list for later processing.
            if (this.exceptionRegions == null)
                this.exceptionRegions = new List<ExceptionRegion>();
            this.exceptionRegions.Add(exceptionRegion);
        }

        /// <summary>
        /// Begins a catch block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        /// <param name="exceptionType"> The type of exception to handle. </param>
        public override void BeginCatchBlock(Type exceptionType)
        {
            if (exceptionType == null)
                throw new ArgumentNullException("exceptionType");
            if (this.activeExceptionRegions == null || this.activeExceptionRegions.Count == 0)
                throw new InvalidOperationException("BeginExceptionBlock() must have been called before calling this method.");

            // Get a token for the exception type.
            var exceptionTypeToken = this.GetToken(exceptionType);

            // Get the top-most exception region.
            var exceptionRegion = this.activeExceptionRegions.Peek();

            // Check there isn't already a catch block with the same type.
            foreach (var clause in exceptionRegion.Clauses)
                if (clause.Type == ExceptionClauseType.Catch && clause.CatchToken == exceptionTypeToken)
                    throw new InvalidOperationException("Multiple catch clauses with the same type are not allowed.");

            // Close off the current exception clause.
            EndCurrentClause(exceptionRegion);

            // Add a new finally clause.
            exceptionRegion.Clauses.Add(new ExceptionClause() { Type = ExceptionClauseType.Catch, ILStart = this.offset, CatchToken = exceptionTypeToken });

            // The evaluation stack starts with the exception on the top of the stack.
            ReplaceEvaluationStack(VESType.Object);
        }

        /// <summary>
        /// Begins a finally block.  BeginTryCatchFinallyBlock() must have already been called.
        /// </summary>
        public override void BeginFinallyBlock()
        {
            BeginExceptionBlock(ExceptionClauseType.Finally);
        }

        /// <summary>
        /// Begins a filter block.  BeginTryCatchFinallyBlock() must have already been called.
        /// </summary>
        public override void BeginFilterBlock()
        {
            BeginExceptionBlock(ExceptionClauseType.Filter);
        }

        /// <summary>
        /// Begins a fault block.  BeginTryCatchFinallyBlock() must have already been called.
        /// </summary>
        public override void BeginFaultBlock()
        {
            BeginExceptionBlock(ExceptionClauseType.Fault);
        }

        /// <summary>
        /// Begins a finally, filter or fault block.
        /// </summary>
        /// <param name="type"> The type of block to begin. </param>
        private void BeginExceptionBlock(ExceptionClauseType type)
        {
            if (this.activeExceptionRegions == null || this.activeExceptionRegions.Count == 0)
                throw new InvalidOperationException("BeginExceptionBlock() must have been called before calling this method.");

            // Get the top-most exception region.
            var exceptionRegion = this.activeExceptionRegions.Peek();

            if (type != ExceptionClauseType.Filter)
            {
                // Check there isn't already an identical block.
                foreach (var clause in exceptionRegion.Clauses)
                    if (clause.Type == type)
                        throw new InvalidOperationException(string.Format("Multiple {0} clauses are not allowed.", type.ToString().ToLowerInvariant()));
            }

            // Close off the current exception clause.
            EndCurrentClause(exceptionRegion);

            // Add a new clause.
            exceptionRegion.Clauses.Add(new ExceptionClause() { Type = type, ILStart = this.offset });

            // The evaluation stack starts empty for finally or fault blocks and with the exception
            // on the top of the stack for filter blocks.
            
            if (type == ExceptionClauseType.Filter)
                ReplaceEvaluationStack(VESType.Object);
            else
                ClearEvaluationStack();
        }

        /// <summary>
        /// Unconditionally branches to the given label.  Unlike the regular branch instruction,
        /// this instruction can exit out of try, filter and catch blocks.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public override void Leave(ILLabel label)
        {
            ClearEvaluationStack();
            BranchCore(label, 0xDD);
        }

        /// <summary>
        /// This instruction can be used from within a finally block to resume the exception
        /// handling process.  It is the only valid way of leaving a finally block.
        /// </summary>
        public override void EndFinally()
        {
            Emit1ByteOpCode(0xDC, 0, 0);
            UnconditionalBranch();
        }

        /// <summary>
        /// This instruction can be used from within a filter block to indicate whether the
        /// exception will be handled.  It pops an integer from the stack which should be <c>0</c>
        /// to continue searching for an exception handler or <c>1</c> to use the handler
        /// associated with the filter.  EndFilter() must be called at the end of a filter block.
        /// </summary>
        public override void EndFilter()
        {
            if (this.activeExceptionRegions == null)
                throw new InvalidOperationException("EndFilter can only be called from within an exception filter.");

            // Get the current exception clause.
            var exceptionRegion = this.activeExceptionRegions.Peek();
            if (exceptionRegion.Clauses.Count == 0)
                throw new InvalidOperationException("EndFilter can only be called from within an exception filter.");
            var latestClause = exceptionRegion.Clauses[exceptionRegion.Clauses.Count - 1];
            if (latestClause.Type != ExceptionClauseType.Filter)
                throw new InvalidOperationException("EndFilter can only be called from within an exception filter.");

            Emit2ByteOpCode(0xFE, 0x11, 1, 0);
            PopStackOperands(VESType.Int32);

            // Record the start of the handler.
            latestClause.FilterHandlerStart = this.offset;

            // The filter handler has the exception on the top of the stack.
            ClearEvaluationStack();
            PushStackOperand(VESType.Object);
        }

        /// <summary>
        /// Closes the currently open exception clause.
        /// </summary>
        /// <param name="exceptionRegion"> The exception region to modify. </param>
        private void EndCurrentClause(ExceptionRegion exceptionRegion)
        {
            if (exceptionRegion.Clauses.Count == 0)
            {
                // End the try block.
                Leave(exceptionRegion.EndLabel);
                exceptionRegion.TryLength = this.offset - exceptionRegion.Start;
            }
            else
            {
                var latestClause = exceptionRegion.Clauses[exceptionRegion.Clauses.Count - 1];
                switch (latestClause.Type)
                {
                    case ExceptionClauseType.Catch:
                        Leave(exceptionRegion.EndLabel);
                        break;
                    case ExceptionClauseType.Finally:
                        EndFinally();
                        break;
                    case ExceptionClauseType.Filter:
                        break;
                    case ExceptionClauseType.Fault:
                        EndFault();
                        break;
                }
                latestClause.ILLength = this.offset - latestClause.ILStart;
            }
        }



        //     DEBUGGING SUPPORT
        //_________________________________________________________________________________________

        /// <summary>
        /// Triggers a breakpoint in an attached debugger.
        /// </summary>
        public override void Breakpoint()
        {
            Emit1ByteOpCode(0x01, 0, 0);
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
            // DynamicMethod does not support sequence points.
        }



        //     MISC
        //_________________________________________________________________________________________

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void NoOperation()
        {
            Emit1ByteOpCode(0x00, 0, 0);
        }

    }

#endif

}
