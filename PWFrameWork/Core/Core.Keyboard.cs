using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PWFrameWork
{
    internal class Keyboard
    {
        object _lock = new object();
        Dictionary<ProcessMemory, KeyEventHandler> keyDownEvents = new Dictionary<ProcessMemory, KeyEventHandler>();
        Dictionary<ProcessMemory, KeyEventHandler> keyUpEvents = new Dictionary<ProcessMemory, KeyEventHandler>();
        IntPtr hHook = IntPtr.Zero;

        void Subscribe(ProcessMemory mem, Dictionary<ProcessMemory, KeyEventHandler> events, KeyEventHandler handler)
        {
            lock (_lock)
            {
                if (hHook == IntPtr.Zero)
                    InstallHook();
                if (events.ContainsKey(mem))
                    events[mem] += handler;
                else
                    events.Add(mem, (KeyEventHandler)handler.Clone());
            }
        }

        WinApi.KeyboardHookProc proc;
        private void InstallHook()
        {
            IntPtr hInstance = WinApi.LoadLibraryW("User32");
            proc = hookProc;
            hHook = WinApi.SetWindowsHookEx(WinApi.WH_KEYBOARD_LL, proc, hInstance, 0);
        }

        ~Keyboard()
        {
            lock (_lock)
            {
                keyDownEvents.Clear();
                keyUpEvents.Clear();
                UninstallHook();
            }
        }

        void Unsubscribe(ProcessMemory mem, Dictionary<ProcessMemory, KeyEventHandler> events, KeyEventHandler handler)
        {
            lock (_lock)
            {
                KeyEventHandler h = events[mem];
                h -= handler;
                if (h == null)
                    events.Remove(mem);
                else
                    events[mem] = h;
                if (keyDownEvents.Count == 0 && keyUpEvents.Count == 0)
                    UninstallHook();
            }
        }

        private void UninstallHook()
        {
            if (hHook != IntPtr.Zero)
            {
                WinApi.UnhookWindowsHookEx(hHook);
                hHook = IntPtr.Zero;
                proc = null;
            }
        }

        public void SubscribeOnKeyUp(ProcessMemory mem, KeyEventHandler handler)
        {
            Subscribe(mem, keyUpEvents, handler);
        }
        public void SubscribeOnKeyDown(ProcessMemory mem, KeyEventHandler handler)
        {
            Subscribe(mem, keyDownEvents, handler);
        }
        public void UnsubscribeOnKeyUp(ProcessMemory mem, KeyEventHandler handler)
        {
            Unsubscribe(mem, keyUpEvents, handler);
        }
        public void UnsubscribeOnKeyDown(ProcessMemory mem, KeyEventHandler handler)
        {
            Unsubscribe(mem, keyDownEvents, handler);
        }

        bool ctrlPressed = false, shiftPressed = false;
        int hookProc(int code, int wParam, ref WinApi.KeyboardHookStruct lParam)
        {
            lock (_lock)
            {
                if (code >= 0)
                {
                    IntPtr currentWindow = WinApi.GetForegroundWindow();
                    int pid;
                    WinApi.GetWindowThreadProcessId(currentWindow, out pid);
                    Dictionary<ProcessMemory, KeyEventHandler> events = null;
                    Keys key = (Keys)lParam.vkCode;

                    if (wParam == WinApi.WM_KEYDOWN || wParam == WinApi.WM_SYSKEYDOWN)
                    {
                        events = keyDownEvents;
                        if (key == Keys.LShiftKey || key == Keys.RShiftKey || key == Keys.ShiftKey)
                            shiftPressed = true;
                        if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.ControlKey)
                            ctrlPressed = true;
                    }
                    else if (wParam == WinApi.WM_KEYUP || wParam == WinApi.WM_SYSKEYUP)
                    {
                        events = keyUpEvents;
                        if (key == Keys.LShiftKey || key == Keys.RShiftKey || key == Keys.ShiftKey)
                            shiftPressed = false;
                        if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.ControlKey)
                            ctrlPressed = false;
                    }

                    if (events != null)
                    {
                        KeyEventHandler handler = null;
                        ProcessMemory mem = null;
                        foreach (var x in events)
                            if (x.Key.Process.Id == pid)
                            {
                                mem = x.Key;
                                handler = x.Value;
                                break;
                            }
                        if (handler != null)
                        {
                            if (shiftPressed)
                                key |= Keys.Shift;
                            if (ctrlPressed)
                                key |= Keys.Control;
                            if ((lParam.flags & 32) == 32)
                                key |= Keys.Alt;
                            KeyEventArgs kea = new KeyEventArgs(key);
                            try
                            {
                                handler(mem, kea);
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex);
                                throw;
                            }
                            if (kea.Handled)
                                return 1;
                        }
                    }
                }
                return WinApi.CallNextHookEx(hHook, code, wParam, ref lParam);
            }
        }

        static Keyboard _singleton = new Keyboard();
        private Keyboard() { }

        internal static Keyboard Singleton
        {
            get
            {
                return _singleton;
            }
        }

        internal void UnsubscribeProcess(ProcessMemory processMemory)
        {
            lock (_lock)
            {
                if (keyDownEvents.ContainsKey(processMemory))
                    keyDownEvents.Remove(processMemory);
                if (keyUpEvents.ContainsKey(processMemory))
                    keyUpEvents.Remove(processMemory);
            }
        }
    }
}
