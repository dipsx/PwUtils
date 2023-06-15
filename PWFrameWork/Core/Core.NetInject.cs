using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace PWFrameWork
{
    public class NetInjection : IDisposable
    {
        static int EntryPoint(string arg)
        {
            try
            {
                string[] args = arg.Split('\n');

                Assembly asm = Assembly.LoadFrom(args[0]);
                Type t = asm.GetType(args[1]);
                MethodInfo m = t.GetMethod(args[2], BindingFlags.Static|BindingFlags.NonPublic);
                string[] a = new string[args.Length - 3];
                Array.Copy(args, 3, a, 0, a.Length);
                isInProcess = true;
                m.Invoke(null, new object[] { a });
            } catch (Exception ex) {
                Console.WriteLine(ex);
                throw;
                //return 1;
            }
            return 0;
        }
        static bool isInProcess = false;

        public static bool IsInProcess
        {
            get
            {
                return isInProcess;
            }
        }

        static object _lock = new object();
        static IntPtr loadLibraryAddr = IntPtr.Zero;
        static IntPtr getProcAddressAddr = IntPtr.Zero;

        ProcessMemory mem;
        MemPtr injectPtr;
        IntPtr workThread = IntPtr.Zero;
        Int32 runProcessAddr = 0;

        public NetInjection(ProcessMemory mem)
        {
            if (IsInProcess && mem.Process.Equals(Process.GetCurrentProcess()))
                throw new ApplicationException("Already injected");

            this.mem = mem;

            byte[] opcode = 
            {
0x60,                           //      PUSHAD                      ;   0
0xB9, 0x78,0x56,0x34,0x12,      //      MOV ECX,12345678            ;   1
0x51,                           //      PUSH ECX                    ;   6
0x51,                           //      PUSH ECX                    ;   7
0xBB, 0x78,0x56,0x34,0x12,      //      MOV EBX,12345678            ;   8
0xFF,0xD3,                      //      CALL EBX                    ;   13
0x68, 0x78,0x56,0x34,0x12,      //      PUSH 12345678               ;   15
0x50,                           //      PUSH EAX                    ;   20
0xBB, 0x78,0x56,0x34,0x12,      //      MOV EBX,12345678            ;   21
0xFF,0xD3,                      //      CALL EBX                    ;   26
0x59,                           //      POP ECX                     ;   28
0x89,0x01,                      //      MOV DWORD PTR DS:[ECX],EAX  ;   29
0x61,                           //      POPAD                       ;   31
0xC3                            //      RETN                        ;   32
            };

            const string unmanagedDll = "pwfw.dll";
            string currentDllPath = Assembly.GetExecutingAssembly().Location;
            string injectorDll = Path.Combine(Path.GetDirectoryName(currentDllPath), unmanagedDll);
            const string funcName = "RunProcess";

            if (!File.Exists(injectorDll))
                throw new FileNotFoundException("Unmanaged injector not found", injectorDll);

            byte[] dllBytes = Encoding.Unicode.GetBytes(injectorDll);
            byte[] funcBytes = Encoding.ASCII.GetBytes(funcName);

            lock (_lock)
            {
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    IntPtr hModule = WinApi.LoadLibraryW("kernel32.dll");
                    loadLibraryAddr = WinApi.GetProcAddress(hModule, "LoadLibraryW");
                    getProcAddressAddr = WinApi.GetProcAddress(hModule, "GetProcAddress");
                }
            }

            injectPtr = mem.Allocate(65536);
            Array.Copy(BitConverter.GetBytes(injectPtr.Address + opcode.Length), 0, opcode, 2, 4);
            Array.Copy(BitConverter.GetBytes((Int32)loadLibraryAddr), 0, opcode, 9, 4);
            Array.Copy(BitConverter.GetBytes(injectPtr.Address + opcode.Length + dllBytes.Length + 2), 0, opcode, 16, 4);
            Array.Copy(BitConverter.GetBytes((Int32)getProcAddressAddr), 0, opcode, 22, 4);

            List<byte> code = new List<byte>(opcode);
            code.AddRange(dllBytes);
            code.AddRange(new byte[] { 0, 0 });
            code.AddRange(funcBytes);
            code.AddRange(new byte[] { 0, 0 });

            injectPtr.Bytes = code.ToArray();

            IntPtr thread = injectPtr.CreateRemoteThread();

            WinApi.WaitForSingleObject(thread, 2000);
            WinApi.CloseHandle(thread);

            runProcessAddr = injectPtr[opcode.Length].Int32;
            Int32 errVal = BitConverter.ToInt32(dllBytes, 0);
            if (runProcessAddr == errVal)
                throw new ApplicationException("Unmanaged injection entry point not found");
        }

        public void Dispose()
        {
            if (injectPtr == null)
                return;
            if (workThread != IntPtr.Zero)
            {
                WinApi.CloseHandle(workThread);
                workThread = IntPtr.Zero;
            }
            injectPtr.Dispose();
            injectPtr = null;
        }

        public bool IsRunning
        {
            get
            {
                if (workThread == IntPtr.Zero)
                    return false;
                injectPtr.UpdateValue();
                return injectPtr.Int32 != 0;
            }
        }

        public void Run()
        {
            Assembly fwAsm = Assembly.GetExecutingAssembly();
            Assembly entryAsm = Assembly.GetEntryAssembly();
            MethodInfo entryPoint = entryAsm.EntryPoint;
            string[] args = Environment.GetCommandLineArgs();

            StringBuilder sb = new StringBuilder();
            sb.Append(fwAsm.Location);
            sb.Append('\n');
            sb.Append(entryAsm.Location);
            sb.Append('\n');
            sb.Append(entryPoint.DeclaringType.FullName);
            sb.Append('\n');
            sb.Append(entryPoint.Name);
            for (int i = 1; i < args.Length; ++i)
            {
                sb.Append('\n');
                sb.Append(args[i]);
            }
            injectPtr.String = sb.ToString();

            workThread = mem[runProcessAddr].CreateRemoteThread(injectPtr.Address);
        }

        public void Join(uint milliseconds)
        {
            if (!IsRunning)
                return;
            WinApi.WaitForSingleObject(workThread, milliseconds);
            workThread = IntPtr.Zero;
        }

        public static ProcessMemory GetProcessMemory()
        {
            return new InProcessMemory();
        }

        public static ProcessMemory GetProcessMemory(Dictionary<string, int> aliases)
        {
            return new InProcessMemory(aliases);
        }
    }
}
