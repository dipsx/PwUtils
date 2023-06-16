using PWFrameWork;
using System;
using System.Linq;
using System.Threading;

namespace PW_PacketListener {
    internal class cHookModule {
        private readonly byte[] _ListenFunction = new byte[] { 139, 68, 36, 4, 163, 0, 0, 0, 0, 139, 68, 36, 8, 163, 0, 0, 0, 0, 199, 5, 0, 0, 0, 0, 1, 0, 0, 0, 161, 0, 0, 0, 0, 131, 248, 1, 116, 246 };

        private byte[] _originalBytes;

        private int _secondWay_addressAfterPacketCall;

        private IntPtr _processHandle;

        private int _offset_MyFunc;

        private int _offset_data_addr;

        private int _offset_data_size;

        private int _offset_flag;

        public cHookModule() {
        }

        private void RestoreOriginalFunction() {

            MemoryManager.WriteBytes(_offset_MyFunc + 36, new byte[] { 0x90, 0x90 });

            if (_secondWay_addressAfterPacketCall != 0) {
                MemoryManager.WriteBytes(_secondWay_addressAfterPacketCall, _originalBytes);
                _secondWay_addressAfterPacketCall = 0;
            } else {
                MemoryManager.WriteBytes(cOptions.PacketSendFunction, _originalBytes);
            }

            Thread.Sleep(500);
            InjectHelper.FreeMemory(_processHandle, _offset_MyFunc);
            InjectHelper.FreeMemory(_processHandle, _offset_data_addr);
            InjectHelper.FreeMemory(_processHandle, _offset_data_size);
            InjectHelper.FreeMemory(_processHandle, _offset_flag);
        }


        public void StartHook() {
            var byteOnPacketFunction = MemoryManager.ReadByte(cOptions.PacketSendFunction);
            if (byteOnPacketFunction == 0xE9) {
                StartHook_SecondWay();
            } else {
                StartHook_FirstWay();
            }
        }

        public void StartHook_FirstWay() {
            this._processHandle = MemoryManager.OpenProcessHandle;
            this._originalBytes = new byte[cOptions.NumBytesToCopy];
            for (int i = 0; i < cOptions.NumBytesToCopy; i++) {
                this._originalBytes[i] = MemoryManager.ReadByte(cOptions.PacketSendFunction + i);
            }
            var numArray = _ListenFunction.ToArray();
            this._offset_MyFunc = InjectHelper.AllocateMemory(this._processHandle, (int)numArray.Length + cOptions.NumBytesToCopy + 5 + 2);
            this._offset_data_addr = InjectHelper.AllocateMemory(this._processHandle, 4);
            this._offset_data_size = InjectHelper.AllocateMemory(this._processHandle, 4);
            this._offset_flag = InjectHelper.AllocateMemory(this._processHandle, 4);
            MemoryManager.WriteBytes(this._offset_data_addr, new byte[4]);
            MemoryManager.WriteBytes(this._offset_data_size, new byte[4]);
            MemoryManager.WriteBytes(this._offset_flag, new byte[4]);

            BitConverter.GetBytes(_offset_data_addr).CopyTo(numArray, 5);
            BitConverter.GetBytes(_offset_data_size).CopyTo(numArray, 14);
            BitConverter.GetBytes(_offset_flag).CopyTo(numArray, 20);
            BitConverter.GetBytes(_offset_flag).CopyTo(numArray, 29);

            MemoryManager.WriteBytes(this._offset_MyFunc, numArray);
            MemoryManager.WriteBytes(this._offset_MyFunc + (int)numArray.Length, this._originalBytes);
            byte[] bytes2 = BitConverter.GetBytes(cOptions.PacketSendFunction + cOptions.NumBytesToCopy);
            byte[] numArray2 = new byte[] { 184, bytes2[0], bytes2[1], bytes2[2], bytes2[3], 255, 224 };
            MemoryManager.WriteBytes(this._offset_MyFunc + (int)numArray.Length + (int)this._originalBytes.Length, numArray2);
            byte[] bytes3 = BitConverter.GetBytes(this._offset_MyFunc);
            byte[] numArray3 = new byte[cOptions.NumBytesToCopy];
            numArray3[0] = 184;
            numArray3[1] = bytes3[0];
            numArray3[2] = bytes3[1];
            numArray3[3] = bytes3[2];
            numArray3[4] = bytes3[3];
            numArray3[5] = 255;
            numArray3[6] = 224;
            for (int j = 7; j < cOptions.NumBytesToCopy; j++) {
                numArray3[j] = 144;
            }

            MemoryManager.WriteBytes(cOptions.PacketSendFunction, numArray3);
        }


        public void StartHook_SecondWay() {
            // в клиенте на месте куда хотим вставить нащ хук содержится относительный адресс функции
            // поэтому мы достаем этот относительный адресс и прибавляем к нему адресс начала функции
            // получаем абсолютный адресс и вызываем уже его в нашем хуке

            _processHandle = MemoryManager.OpenProcessHandle;
            var listenFunctionBytes = _ListenFunction.ToArray();

            _offset_MyFunc = InjectHelper.AllocateMemory(_processHandle, listenFunctionBytes.Length + 100);
            _offset_data_addr = InjectHelper.AllocateMemory(_processHandle, 4);
            _offset_data_size = InjectHelper.AllocateMemory(_processHandle, 4);
            _offset_flag = InjectHelper.AllocateMemory(_processHandle, 4);

            MemoryManager.WriteBytes(_offset_data_addr, new byte[4]);
            MemoryManager.WriteBytes(_offset_data_size, new byte[4]);
            MemoryManager.WriteBytes(_offset_flag, new byte[4]);

            BitConverter.GetBytes(_offset_data_addr).CopyTo(listenFunctionBytes, 5);
            BitConverter.GetBytes(_offset_data_size).CopyTo(listenFunctionBytes, 14);
            BitConverter.GetBytes(_offset_flag).CopyTo(listenFunctionBytes, 20);
            BitConverter.GetBytes(_offset_flag).CopyTo(listenFunctionBytes, 29);

            MemoryManager.WriteBytes(this._offset_MyFunc, listenFunctionBytes);

            // получить относительный адресс функции которую будем вызывать
            var addrReletive = MemoryManager.ReadInt32(cOptions.PacketSendFunction + 1);
            var addrAbsolute = cOptions.PacketSendFunction + 5 + addrReletive;
            _secondWay_addressAfterPacketCall = addrAbsolute;
            _originalBytes = MemoryManager.ReadBytes(addrAbsolute, 9);

            var asm = new ASM();
            asm.AddBytes(_originalBytes);
            asm.Mov_EAX(addrAbsolute + _originalBytes.Length);
            asm.JMP_EAX();
            asm.WriteAsmByHwnd((int)_processHandle, _offset_MyFunc + listenFunctionBytes.Length);

            asm.Mov_EAX(_offset_MyFunc);
            asm.JMP_EAX();
            asm.Nop();
            asm.Nop();
            asm.WriteAsmByHwnd((int)_processHandle, addrAbsolute);
        }

        public void StopHook() {
            this.RestoreOriginalFunction();
        }

        public byte[] TimerTick() {
            int num;
            if (MemoryManager.ReadInt32(this._offset_flag) != 1) {
                return null;
            }
            int num1 = MemoryManager.ReadInt32(this._offset_data_size);
            int num2 = MemoryManager.ReadInt32(this._offset_data_addr);
            byte[] numArray = new byte[num1];
            WinApi.ReadProcessMemory(this._processHandle, num2, numArray, (int)numArray.Length, out num);
            MemoryManager.WriteInt32(this._offset_flag, 0);
            return numArray;
        }
    }
}