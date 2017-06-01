using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCPU : MonoBehaviour {
    public static int MEM_SIZE = 0x10000;
    public static int REG_SIZE = 0x8;
    ushort aRef, bRef;
    ushort aMode, bMode;

    public enum varMode {MEM_MODE, REG_MODE, LIT_MODE, PC_MODE, SP_MODE, EX_MODE};
    public enum opCodes {spi, SET, ADD, SUB, MUL, MLI, DIV, DVI, MOD, MDI, AND, BOR, XOR, SHR, ASR, SHL, IFB, IFC, IFN, IFG, IFA, IFL, IFU, NU18, NU19, ADX, NU1C, NU1D, STI, STD}

    // takes the current dcpu state 
    // moves it one cycle forward
    // and returns a  new state
    public dcpuState step(dcpuState state){
        dcpuState oldState = state.copy(); 
        ushort addr = state.memory[state.PC++];
        ushort aa = (ushort)((addr >> 10) & 0x3F), bb = (ushort)((addr >> 5) & 0x1F), op = (ushort)((addr) & 0x1F);
        ushort nxtA = 0, nxtB = 0;

        if(op != 0){
            if(needsNextVar(aa)){
                nxtA = state.memory[state.PC++];
                tick(state);
            }
            if (needsNextVar(bb)){
                nxtB = state.memory[state.PC++];
                tick(state);
            }
            ushort a = handleVar(state, aa, nxtA, false);
            ushort b = handleVar(state, bb, nxtB, true);
            handleOpCode(state, op, b, a);
        }
        else{
            
        }
        tick(state);
        return state;
    }



    private ushort handleVar(dcpuState state, ushort value, ushort nxtVar, bool isB){
        if(value <= 0x07){ // register (A, B, C, X, Y, Z, I, J)
            if (isB){
                bMode = (ushort)varMode.REG_MODE;
                bRef = value;
            }
            return state.registers[value];

        }else if(value <= 0x0F){ // [register]
            if (isB){
                bMode = (ushort)varMode.MEM_MODE;
                bRef = state.registers[value % REG_SIZE];
            }
            return state.memory[state.registers[value & REG_SIZE]];

        }else if(value <= 0x17){ // [register + next word]
            if(isB){
                bMode = (ushort)varMode.MEM_MODE;
                bRef = (ushort)(state.registers[value % REG_SIZE] + nxtVar);
            }
            return state.memory[state.registers[value % REG_SIZE] + nxtVar];

        }else if(value == 0x18){ // PUSH/[--SP] POP/[SP++]
            if(isB){
            bMode = (ushort)varMode.MEM_MODE;
                ushort temp = --state.SP;
            bRef = (temp);
            return state.memory[temp];
            }else{
                return state.memory[state.SP++];
            }
        }else if(value == 0x19){ // [SP] / PEEK
            if(isB){
                bMode = (ushort)varMode.MEM_MODE;
                bRef = state.SP;
            }
            return state.memory[state.SP];

        }else if(value == 0x1A){ // [SP + next word] / PICK n
            if(isB){
                bMode = (ushort)varMode.MEM_MODE;
                bRef = (ushort)(state.SP + nxtVar);
            }
            return state.memory[state.SP + nxtVar];

        }else if(value == 0x1B){ // SP
            if(isB){
                bMode = (ushort)varMode.SP_MODE;
                bRef = state.SP; 
            }
            return state.SP;

        } else if(value == 0x1C){ // PC
            if(isB){
                bMode = (ushort)varMode.PC_MODE;
                bRef = state.PC;
            }
            return state.PC;

        }else if(value == 0x1D){ // EX
            if(isB){
                bMode = (ushort)varMode.EX_MODE;
                bRef = state.EX;
            }
            return state.EX;

        }else if(value == 0x1E){ // [next word]
            if(isB){
                bMode = (ushort)varMode.MEM_MODE;
                bRef = nxtVar;
            }
            return state.memory[nxtVar];

        }else if(value == 0x1F){ // next word (lit)
            if(isB){
                bMode = (ushort)varMode.LIT_MODE;
                bRef = nxtVar;
            }
            return nxtVar;
        }else if(value >= 0x20 && value <= 0x3F){
            if (isB){
                bMode = (ushort)varMode.LIT_MODE;
                bRef = (ushort)(value - 33);
            }
            return (ushort)(value - 33);
        }else{
            //this is for if the a or b variable exceeds its boudns 
        }
        return 0;
    }
    
    private void handleOpCode(dcpuState state, ushort op, ushort b, ushort a){

        ushort res = 0; // temp container for result
        bool needsProcessing = true; // this will only be true if data needs to be stored back to the dcpu
        switch(op){
            case (ushort)opCodes.SET: //sets b to a
                res = a;
                tick(state);
                break;

            case (ushort)opCodes.ADD: //add b and a together and stores it in b
                res = (ushort)(b + a);
                state.EX = (ushort)((b + a) % 0xFFFF);
                tick(state, 2);
                break;

            case (ushort)opCodes.SUB: //subtracts b from a
                res = (ushort)(b - a);
                if ((b - a) < 0) state.EX = 0xFFFF;
                tick(state, 2);
                break;

            case (ushort)opCodes.MUL: // multiply b and a unsgined
                res = (ushort)(b * a);
                state.EX = (ushort)(((b * a) >> 16) & 0xFFFF);
                tick(state, 2);
                break;

            case (ushort)opCodes.MLI: // multiply b and a signed
                res = (ushort)((short)b * (short)a);
                state.EX = (ushort)((((short)b * (short)a) >> 16) & 0xFFFF);
                break;

            case (ushort)opCodes.DIV: // divide b by a signed
                if (a == 0) { state.EX = 0;}
                else{res = (ushort)(b / a); state.EX = (ushort)(((b << 16) / a) &0xFFFF);}
                tick(state, 3);
                break;

            case (ushort)opCodes.DVI: // divide b by a unsigned
                if (a == 0) { state.EX = 0; }
                else { res = (ushort)((short)b / (short)a); state.EX = (ushort)((((short)b << 16) / (short)a) & 0xFFFF); }
                tick(state, 3);
                break;

            case (ushort)opCodes.MOD: // gets the modulus of b using a signed
                if(a == 0){ res = b;}
                else{res = (ushort)(b % a); }
                tick(state, 3);
                break;

            case (ushort)opCodes.MDI: // gets the modulus of b using a unsigned
                if(a == 0) { res = b; }
                else { res = (ushort)((short)b % (short)a); }
                tick(state, 3);
                break;

            case (ushort)opCodes.AND: // and b and a together
                res = (ushort)(b & a);
                tick(state);
                break;

            case (ushort)opCodes.BOR: // its or's a and b together 
                res = (ushort)(b | a);
                tick(state);
                break;

            case (ushort)opCodes.XOR: // exclusively or's a and b
                res = (ushort)(b ^ a);
                tick(state);
                break;

            case (ushort)opCodes.SHR: // does a logical right shift
                res = (ushort)(b >> a);
                state.EX = (ushort)(((b << 16) >> a) & 0xFFFF);
                tick(state);
                break;

            case (ushort)opCodes.ASR: // does a arithmatic right shift
                res = (ushort)((short)b >> (short)a);
                state.EX = (ushort)((((short)b << 16) >> (short)a) & 0xFFFF);
                tick(state);
                break;

            case (ushort)opCodes.SHL: // shifts to the left
                res = (ushort)(b << a);
                state.EX = (ushort)(((b << a) >> 16) & 0xFFFF);
                tick(state);
                break;

            case (ushort)opCodes.IFB: // 
                ushort amt = ifJumpNum(state);
                if((b & a) == 0) { state.PC += amt; tick(state, amt);}
                tick(state);
                needsProcessing = false;
                break;

        }
        if(needsProcessing){
            if(bMode == (ushort)varMode.MEM_MODE){
                state.memory[bRef] = res;
            }else if(bMode == (ushort)varMode.REG_MODE){
                state.registers[bRef] = res;
            }else if(bMode == (ushort)varMode.PC_MODE){
                state.PC = res;
            }else if(bMode == (ushort)varMode.SP_MODE){
                state.SP = res;
            }else if(bMode == (ushort)varMode.EX_MODE){
                state.EX = res;
            } else{

            }
        }
    }

    //utils
    private void tick(dcpuState state, int n = 1){
        state.ticks += n;
    }

    private bool needsNextVar(ushort value){
        if ((value >= 0x10 && value < 0x17) || value == 0xA1 || value == 0x1E || value == 0x1F)
        {
            return true;
        }
        return false;
    }

    private ushort ifJumpNum(dcpuState state){
        ushort amount = 1;
        ushort addr = state.memory[state.PC + 1];
        ushort aa = (ushort)((addr >> 10) & 0x3F), bb = (ushort)((addr >> 5) & 0x1F);
        if (needsNextVar(aa))
            amount++;
        if (needsNextVar(bb))
            amount++;
        return amount;
    }
}
