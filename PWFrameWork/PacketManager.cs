using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PWFrameWork
{
    public class PacketManager
    {
        ProcessMemory process;
        TimeSpan timeout;

        // Код инжекта на отправку пакетов
        byte[] sendPacketOpcode = new byte[] 
        { 
            0x60,                                   //PUSHAD
            0xB8, 0x00, 0x00, 0x00, 0x00,           //MOV EAX, SendPacketAddress
            0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00,     //MOV ECX, DWORD PTR [realBaseAddress]
            0x8B, 0x49, 0x20,                       //MOV ECX, DWORD PTR [ECX+20]
            0xBF, 0x00, 0x00, 0x00, 0x00,           //MOV EDI, packetAddress
            0x6A, 0x00,                             //PUSH packetSize
            0x57,                                   //PUSH EDI
            0xFF, 0xD0,                             //CALL EAX
            0x61,                                   //POPAD
            0xC3                                    //RET
        };

        [UnmanagedFunctionPointer(CallingConvention.FastCall)]
        delegate void SendPacketFunction(IntPtr Base, Int32 packetSize, IntPtr packetAddress);

        SendPacketFunction localSendPacket = null;
        IntPtr BA = IntPtr.Zero;

        public PacketManager(ProcessMemory process, Int32 baseAddress, Int32 sendPacketFuncAddress, TimeSpan timeout)
        {
            this.process = process;
            if (process is InProcessMemory)
            {
                localSendPacket = (SendPacketFunction)Marshal.GetDelegateForFunctionPointer((IntPtr)sendPacketFuncAddress, typeof(SendPacketFunction));
                BA = (IntPtr)(process[baseAddress] + 0x20).Address;
            }
            else
            {
                var data = BitConverter.GetBytes(sendPacketFuncAddress);
                for (int i = 0; i < data.Length; ++i)
                    sendPacketOpcode[2 + i] = data[i];
                data = BitConverter.GetBytes(baseAddress);
                for (int i = 0; i < data.Length; ++i)
                    sendPacketOpcode[8 + i] = data[i];
                this.timeout = timeout;
            }
        }
        public PacketManager(ProcessMemory process, Int32 baseAddress, Int32 sendPacketFuncAddress)
            : this(process, baseAddress, sendPacketFuncAddress, TimeSpan.Zero)
        {
        }


        public void SendPacket(byte[] packet)
        {
            if (localSendPacket != null)
            {
                GCHandle h = GCHandle.Alloc(packet, GCHandleType.Pinned);
                try
                {
                    localSendPacket(BA, packet.Length, h.AddrOfPinnedObject());
                } 
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    h.Free();
                }
            }
            else
            {
                var inject = new List<byte>(sendPacketOpcode);
                inject.AddRange(packet);
                using (MemPtr injectPtr = process.Allocate(inject.Count))
                {
                    inject[21] = (byte)packet.Length;
                    var data = BitConverter.GetBytes(injectPtr.Address + sendPacketOpcode.Length);
                    for (int i = 0; i < data.Length; ++i)
                        inject[16 + i] = data[i];
                    injectPtr.Bytes = inject.ToArray();

                    IntPtr tid = injectPtr.CreateRemoteThread();
                    WinApi.WaitForSingleObject(tid, timeout == TimeSpan.Zero ? 0xFFFFFFFF : (uint)timeout.Milliseconds);
                    try
                    {
                        WinApi.CloseHandle(tid);
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }
}
