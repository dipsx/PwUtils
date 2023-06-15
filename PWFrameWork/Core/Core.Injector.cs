using System;

namespace PWFrameWork
{
    public static class InjectHelper
    {
        /// <summary>
        /// Выделяем область памяти, указанного размера
        /// </summary>
        /// <param name="processHandle">Дескриптор процесса</param>
        /// <param name="memorySize">Размер области в байтах</param>
        /// <returns></returns>
        public static int AllocateMemory(IntPtr processHandle, int memorySize)
        {
            return (int)WinApi.VirtualAllocEx(processHandle, (IntPtr)0, memorySize, WinApi.AllocationType.Commit, WinApi.MemoryProtection.ReadWrite);
        }

        /// <summary>
        /// Освобождает область памяти указанного размера
        /// </summary>
        /// <param name="processHandle">Дескриптор процесса</param>
        /// <param name="address">Адрес</param>
        /// <param name="memorySize">Размер области в байтах</param>
        public static void FreeMemory(IntPtr processHandle, int address, int memorySize = 0)
        {
            WinApi.VirtualFreeEx(processHandle, (IntPtr)address, 0, WinApi.FreeType.Release);
        }

        /// <summary>
        /// Создает поток, выполняющий код по указанному адресу
        /// </summary>
        /// <param name="processHandle">Дескриптор процесса</param>
        /// <param name="address">Адрес</param>
        /// <returns></returns>
        public static IntPtr CreateRemoteThread(IntPtr processHandle, int address)
        {
            IntPtr tmpIntPtr;

            return WinApi.CreateRemoteThread(processHandle, IntPtr.Zero, 0, (IntPtr)address, IntPtr.Zero, 0, out tmpIntPtr);
        }
    }
}
