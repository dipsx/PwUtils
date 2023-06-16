using PWFrameWork;
using System;
using System.Threading.Tasks;

namespace PW_PacketListener;
internal class cPacketInjection : IDisposable {

    private int _sendPacketOpcodeAddress;
    private int _packetAddressLocation { get => _packetAddressLocation == 0 ? 0 : _packetAddressLocation + 16; }
    private int _packetSizeAddress { get => _packetAddressLocation == 0 ? 0 : _packetAddressLocation + 21; }


    public async Task SendPacket(byte[] packetData) {
        var processHandle = MemoryManager.OpenProcessHandle;

        var packetAdrPtr = InjectHelper.AllocateMemory(processHandle, packetData.Length);
        MemoryManager.WriteBytes(packetAdrPtr, packetData);

        if (_sendPacketOpcodeAddress == 0)
            LoadSendPacketOpcode(processHandle);

        MemoryManager.WriteBytes(_packetAddressLocation, BitConverter.GetBytes(packetAdrPtr));
        MemoryManager.WriteBytes(_packetSizeAddress, BitConverter.GetBytes(packetData.Length));

        var threadPtr = InjectHelper.CreateRemoteThread(processHandle, _sendPacketOpcodeAddress);
        await Task.Run(() => WinApi.WaitForSingleObject(threadPtr, 5000));

        WinApi.CloseHandle(threadPtr);
        InjectHelper.FreeMemory(processHandle, packetAdrPtr);
    }

    private void LoadSendPacketOpcode(IntPtr processHandle) {
        var asm = new ASM();
        asm.Pushad();
        asm.Mov_EAX(cOptions.PacketSendFunction);
        asm.Mov_ECX_DWORD_Ptr(cOptions.BaseAddress);
        asm.Mov_ECX_DWORD_Ptr_ECX_Add(0x20);
        asm.Mov_EDI(0x0);
        asm.Push68(0x0);
        asm.Push_EDI();
        asm.Call_EAX();
        asm.Popad();
        asm.Ret();

        _sendPacketOpcodeAddress = asm.WriteAsmByHwnd((int)processHandle, 0);
    }

    public void Dispose() {
        if (_sendPacketOpcodeAddress != 0)
            InjectHelper.FreeMemory(MemoryManager.OpenProcessHandle, _sendPacketOpcodeAddress);
    }
}