using PWFrameWork;
using System;
using System.Threading.Tasks;

namespace PW_PacketListener
{
	internal class cPacketInjection
	{
		private readonly byte[] _sendPacketOpcode = new byte[] { 96, 184, 0, 0, 0, 0, 139, 13, 0, 0, 0, 0, 139, 73, 32, 191, 0, 0, 0, 0, 104, 0, 0, 0, 0, 87, 255, 208, 97, 195 };

		private int _packetAddressLocation;

		private int _packetSizeAddress;

		private int _sendPacketOpcodeAddress;

		public cPacketInjection()
		{
		}

		private void LoadSendPacketOpcode(IntPtr processHandle)
		{

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

            this._sendPacketOpcodeAddress = asm.WriteAsmByHwnd((int)processHandle, 0);

			this._packetAddressLocation = this._sendPacketOpcodeAddress + 16;
			this._packetSizeAddress = this._sendPacketOpcodeAddress + 21;
		}

		public async Task SendPacket(byte[] packetData)
		{
			IntPtr openProcessHandle = MemoryManager.OpenProcessHandle;
			int packetAdrPtr = InjectHelper.AllocateMemory(openProcessHandle, packetData.Length);
			MemoryManager.WriteBytes(packetAdrPtr, packetData);

			if (this._sendPacketOpcodeAddress == 0)
				this.LoadSendPacketOpcode(openProcessHandle);

            MemoryManager.WriteBytes(this._packetAddressLocation, BitConverter.GetBytes(packetAdrPtr));
			MemoryManager.WriteBytes(this._packetSizeAddress, BitConverter.GetBytes(packetData.Length));
			IntPtr threadPtr = InjectHelper.CreateRemoteThread(openProcessHandle, this._sendPacketOpcodeAddress);
			await Task.Run(() => WinApi.WaitForSingleObject(threadPtr, 5000));
			WinApi.CloseHandle(threadPtr);
			InjectHelper.FreeMemory(openProcessHandle, packetAdrPtr, packetData.Length);
			//InjectHelper.FreeMemory(openProcessHandle, this._sendPacketOpcodeAddress, (int)this._sendPacketOpcode.Length);
		}
	}
}