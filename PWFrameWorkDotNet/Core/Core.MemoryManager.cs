using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;
using System.Security.Cryptography;
using System.Diagnostics;

namespace PWFrameWork
{
    public static class MemoryManager
    {
        public static IntPtr OpenProcessHandle { get; set; }
        public static int ProcessId { get; set; }


        private const int PROCESS_ALL_ACCESS = 0x1f0fff;
        private const int PROCESS_CREATE_THREAD = 0x2;
        private const int PROCESS_VM_OPERATION = 0x8;
        private const int PROCESS_VM_WRITE = 0x20;

        /// <summary>
        /// Открывает объект процесса, возвращая дескриптор процесса. 
        /// Так же, дескриптор сохранится в свойстве класса: OpenProcessHandle.
        /// </summary>
        /// <param name="processId">PID процесса, который мы хотитм открыть.</param>
        /// <returns></returns>
        public static void OpenProcess(int processId)
        {
            ProcessId = processId;
            //OpenProcessHandle = WinApi.OpenProcess(WinApi.ProcessAccessFlags.All, false, processId);
            OpenProcessHandle = WinApi.OpenProcess((WinApi.ProcessAccessFlags)(PROCESS_ALL_ACCESS | PROCESS_CREATE_THREAD | PROCESS_VM_WRITE), false, processId);

            return;
        }

        /// <summary>
        /// Закрывает дескриптор открытого процесса.
        /// </summary>
        public static void CloseProcess()
        {
            if(OpenProcessHandle != IntPtr.Zero)
                WinApi.CloseHandle(OpenProcessHandle);
            OpenProcessHandle = IntPtr.Zero;
        }


        public static byte ReadByte(Int32 address)
        {
            int read; var buffer = new byte[1];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return buffer[0];
        }

        public static byte[] ReadBytes(int address, int count) {
            byte[] buffer = new byte[count];
            int bytesRead = 0;
            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, count, out bytesRead);
            return buffer;
        }

        /// <summary>
        /// Читает из памяти Int16 по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Int16 ReadInt16(Int32 address)
        {
            int read; var buffer = new byte[2];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return BitConverter.ToInt16(buffer, 0);
        }

        /// <summary>
        /// Читает из памяти Int32 по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Int32 ReadInt32(Int32 address)
        {
            int read; var buffer = new byte[4];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return BitConverter.ToInt32(buffer, 0);
        }

        public static uint ReadUInt32(Int32 address)
        {
            int read; var buffer = new byte[4];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return BitConverter.ToUInt32(buffer, 0);
        }

        /// <summary>
        /// Читает из памяти Int64 по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Int64 ReadInt64(Int32 address)
        {
            int read; var buffer = new byte[8];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Читает из памяти Float по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Single ReadFloat(Int32 address)
        {
            int read; var buffer = new byte[4];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// Читает из памяти Double по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Double ReadDouble(Int32 address)
        {
            int read; var buffer = new byte[8];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, buffer.Length, out read);

            return BitConverter.ToDouble(buffer, 0);
        }

        /// <summary>
        /// Читает из памяти String по указанному адресу с заданной длиной.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static String ReadString_Unicode(Int32 address, Int32 length)
        {
            int read; var buffer = new byte[length];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, length, out read);

            var enc = new UnicodeEncoding();
            var rtnStr = enc.GetString(buffer);

            return (rtnStr.IndexOf('\0') != -1) ? rtnStr.Substring(0, rtnStr.IndexOf('\0')) : rtnStr;
        }

        /// <summary>
        /// Читает из памяти String по указанному адресу с заданной длиной в кодировке ANSCII.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static String ReadString_ASCII(Int32 address, Int32 length)
        {
            int read; var buffer = new byte[length];

            WinApi.ReadProcessMemory(OpenProcessHandle, address, buffer, length, out read);

            var enc = new ASCIIEncoding();
            var rtnStr = enc.GetString(buffer);

            return (rtnStr.IndexOf('\0') != -1) ? rtnStr.Substring(0, rtnStr.IndexOf('\0')) : rtnStr;
        }


        /// <summary>
        /// Читает из памяти Int16, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static Int16 ChainReadInt16(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadInt16(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadInt16(tmpInt + offsets[offsets.Length - 1]);
        }

        /// <summary>
        /// Читает из памяти Int32, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static Int32 ChainReadInt32(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadInt32(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadInt32(tmpInt + offsets[offsets.Length - 1]);
        }

        /// <summary>
        /// Читает из памяти UInt32, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static UInt32 ChainReadUInt32(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadUInt32(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadUInt32(tmpInt + offsets[offsets.Length - 1]);
        }
        /// <summary>
        /// Читает из памяти Int64, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static Int64 ChainReadInt64(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadInt64(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadInt64(tmpInt + offsets[offsets.Length - 1]);
        }

        /// <summary>
        /// Читает из памяти Float, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static Single ChainReadFloat(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadFloat(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadFloat(tmpInt + offsets[offsets.Length - 1]);
        }

        /// <summary>
        /// Читает из памяти Double, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static Double ChainReadDouble(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadDouble(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadDouble(tmpInt + offsets[offsets.Length - 1]);
        }

        /// <summary>
        /// Читает из памяти String заданной длины, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static String ChainReadString_Unicode(Int32 address, Int32 length, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadString_Unicode(address, length);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadString_Unicode(tmpInt + offsets[offsets.Length - 1], length);
        }

        /// <summary>
        /// Читает из памяти String заданной длины, в кодировке ASCII, используя цепочку оффсетов .
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static String ChainReadString_ASCII(Int32 address, Int32 length, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadString_ASCII(address, length);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return ReadString_ASCII(tmpInt + offsets[offsets.Length - 1], length);
        }

        /// <summary>
        /// Получает конечный адрес, используя цепочку оффсетов.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        private static Int32 GetaddressByChaing(Int32 address, params Int32[] offsets)
        {
            if (offsets.Length == 0) return ReadInt32(address);

            var tmpInt = ReadInt32(address);

            for (var i = 0; i < offsets.Length - 1; i++)
            {
                tmpInt = ReadInt32(tmpInt + offsets[i]);
            }

            return tmpInt + offsets[offsets.Length - 1];
        }

        static public T Read<T>(Int32 address)
        {
            return ReadArray<T>(address, 1)[0];
        }

        static public T[] ReadArray<T>(Int32 address, int length)
        {
            T[] result = new T[length];
            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                int read = 0;
                WinApi.ReadProcessMemory2(OpenProcessHandle, address, ptr, length * Marshal.SizeOf(typeof(T)), out read);
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                handle.Free();
            }
            return result;
        }

        /// <summary>
        /// Записывает Byte по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteByte(Int32 address, byte value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, new[] { value }, 1, out tmpInt);
        }

        /// <summary>
        /// Записывает массив Byte по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteBytes(Int32 address, byte[] value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, value, value.Length, out tmpInt);
        }

        /// <summary>
        /// Записывает Int16 по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteInt16(Int32 address, Int16 value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, BitConverter.GetBytes(value), 2, out tmpInt);
        }

        /// <summary>
        /// Записывает Int32 по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteInt32(Int32 address, Int32 value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, BitConverter.GetBytes(value), 4, out tmpInt);
        }

        /// <summary>
        /// Записывает Int64 по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteInt64(Int32 address, Int64 value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, BitConverter.GetBytes(value), 8, out tmpInt);
        }

        /// <summary>
        /// Записывает Float по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteFloat(Int32 address, float value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, BitConverter.GetBytes(value), 4, out tmpInt);
        }

        /// <summary>
        /// Записывает Double по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteDouble(Int32 address, Double value)
        {
            int tmpInt;

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, BitConverter.GetBytes(value), 8, out tmpInt);
        }

        /// <summary>
        /// Записывает UnicodeString по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool WriteString(Int32 address, string str)
        {
            int tmpInt;
            var strBytes = new List<byte>();
            strBytes.AddRange(Encoding.Unicode.GetBytes(str));
            strBytes.AddRange(Encoding.Unicode.GetBytes("\0"));
            
            return WinApi.WriteProcessMemory(OpenProcessHandle, address, strBytes.ToArray(), strBytes.Count, out tmpInt);
        }

        /// <summary>
        /// Записывает ASCII String по указанному адресу.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool WriteString_ASCII(Int32 address, string str)
        {
            int tmpInt;
            var strBytes = new List<byte>();
            strBytes.AddRange(Encoding.ASCII.GetBytes(str));
            strBytes.AddRange(Encoding.ASCII.GetBytes("\0"));

            return WinApi.WriteProcessMemory(OpenProcessHandle, address, strBytes.ToArray(), strBytes.Count, out tmpInt);
        }

        /// <summary>
        /// Записывает Byte по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteByte(byte value, Int32 address, params Int32[] offsets)
        {
            return WriteByte(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает массив Byte по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteBytes(byte[] value, Int32 address, params Int32[] offsets)
        {
            return WriteBytes(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает Int16 по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteInt16(Int16 value, Int32 address, params Int32[] offsets)
        {
            return WriteInt16(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает Int32 по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteInt32(Int32 value, Int32 address, params Int32[] offsets)
        {
            return WriteInt32(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает Int64 по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteInt64(Int64 value, Int32 address, params Int32[] offsets)
        {
            return WriteInt64(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает Float по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteFloat(Single value, Int32 address, params Int32[] offsets)
        {
            return WriteFloat(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает Double по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteDouble(Double value, Int32 address, params Int32[] offsets)
        {
            return WriteDouble(GetaddressByChaing(address, offsets), value);
        }

        /// <summary>
        /// Записывает UnicodeString по адресу, полученному благодаря цепочке оффсетов..
        /// </summary>
        /// <param name="str"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public static bool ChainWriteString(string str, Int32 address, params Int32[] offsets)
        {
            return WriteString(GetaddressByChaing(address, offsets), str);
        }
    }
}
