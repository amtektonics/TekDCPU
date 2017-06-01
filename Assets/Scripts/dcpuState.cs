
public class dcpuState{
    public ushort[] memory;
    public ushort[] registers;
    public ushort PC, EX, SP, IA;
    public long ticks;
    public dcpuState(){
        memory    = new ushort[DCPU.MEM_SIZE];
        registers = new ushort[DCPU.REG_SIZE];
        PC = 0;
        EX = 0;
        SP = 0;
        IA = 0;
    }
    public dcpuState copy(){
        dcpuState newState = new dcpuState();
        newState.memory = this.memory;
        newState.registers = this.registers;
        newState.PC = this.PC;
        newState.EX = this.EX;
        newState.SP = this.SP;
        newState.IA = this.IA;
        return newState;
    }

}
