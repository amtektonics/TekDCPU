
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


    public dcpuState(byte[] data) {
        memory = new ushort[DCPU.MEM_SIZE];
        registers = new ushort[DCPU.REG_SIZE];
        PC = 0; EX = 0; SP = 0; IA = 0;
        ticks = 0;

        for (int i = 0; i < DCPU.MEM_SIZE; i++){
            memory[i] = (ushort)((data[i * 2] << 4 ) | data[(i * 2) + 1]);
        }

        int rI = DCPU.MEM_SIZE;
        for(int i = 0; i < DCPU.REG_SIZE; i++){
            registers[i] = (ushort)((data[(i * 2) + rI] << 4) | data[((i * 2) + 1) + rI]);
        }
        int spReg = DCPU.MEM_SIZE + DCPU.REG_SIZE;
        PC = (ushort)((data[((spReg + 0) * 2)] << 4) | data[(((spReg + 0) * 2) + 1)]);

        EX = (ushort)((data[((spReg + 1) * 2)] << 4) | data[(((spReg + 1) * 2) + 1)]);

        SP = (ushort)((data[((spReg + 2) * 2)] << 4) | data[(((spReg + 2) * 2) + 1)]);

        IA = (ushort)((data[((spReg + 3) * 2)] << 4) | data[(((spReg + 3) * 2) + 1)]);

        int j = spReg + 4;
        ticks = (ushort)(data[(j * 2) + 0] << 56 | data[(j * 2) + 1] << 48 | 
                         data[(j * 2) + 2] << 40 | data[(j * 2) + 3] << 32 |
                         data[(j * 2) + 4] << 24 | data[(j * 2) + 5] << 16 |
                         data[(j * 2) + 6] << 08 | data[(j * 2) + 7]);

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

    //seralizes the dcpu memory into a byte array
    public byte[] serialize(){
        byte[] data = new byte[(DCPU.MEM_SIZE * 2) + (DCPU.REG_SIZE * 2) + 8 + 8];

        //encode the memory
        for(int i = 0; i < (DCPU.MEM_SIZE); i++){
            data[(i * 2) + 0] = (byte)((memory[i] >> 4) & 0xFF);
            data[(i * 2) + 1] = (byte)( memory[i]       & 0xFF);
        }
        int regIndex = (DCPU.MEM_SIZE);
        //encode the registers
        for(int i = 0; i < DCPU.REG_SIZE; i++){
            data[regIndex + (i * 2) + 0] = (byte)((registers[i] >> 4) & 0xFF);
            data[regIndex + (i * 2) + 1] = (byte)(registers[i]        & 0xFF);
        }
        int spIndex = regIndex + (DCPU.REG_SIZE);

        // encode PC
        data[(spIndex * 2) + 0] = (byte)((PC >> 4) & 0xFF); 
        data[(spIndex * 2) + 1] = (byte)(PC        & 0xFF);

        // encode EX
        data[((spIndex + 1) * 2) + 0] = (byte)((EX >> 4) & 0xFF); 
        data[((spIndex + 1) * 2) + 1] = (byte)(EX        & 0xFF);

        // encode SP
        data[((spIndex + 2) * 2) + 0] = (byte)((SP >> 4) & 0xFF);
        data[((spIndex + 2) * 2) + 0] = (byte)(SP        & 0xFF);

        // encode IA
        data[((spIndex + 3) * 2) + 0] = (byte)((IA >> 4) & 0xFF);
        data[((spIndex + 3) * 2) + 1] = (byte)(IA        & 0xFF);

        int tickIndex = spIndex + 4;

        //encode ticks
        data[(tickIndex * 2) + 0] = (byte)((ticks >> 56) & 0xFF);
        data[(tickIndex * 2) + 1] = (byte)((ticks >> 48) & 0xFF);
        data[(tickIndex * 2) + 2] = (byte)((ticks >> 40) & 0xFF);
        data[(tickIndex * 2) + 3] = (byte)((ticks >> 32) & 0xFF);
        data[(tickIndex * 2) + 4] = (byte)((ticks >> 24) & 0xFF);
        data[(tickIndex * 2) + 5] = (byte)((ticks >> 16) & 0xFF);
        data[(tickIndex * 2) + 6] = (byte)((ticks >> 8) & 0xFF);
        data[(tickIndex * 2) + 7] = (byte)(ticks        & 0xFF);

        return data;
    }
}
