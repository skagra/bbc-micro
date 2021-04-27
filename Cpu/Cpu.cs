using BbcMicro.Cpu.Exceptions;
using BbcMicro.Memory.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using BbcMicro.Memory.Extensions;

namespace BbcMicro.Cpu
{
    public sealed class CPU
    {
        // Trap calls, optionally replacing their operation - useful for patching in OS routines
        // Processing stops when the first callback returns true to indicate it has handled the operation
        private List<Func<CPU, OpCode, AddressingMode, ushort, bool>> _interceptionCallbacks = new List<Func<CPU, OpCode, AddressingMode, ushort, bool>>();

        // Called before each instruction is executed
        private readonly List<Action<CPU, OpCode, AddressingMode>> _preExecutionCallbacks = new List<Action<CPU, OpCode, AddressingMode>>();

        // Called after each instruction is executed
        private readonly List<Action<CPU, OpCode, AddressingMode>> _postExecutionCallbacks = new List<Action<CPU, OpCode, AddressingMode>>();

        // Implementations of each op code, passed the resolved operand and the addressing mode
        private readonly Action<ushort, AddressingMode>[] _implementations;

        // Decodes op code bytes into op code symbol and addressing mode
        private readonly Decoder _decoder = new Decoder();

        // Mapping of op code enum to implementations
        public CPU(IAddressSpace addressSpace)
        {
            Memory = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));

            _implementations = new Action<ushort, AddressingMode>[]
            {
                ADC, AND, ASL, BCC, BCS, BEQ, BIT, BMI,
                BNE, BPL, BRK, BVC, BVS, CLC, CLD, CLI,
                CLV, CMP, CPX, CPY, DEC, DEX, DEY, EOR,
                INC, INX, INY, JMP, JSR, LDA, LDX, LDY,
                LSR, NOP, ORA, PHA, PHP, PLA, PLP, ROL,
                ROR, RTI, RTS, SBC, SEC, SED, SEI, STA,
                STX, STY, TAX, TAY, TSX, TXA, TXS, TYA
            };
        }

        public void AddInterceptionCallback(Func<CPU, OpCode, AddressingMode, ushort, bool> callback)
        {
            _interceptionCallbacks.Add(callback);
        }

        public void AddPreExecutionCallback(Action<CPU, OpCode, AddressingMode> callback)
        {
            _preExecutionCallbacks.Add(callback);
        }

        public void AddPostExecutionCallback(Action<CPU, OpCode, AddressingMode> callback)
        {
            _postExecutionCallbacks.Add(callback);
        }

        public void ExecuteNextInstruction()
        {
            // Is this a valid op code?
            try
            {
                // Grab to opCode at PC
                byte opCode = Memory.GetByte(PC);

                // Find the decoder entry for the instruction
                var decoded = _decoder.Decode(opCode);

                // Calculate the actual operand using the addressing mode
                var resolvedOperand = ResolveOperand((ushort)(PC + 1), decoded.addressingMode);

                // Run pre-execution callbacks
                _preExecutionCallbacks.ForEach(callback => callback(this, decoded.opCode, decoded.addressingMode));

                // Keep a record of the current PC
                var oldPC = PC;

                // Calculate how many the current instruction and operand occupy
                var pcDelta = (byte)(_decoder.GetAddressingModePCDelta(decoded.addressingMode) + 1);

                // Interception callbacks
                var intercepted = false;
                foreach (var interceptionCallback in _interceptionCallbacks)
                {
                    // Run interception callbacks until first indicates it has been handled
                    intercepted = interceptionCallback(this, decoded.opCode, decoded.addressingMode, resolvedOperand);
                    if (intercepted)
                    {
                        break;
                    }
                }

                // Was processing intercepted?
                if (!intercepted)
                {
                    // No - then run standard instruction implementation
                    PC += pcDelta;
                    _implementations[(byte)decoded.opCode](resolvedOperand, decoded.addressingMode);
                }
                else
                {
                    // If the interception routine did not modify the PC, then skip past the current instruction
                    if (oldPC == PC)
                    {
                        PC += pcDelta;
                    }
                }

                // Call post-execution callbacks
                _postExecutionCallbacks.ForEach(callback => callback(this, decoded.opCode, decoded.addressingMode));
            }
            catch (Exception e)
            {
                throw new CPUStatefulException(this, "Error processing instruction", e, true);
            }
        }

        public void ExecuteToBrk()
        {
            while (Memory.GetByte(PC) != 0x00)
            {
                ExecuteNextInstruction();
            }
        }

        private void UpdateFlags(byte accordingToValue, PFlags flagsToUpdate)
        {
            if (flagsToUpdate.HasFlag(PFlags.Z))
            {
                PSet(PFlags.Z, accordingToValue == 0);
            }

            if (flagsToUpdate.HasFlag(PFlags.N))
            {
                PSet(PFlags.N, (accordingToValue & 0b10000000) != 0);
            }
        }

        /*
         * Op code implementations
         *
         * Definitions are taken from http://www.obelisk.me.uk/6502/reference.html
         */

        /*
         * Add with carry
         *
         * This instruction adds the contents of a memory location to the accumulator
         * together with the carry bit. If overflow occurs the carry bit is set,
         * this enables multiple byte addition to be performed.
         */

        private void ADC(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            short result = (short)(A + operandValue + (PIsSet(PFlags.C) ? 1 : 0));
            PSet(PFlags.C, result > 0xFF);
            PSet(PFlags.V, ((A ^ result) & (operandValue ^ result) & 0x80) != 0);
            A = (byte)result;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Logical and
         *
         * A logical AND is performed, bit by bit, on the accumulator contents
         * using the contents of a byte of memory.
         */

        private void AND(ushort operand, AddressingMode addressingMode)
        {
            A &= GetByteValue(operand, addressingMode);
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Arithmetic shift left
         *
         * This operation shifts all the bits of the accumulator or memory contents one
         * bit left. Bit 0 is set to 0 and bit 7 is placed in the carry flag. The
         * effect of this operation is to multiply the memory contents by 2
         * (ignoring 2's complement considerations), setting the carry if the result
         * will not fit in 8 bits.
         */

        private void ASL(ushort operand, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                PSet(PFlags.C, (A & 0b1000_0000) != 0);
                A = (byte)(A << 1);
                UpdateFlags(A, PFlags.N | PFlags.Z);
            }
            else
            {
                var operandValue = GetByteValue(operand, addressingMode);
                Memory.SetByte((byte)(operandValue << 1), operand);
                PSet(PFlags.C, (operandValue & 0b1000_0000) != 0);
                UpdateFlags(operandValue, PFlags.N | PFlags.Z);
            }
        }

        /*
         * Branch if carry clear
         *
         * If the carry flag is clear then add the relative displacement to the
         * program counter to cause a branch to a new location.
         */

        private void BCC(ushort operand, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.C))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Branch if carry set
         *
         * If the carry flag is set then add the relative displacement to the
         * program counter to cause a branch to a new location.
         */

        private void BCS(ushort operand, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.C))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Branch if equal
         *
         * If the zero flag is set then add the relative displacement to
         * the program counter to cause a branch to a new location.
         */

        private void BEQ(ushort operand, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.Z))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Bit test
         *
         * This instructions is used to test if one or more bits are set
         * in a target memory location. The mask pattern in A is ANDed with
         * the value in memory to set or clear the zero flag, but the result
         * is not kept. Bits 7 and 6 of the value from memory are copied into
         * the N and V flags.
         */

        private void BIT(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            PSet(PFlags.Z, (A & operandValue) == 0);
            PSet(PFlags.V, (operandValue & 0b0100_0000) != 0);
            PSet(PFlags.N, (operandValue & 0b1000_0000) != 0);
        }

        /*
         * Branch if minus
         *
         * If the negative flag is set then add the relative displacement to
         * the program counter to cause a branch to a new location.
         */

        private void BMI(ushort operand, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.N))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Branch if not equal
         *
         * If the zero flag is clear then add the relative displacement
         * to the program counter to cause a branch to a new location.
         */

        private void BNE(ushort operand, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.Z))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Branch if positive
         *
         * If the negative flag is clear then add the relative displacement
         * to the program counter to cause a branch to a new location.
         */

        private void BPL(ushort operand, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.N))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Force Interrupt
         *
         * The BRK instruction forces the generation of an interrupt request.
         * The program counter and processor status are pushed on the stack then
         * the IRQ interrupt vector at $FFFE/F is loaded into the PC and the break
         * flag in the status set to one.
         *
         * The interpretation of a BRK depends on the operating system. On the BBC
         * Microcomputer it is used by language ROMs to signal run time errors but
         * it could be used for other purposes (e.g. calling operating system
         * functions, etc.).
         */

        private void BRK(ushort operandAddress, AddressingMode addressingMode)
        {
            // TODO
            throw new NotImplementedException("BRK");
        }

        /*
         * Branch if overflow clear
         *
         * If the overflow flag is clear then add the relative displacement to
         * the program counter to cause a branch to a new location.
         */

        private void BVC(ushort operand, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.V))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Branch if overflow set
         *
         * If the overflow flag is set then add the relative displacement to
         * the program counter to cause a branch to a new location.
         */

        private void BVS(ushort operand, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.V))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
            }
        }

        /*
         * Clear carry
         */

        private void CLC(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.C);
        }

        /*
         * Clear decimal mode
         */

        private void CLD(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.D);
        }

        /*
         * Clear interrupt disable
         *
         * Clears the interrupt disable flag allowing normal interrupt requests
         * to be serviced.
         */

        private void CLI(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.I);
        }

        /*
         * Clear overflow flag
         */

        private void CLV(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.V);
        }

        /*
         * Compare
         *
         * This instruction compares the contents of the accumulator with another
         * memory held value and sets the zero and carry flags as appropriate.
         */

        private void CMP(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            PSet(PFlags.N, A < operandValue);
            PSet(PFlags.C, A >= operandValue);
            PSet(PFlags.Z, A == operandValue);
        }

        /*
         * Compare X register
         *
         * This instruction compares the contents of the X register with another
         * memory held value and sets the zero and carry flags as appropriate.
         */

        private void CPX(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            PSet(PFlags.N, X < operandValue);   
            PSet(PFlags.C, X >= operandValue);
            PSet(PFlags.Z, X == operandValue);
        }

        /*
         * Compare Y register
         *
         * This instruction compares the contents of the Y register with another
         * memory held value and sets the zero and carry flags as appropriate.
         */

        private void CPY(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            PSet(PFlags.N, Y < operandValue); 
            PSet(PFlags.C, Y >= operandValue);
            PSet(PFlags.Z, Y == operandValue);
        }

        /*
         * Decrement memory
         *
         * Subtracts one from the value held at a specified memory location
         * setting the zero and negative flags as appropriate.
         */

        private void DEC(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            byte result = (byte)(operandValue - 1);
            Memory.SetByte(result, operand);
            UpdateFlags(result, PFlags.N | PFlags.Z);
        }

        /*
         * Decrement X register
         *
         * Subtracts one from the X register setting the zero and negative
         * flags as appropriate.
         */

        private void DEX(ushort operand, AddressingMode addressingMode)
        {
            X -= 1;
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Decrement Y register
         *
         * Subtracts one from the Y register setting the zero and negative
         * flags as appropriate.
         */

        private void DEY(ushort operand, AddressingMode addressingMode)
        {
            Y -= 1;
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Exclusive or
         *
         * An exclusive OR is performed, bit by bit, on the accumulator
         * contents using the contents of a byte of memory.
         */

        private void EOR(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            A ^= operandValue;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Increment memory
         *
         * Adds one to the value held at a specified memory location
         * setting the zero and negative flags as appropriate.
         */

        private void INC(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            operandValue++;
            Memory.SetByte(operandValue, operand);
            UpdateFlags(operandValue, PFlags.N | PFlags.Z);
        }

        /*
         * Increment X register
         *
         * Adds one to the X register setting the zero and negative flags
         * as appropriate.
         */

        private void INX(ushort operand, AddressingMode addressingMode)
        {
            X++;
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Increment Y register
         *
         * Adds one to the X register setting the zero and negative flags
         * as appropriate.
         */

        private void INY(ushort operand, AddressingMode addressingMode)
        {
            Y++;
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Jump
         *
         * Sets the program counter to the address specified by the operand.
         *
         * An original 6502 has does not correctly fetch the target address if
         * the indirect vector falls on a page boundary (e.g. $xxFF where xx is
         * any value from $00 to $FF). In this case fetches the LSB from $xxFF
         * as expected but takes the MSB from $xx00. This is fixed in some later
         * chips like the 65SC02 so for compatibility always ensure the indirect
         * vector is not at the end of the page.
         */

        private void JMP(ushort operand, AddressingMode addressingMode)
        {   
            PC = operand;
        }

        /*
         * Jump to Subroutine
         *
         * The JSR instruction pushes the address (minus one) of the return point
         * on to the stack and then sets the program counter to the target memory
         * address.
         */

        private void JSR(ushort operand, AddressingMode addressingMode)
        {
            // Operand is in host machine byte order, it is always the absolute address to target
            ushort addressToPush = (ushort)(PC - 1); // This is what the 6502 does!
            byte operandLSB = (byte)addressToPush;
            byte operandMSB = (byte)(addressToPush >> 8);

            // Push the return address onto the stack
            // MSB first to match little endian byte order as the stack grows downwards
            PushByte(operandMSB);
            PushByte(operandLSB);

            PC = operand;
        }

        /*
         * Load accumulator
         *
         * Loads a byte of memory into the accumulator setting the
         * zero and negative flags as appropriate.
         */

        private void LDA(ushort operand, AddressingMode addressingMode)
        {
            A = GetByteValue(operand, addressingMode);
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Load X register
         *
         * Loads a byte of memory into the X register setting the zero
         * and negative flags as appropriate.
         */

        private void LDX(ushort operand, AddressingMode addressingMode)
        {
            X = GetByteValue(operand, addressingMode);
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Load Y register
         *
         * Loads a byte of memory into the Y register setting the zero
         * and negative flags as appropriate.
         */

        private void LDY(ushort operand, AddressingMode addressingMode)
        {
            Y = GetByteValue(operand, addressingMode);
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Logical shift right
         *
         * Each of the bits in A or M is shift one place to the right. The
         * bit that was in bit 0 is shifted into the carry flag. Bit 7 is set
         * to zero.
         */

        private void LSR(ushort operand, AddressingMode addressingMode)
        {
            if (addressingMode==AddressingMode.Accumulator)
            {
                PSet(PFlags.C, (A & 00000_0001) != 0);
                A = (byte)(A >> 1);
                UpdateFlags(A, PFlags.N | PFlags.Z);
            }
            else
            {
                byte operandValue = GetByteValue(operand, addressingMode);
                byte shiftedValue = (byte)(operandValue >> 1);
                Memory.SetByte(shiftedValue, operand);
                PSet(PFlags.C, (operandValue & 00000_0001) != 0);
                UpdateFlags(shiftedValue, PFlags.N | PFlags.Z);
            }
        }

        /*
         * No operation
         *
         * The NOP instruction causes no changes to the processor other
         * than the normal incrementing of the program counter to the next
         * instruction.
         */

        private void NOP(ushort operand, AddressingMode addressingMode)
        {
        }

        /*
         * Logical inclusive or
         *
         * An inclusive OR is performed, bit by bit, on the accumulator
         * contents using the contents of a byte of memory.
         */

        private void ORA(ushort operand, AddressingMode addressingMode)
        {
            A |= GetByteValue(operand, addressingMode);
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Push accumulator
         *
         * Pushes a copy of the accumulator on to the stack.
         */

        private void PHA(ushort operand, AddressingMode addressingMode)
        {
            PushByte(A);
        }

        /*
         * Push processor status
         *
         * Pushes a copy of the status flags on to the stack.
         */

        private void PHP(ushort operandAddress, AddressingMode addressingMode)
        {
            PushByte(P);
        }

        /*
         * Pull accumulator
         *
         * Pulls an 8 bit value from the stack and into the accumulator.
         * The zero and negative flags are set as appropriate.
         */

        private void PLA(ushort operandAddress, AddressingMode addressingMode)
        {
            A=PopByte();
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Pull processor status
         *
         * Pulls an 8 bit value from the stack and into the processor flags.
         * The flags will take on new states as determined by the value pulled.
         */

        private void PLP(ushort operandAddress, AddressingMode addressingMode)
        {
            P = PopByte();
        }

        /*
         * Rotate left
         *
         * Move each of the bits in either A or M one place to the left. Bit 0
         * is filled with the current value of the carry flag whilst the old
         * bit 7 becomes the new carry flag value.
         */

        private void ROL(ushort operand, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                byte operandValue=A;
                A = (byte)((byte)(A << 1) | (byte)(PIsSet(PFlags.C) ? 0b0000_0001 : 0b0000_0000));
                PSet(PFlags.C, (operandValue & 0b1000_0000) != 0);
                UpdateFlags(A, PFlags.N | PFlags.Z);

            }
            else
            {
                byte operandValue = GetByteValue(operand, addressingMode);
                byte rotateResult = (byte)((byte)(operandValue << 1) | (byte)(PIsSet(PFlags.C) ? 0b0000_0001 : 0b0000_0000));
                Memory.SetByte(rotateResult, operand);
                PSet(PFlags.C, (operandValue & 0b1000_0000) != 0);
                UpdateFlags(rotateResult, PFlags.N | PFlags.Z);
            }
        }

        /*
         * Rotate right
         *
         * Move each of the bits in either A or M one place to the right. Bit 7
         * is filled with the current value of the carry flag whilst the old
         * bit 0 becomes the new carry flag value.
         */

        private void ROR(ushort operand, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                byte operandValue=A;
                A = (byte)((byte)(A >> 1) | (byte)(PIsSet(PFlags.C) ? 0b1000_0000 : 0b0000_000));
                PSet(PFlags.C, (operandValue & 0b0000_0001) != 0);
                UpdateFlags(A, PFlags.N | PFlags.Z);
            }
            else
            {
                byte operandValue = GetByteValue(operand, addressingMode);
                byte rotatedResult = (byte)((byte)(operandValue >> 1) | (byte)(PIsSet(PFlags.C) ? 0b1000_0000 : 0b0000_000));
                Memory.SetByte(rotatedResult, operand);
                PSet(PFlags.C, (operandValue & 0b0000_0001) != 0);
                UpdateFlags(rotatedResult, PFlags.N | PFlags.Z);
            }
        }

        /*
         * Return from interrupt
         *
         * The RTI instruction is used at the end of an interrupt processing
         * routine. It pulls the processor flags from the stack followed by
         * the program counter.
         */

        private void RTI(ushort operand, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Return from subroutine
         *
         * The RTS instruction is used at the end of a subroutine to return
         * to the calling routine. It pulls the program counter (minus one) from the stack.
         */

        private void RTS(ushort operandAddress, AddressingMode addressingMode)
        {
            PC = (ushort)(PopByte() + (PopByte() << 8) + 1);
        }

        /*
         * Subtract with carry
         *
         * This instruction subtracts the contents of a memory location to the
         * accumulator together with the not of the carry bit. If overflow occurs
         * the carry bit is clear, this enables multiple byte subtraction to be
         * performed.
         */

        private void SBC(ushort operand, AddressingMode addressingMode)
        {
            byte operandValue = GetByteValue(operand, addressingMode);
            short subtractWithCarryResult = (short)(A - operandValue - (PIsSet(PFlags.C) ? 0 : 1));
            PSet(PFlags.C, subtractWithCarryResult >= 0);
            PSet(PFlags.V, ((A ^ subtractWithCarryResult) & (operandValue ^ subtractWithCarryResult) & 0x80) != 0);
            A = (byte)subtractWithCarryResult;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Set carry flag
         */

        private void SEC(ushort operand, AddressingMode addressingMode)
        {
            PSet(PFlags.C);
        }

        /*
         * Set decimal flag
         */

        private void SED(ushort operand, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Set interrupt disable
         */

        private void SEI(ushort operand, AddressingMode addressingMode)
        {
            PSet(PFlags.I);
        }

        /*
         * Store accumulator
         *
         * Stores the contents of the accumulator into memory.
         */

        private void STA(ushort operand, AddressingMode addressingMode)
        {
            Memory.SetByte(A, operand);
        }

        /*
         * Store X register
         *
         * Stores the contents of the X register into memory.
         */

        private void STX(ushort operand, AddressingMode addressingMode)
        {
            Memory.SetByte(X, operand);
        }

        /*
         * Store Y register
         *
         * Stores the contents of the Y register into memory.
         */

        private void STY(ushort operand, AddressingMode addressingMode)
        {
            Memory.SetByte(Y, operand);
        }

        /*
         * Transfer accumulator to X
         *
         * Copies the current contents of the accumulator into the X
         * register and sets the zero and negative flags as appropriate.
         */

        private void TAX(ushort operand, AddressingMode addressingMode)
        {
            X = A;
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Transfer accumulator to Y
         *
         * Copies the current contents of the accumulator into the Y
         * register and sets the zero and negative flags as appropriate.
         */

        private void TAY(ushort operand, AddressingMode addressingMode)
        {
            Y = A;
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Transfer stack pointer to X
         *
         * Copies the current contents of the stack register into
         * the X register and sets the zero and negative flags as appropriate.
         */

        private void TSX(ushort operand, AddressingMode addressingMode)
        {
            X = S;
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Transfer X to accumulator
         *
         * Copies the current contents of the X register into the
         * accumulator and sets the zero and negative flags as appropriate.
         */

        private void TXA(ushort operand, AddressingMode addressingMode)
        {
            A = X;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Transfer X to stack pointer
         *
         * Copies the current contents of the X register into the stack register.
         */

        private void TXS(ushort operand, AddressingMode addressingMode)
        {
            S = X;
            UpdateFlags(S, PFlags.N | PFlags.Z);
        }

        /*
         * Transfer Y to accumulator
         *
         * Copies the current contents of the Y register into the accumulator
         * and sets the zero and negative flags as appropriate.
         */

        private void TYA(ushort operand, AddressingMode addressingMode)
        {
            A = Y;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * CPU state
         */

        // Program counter
        public ushort PC { get; set; }

        // Stack pointer
        public byte S { get; set; } = 0xFF;

        // Accumulator
        public byte A { get; set; }

        // Index register X
        public byte X { get; set; }

        // Index register Y
        public byte Y { get; set; }

        // Processor status
        public byte P { get; set; }

        [Flags]
        public enum PFlags : byte
        {
            C = 0x01, // Carry
            Z = 0x02, // Zero result
            I = 0x04, // Interrupt disable
            D = 0x08, // Decimal mode
            B = 0x10, // Break command
            X = 0x20, // Reserved for expansion
            V = 0x40, // Overflow
            N = 0x80  // Negative result
        }

        public bool PIsSet(PFlags flags)
        {
            return (P & (byte)flags) == (byte)flags;
        }

        public void PSet(PFlags flags)
        {
            P = (byte)(P | (byte)flags);
        }

        public void PReset(PFlags flags)
        {
            P = (byte)(P & ~(byte)flags);
        }

        public void PSet(PFlags flags, bool value)
        {
            if (value)
            {
                PSet(flags);
            }
            else
            {
                PReset(flags);
            }
        }

        private const ushort STACK_BASE_ADRESS = 0x100;

        public void PushByte(byte value)
        {
            if (S == 0)
            {
                throw new CPUStatefulException(this, "Stack overflow", true);
            }
            Memory.SetByte(value, (ushort)(STACK_BASE_ADRESS + S));
            S--;
        }

        public byte PopByte()
        {
            if (S == 0xFF)
            {
                throw new CPUStatefulException(this, "Stack underflow", true);
            }
            S++;
            return Memory.GetByte((ushort)(STACK_BASE_ADRESS + S));
        }

        public IAddressSpace Memory { get; }

        /*
         * Addressing modes
         */

        /*
         * Follows a value returned by ResolveOperand() to the actual value to process
         */

        private byte GetByteValue(ushort operand, AddressingMode addressingMode)
        {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            return addressingMode switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            {
                AddressingMode.Accumulator => A,
                AddressingMode.Immediate => (byte)operand,
                AddressingMode.Implied => 0,
                AddressingMode.Relative => (byte)operand,
                AddressingMode.Absolute => Memory.GetByte(operand),
                AddressingMode.ZeroPage => Memory.GetByte(operand),
                AddressingMode.Indirect => 0, 
                AddressingMode.AbsoluteIndexedX => Memory.GetByte(operand),
                AddressingMode.AbsoluteIndexedY => Memory.GetByte(operand),
                AddressingMode.ZeroPageIndexedX => Memory.GetByte(operand),
                AddressingMode.ZeroPageIndexedY => Memory.GetByte(operand),
                AddressingMode.IndexedXIndirect => Memory.GetByte(operand),
                AddressingMode.IndirectIndexedY => Memory.GetByte(operand)
            };
        }

        /*
         * Resolves the address of the operand to the actual operand value that an instruction would process.
         * Results are always in host byte order (which may or may not match 6502 byte ordering)
         * When the operand is byte it is returned in the least significant byte of the result
         */

        private ushort ResolveOperand(ushort operandAddress, AddressingMode addressingMode)
        {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            return addressingMode switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            {
                AddressingMode.Accumulator => 0, // Operand ignored
                AddressingMode.Immediate => Memory.GetByte(operandAddress), // Operand is the byte following the op code
                AddressingMode.Implied => 0, // Operand ignored
                AddressingMode.Relative => Memory.GetByte(operandAddress), // Operand is the byte following the op code
                AddressingMode.Absolute => Memory.GetNativeWord(operandAddress), // Operand is the word following the op code
                AddressingMode.ZeroPage => Memory.GetByte(operandAddress), // Operand is the byte following the op code
                AddressingMode.Indirect => Memory.GetNativeWord(Memory.GetNativeWord(operandAddress)), // Operand is pointed to by the word following the op code
                AddressingMode.AbsoluteIndexedX => (ushort)(Memory.GetNativeWord(operandAddress) + X), // Operand is the word following the op code + X
                AddressingMode.AbsoluteIndexedY => (ushort)(Memory.GetNativeWord(operandAddress) + Y), // Operand is the word following the op code + Y
                AddressingMode.ZeroPageIndexedX => (byte)(Memory.GetByte(operandAddress) + X), // Cast to byte to wrap to zero page
                AddressingMode.ZeroPageIndexedY => (byte)(Memory.GetByte(operandAddress) + Y), // Cast to byte to wrap to zero page
                AddressingMode.IndexedXIndirect => Memory.GetNativeWord((byte)(Memory.GetByte(operandAddress) + X)), // Cast to byte to wrap to zero page
                AddressingMode.IndirectIndexedY => (ushort)(Memory.GetNativeWord(Memory.GetByte(operandAddress)) + Y),
            };
        }

        public override string ToString()
        {
            return $"PC=0x{PC:X4}, S=0x{S:X2} A=0x{A:X2}, X=0x{X:X2}, Y=0x{Y:X2}, " +
                $"P=0x{P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => (PIsSet(flag) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})";
        }
    }
}