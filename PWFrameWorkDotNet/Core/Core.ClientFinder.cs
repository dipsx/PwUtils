using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PWFrameWork
{
    public class ClientWindow
    {
        public String Name { get; set; }
        public IntPtr Handle { get; set; }
        public Int32 ProcessId { get; set; }

        public Process Process
        {
            get
            {
                return System.Diagnostics.Process.GetProcessById(ProcessId);
            }
        }

        public ClientWindow(string name, IntPtr handle, int id)
        {
            Name = name; Handle = handle; ProcessId = id;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ClientVerifiedArgs : EventArgs
    {
        private ProcessMemory mem;
        public ProcessMemory Process { get { return mem; } }
        public bool IsValid { get; set; }
        public string Name { get; set; }

        internal ClientVerifiedArgs(ProcessMemory mem, bool isValid, string name)
        {
            this.mem = mem;
            IsValid = isValid;
            Name = name;
        }
    }
    public delegate void ClientVerifier(ClientFinder sender, ClientVerifiedArgs args);

    public class ClientFinder
    {
        private Int32 BaseAddress { get; set; }
        private Int32 GameRun { get; set; }
        private Int32 HostPlayerStruct { get; set; }
        private Int32 HostPlayerName { get; set; }

        /// <summary>
        /// Инициализирует новый объект ClientFinder класса.
        /// </summary>
        /// <param name="baseAddress">Базовый адрес клиента PW</param>
        /// <param name="gameRun">Адрес структуры GameRun</param>
        /// <param name="hostPlayerStructOffset">Оффсет имени персонажа</param>
        public ClientFinder(Int32 baseAddress, Int32 gameRun, Int32 hostPlayerNameOffset, Int32 hostPlayerStruct = 0x34)
        {
            HostPlayerStruct = hostPlayerStruct;
            BaseAddress = baseAddress; GameRun = gameRun; HostPlayerName = hostPlayerNameOffset;
        }

        /// <summary>
        /// Инициализирует новый объект ClientFinder класса для поиска всех окон Perfect World
        /// </summary>
        public ClientFinder()
        {

            BaseAddress = 0; GameRun = 0; HostPlayerName = 0; HostPlayerStruct = 0;
        }

        public event ClientVerifier ClientVerifier;

        /// <summary>
        /// Создает объект ClientWindow для каждого клиента PW, у которого возможно определить имя 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClientWindow> GetWindowsNamedPrior()
        {
            IntPtr hwnd = IntPtr.Zero;
            while (true)
            {
                hwnd = WinApi.FindWindowEx(IntPtr.Zero, hwnd, "ElementClient Window", null);

                if (hwnd == IntPtr.Zero)
                    break;
                int pid;
                WinApi.GetWindowThreadProcessId(hwnd, out  pid);
                using (Process p = System.Diagnostics.Process.GetProcessById(pid))
                {
                    ProcessMemory mem = new ProcessMemory(p);
                    ClientVerifiedArgs args = new ClientVerifiedArgs(mem, false, null);

                    if (DefaultClientVerifier(mem))
                    {
                        args.IsValid = true;
                        args.Name = (mem[GameRun] + HostPlayerStruct + HostPlayerName + 0).String;
                    }

                    if (ClientVerifier != null)
                        ClientVerifier(this, args);
                    //Если получилось узнать имя - выводим имя
                    if (args.IsValid)
                    {
                        yield return new ClientWindow(String.IsNullOrEmpty(args.Name) ? WinApi.GetWindowText(hwnd) : args.Name,
                                               hwnd,
                                               pid);
                    }
                    else //иначе выводим все найденные окна без имени
                    {
                        yield return new ClientWindow(WinApi.GetWindowText(hwnd), hwnd, pid);
                    }


                }
            }
        }

        bool DefaultClientVerifier(ProcessMemory mem)
        {

            return HostPlayerStruct != 0 && mem[BaseAddress].Int32 + 0x1C == GameRun;

        }

        /// <summary>
        /// Создает объект ClientWindow для каждого клиента PW, который найден и выдает его как имя окна. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClientWindow> GetWindowsAll()
        {
            IntPtr hwnd = IntPtr.Zero;
            while (true)
            {
                hwnd = WinApi.FindWindowEx(IntPtr.Zero, hwnd, "ElementClient Window", null);

                if (hwnd == IntPtr.Zero)
                    break;
                int pid;
                WinApi.GetWindowThreadProcessId(hwnd, out  pid);
                using (Process p = System.Diagnostics.Process.GetProcessById(pid))
                {
                    yield return new ClientWindow(WinApi.GetWindowText(hwnd), hwnd, pid);

                }
            }
        }




    }
}
