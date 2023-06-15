using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace PWFrameWork
{
    internal delegate void DisposeHandle();

    public class ProcessMemory
    {
        Process process;
        protected Dictionary<string, int> aliases;
        public ProcessMemory(Process process)
        {
            this.process = process;
            aliases = new Dictionary<string, int>();

            process.Disposed += new EventHandler(process_Disposed);
        }

        void process_Disposed(object sender, EventArgs e)
        {
            process = null;
            Keyboard.Singleton.UnsubscribeProcess(this);
        }
        public ProcessMemory(Process process, Dictionary<string, int> aliases)
        {
            this.process = process;
            this.aliases = aliases;
            process.Disposed += new EventHandler(process_Disposed);
        }

        public Process Process
        {
            get
            {
                return process;
            }
        }

        internal int FindOffset(string alias)
        {
            int res;
            if (aliases.TryGetValue(alias, out res))
                return res;
            return -1;
        }

        internal virtual void ReadBytes(Int32 address, byte[] data)
        {
            int read;
            WinApi.ReadProcessMemory(process.Handle, address, data, data.Length, out read);
        }

        internal virtual void WriteBytes(Int32 address, byte[] data)
        {
            int written;
            WinApi.WriteProcessMemory(process.Handle, address, data, data.Length, out written);
        }

        internal virtual T[] Read<T>(Int32 address, int count) where T: struct
        {
            T[] result = new T[count];
            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                int read = 0;
                WinApi.ReadProcessMemory2(process.Handle, address, ptr, count * Marshal.SizeOf(typeof(T)), out read);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                throw;
            } finally
            {
                handle.Free();
            }
            return result;
        }

        internal virtual void Write<T>(Int32 address, T[] data) where T: struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                int written = 0;
                WinApi.WriteProcessMemory2(process.Handle, address, ptr, data.Length * Marshal.SizeOf(typeof(T)), out written);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                throw;
            } finally
            {
                handle.Free();
            }
        }

        public virtual MemPtr this[Int32 address]
        {
            get
            {
                return address == 0 ? null : new MemPtr(this, address);
            }
        }

        public MemPtr this[string alias]
        {
            get
            {
                int address = FindOffset(alias);
                if (address == -1)
                    return null;
                return this[address];
            }
        }

        public override bool Equals(object obj)
        {
            ProcessMemory pm = obj as ProcessMemory;
            if (pm == this)
                return true;
            if (process == pm.process)
                return process != null;
            return pm != null && process.Handle == pm.process.Handle;
        }

        public override int GetHashCode()
        {
            return process == null ? 0 : process.Handle.GetHashCode();
        }

        public virtual MemPtr Allocate(int memorySize, bool isExecutable = true)
        {
            int address = (int)WinApi.VirtualAllocEx(process.Handle, (IntPtr)0, memorySize, WinApi.AllocationType.Commit, isExecutable ? WinApi.MemoryProtection.ExecuteReadWrite : WinApi.MemoryProtection.ReadWrite);
            if (address == 0)
                return null;
            return new MemPtr(this, address, new DisposeHandle(
                    delegate()
                    {
                        WinApi.VirtualFreeEx(process.Handle, (IntPtr)address, memorySize, WinApi.FreeType.Release);
                    }
                ));
        }

        internal virtual IntPtr CreateRemoteThread(int address, int argument)
        {
            IntPtr tmpIntPtr;

            return WinApi.CreateRemoteThread(process.Handle, IntPtr.Zero, 0, (IntPtr)address, (IntPtr)argument, 0, out tmpIntPtr);
        }

        public event KeyEventHandler KeyUp
        {
            add
            {
                Keyboard.Singleton.SubscribeOnKeyUp(this, value);
            }
            remove
            {
                Keyboard.Singleton.UnsubscribeOnKeyUp(this, value);
            }
        }
        public event KeyEventHandler KeyDown
        {
            add
            {
                Keyboard.Singleton.SubscribeOnKeyDown(this, value);
            }
            remove
            {
                Keyboard.Singleton.UnsubscribeOnKeyDown(this, value);
            }
        }
    }

    internal class InProcessMemory : ProcessMemory
    {
        public InProcessMemory() : base(Process.GetCurrentProcess())
        {
        }

        public InProcessMemory(Dictionary<string, int> aliases) : base(Process.GetCurrentProcess(), aliases)
        {
        }

        internal override void ReadBytes(int address, byte[] data)
        {
            Marshal.Copy((IntPtr)address, data, 0, data.Length);
        }

        internal override void WriteBytes(int address, byte[] data)
        {
            Marshal.Copy(data, 0, (IntPtr)address, data.Length);
        }

        internal override T[] Read<T>(int address, int count)
        {
            T[] result = new T[count];
            int size = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < result.Length; ++i)
                Marshal.PtrToStructure((IntPtr)(address + i * size), result[i]);
            return result;
        }

        internal override void Write<T>(int address, T[] data)
        {
            base.Write<T>(address, data);
            int size = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < data.Length; ++i)
                Marshal.StructureToPtr(data[i], (IntPtr)(address + i * size), false);
        }

        public override MemPtr Allocate(int memorySize, bool isExecutable = true)
        {
            int address = (int)WinApi.VirtualAlloc((IntPtr)0, memorySize, WinApi.AllocationType.Commit, isExecutable ? WinApi.MemoryProtection.ExecuteReadWrite : WinApi.MemoryProtection.ReadWrite);
            if (address == 0)
                return null;
            return new MemPtr(this, address, new DisposeHandle(
                    delegate()
                    {
                        WinApi.VirtualFree((IntPtr)address, memorySize, WinApi.FreeType.Release);
                    }
                ));
        }

        internal override IntPtr CreateRemoteThread(int address, int argument)
        {
            IntPtr tmpIntPtr;

            return WinApi.CreateThread(IntPtr.Zero, 0, (IntPtr)address, (IntPtr)argument, 0, out tmpIntPtr);
        }

        public override bool Equals(object obj)
        {
            InProcessMemory pm = obj as InProcessMemory;
            return pm != null && aliases == pm.aliases;
        }

        public override int GetHashCode()
        {
            return Process.GetCurrentProcess().Handle.GetHashCode();
        }
    }

    public partial class MemPtr : IDisposable
    {
        ProcessMemory process;
        Int32 address;
        Int32 _value;
        bool hasValue = false;

        DisposeHandle disposeHandle = null;

        internal MemPtr(ProcessMemory process, Int32 address)
        {
            this.process = process;
            this.address = address;
        }

        internal MemPtr(ProcessMemory process, Int32 address, DisposeHandle disposeHandle)
        {
            this.process = process;
            this.address = address;
            this.disposeHandle = disposeHandle;
        }

        public Int32 Address
        {
            get
            {
                return address;
            }
        }

        public MemPtr Ptr
        {
            get
            {
                return process[Int32];
            }
        }

        public ProcessMemory Memory
        {
            get
            {
                return process;
            }
        }

        public void UpdateValue()
        {
            hasValue = false;
        }

        public Int32 Int32
        {
            get
            {
                if (hasValue)
                    return _value;
                var data = new byte[4];
                process.ReadBytes(address, data);
                _value = BitConverter.ToInt32(data, 0);
                hasValue = true;
                return _value;
            }
            set
            {
                hasValue = true;
                _value = value;
                process.WriteBytes(address, BitConverter.GetBytes(value));
            }
        }

        public UInt32 UInt32
        {
            get
            {
                return (UInt32)Int32;
            }
            set
            {
                Int32 = (Int32)value;
            }
        }

        public static MemPtr operator +(MemPtr ptr, Int32 offset)
        {
            if (ptr == null)
                throw new ArgumentNullException();
            return new MemPtr(ptr.process, ptr.Int32 == 0 ? 0 : ptr.Int32 + offset);
        }

        public static MemPtr operator +(MemPtr ptr, string alias)
        {
            if (ptr == null)
                throw new ArgumentNullException();
            int offset = ptr.process.FindOffset(alias);
            if (offset == -1)
                return null;
            return ptr + offset;
        }

        public static MemPtr operator +(MemPtr ptr, IEnumerable<Int32> offsets)
        {
            if (ptr == null || ptr.address == 0)
                throw new ArgumentNullException();
            Int32 val = ptr.Address;
            var data = new byte[4];
            foreach (Int32 offset in offsets)
                if (val == 0)
                    return null;
                else
                {
                    ptr.process.ReadBytes(val, data);
                    val = BitConverter.ToInt32(data, 0);
                    if (val == 0)
                        return null;
                    val += offset;
                }
            return new MemPtr(ptr.process, val);
        }

        public static Int32 operator -(MemPtr ptr1, MemPtr ptr2)
        {
            if (ptr1 == null || ptr2 == null)
                throw new ArgumentNullException();
            if (ptr1.process != ptr2.process)
                throw new ArgumentException();

            return ptr1.address - ptr2.address;
        }

        public MemPtr this[Int32 offset]
        {
            get
            {
                if (address == 0)
                    return null;
                return new MemPtr(process, address + offset);
            }
        }

        public MemPtr this[string alias]
        {
            get
            {
                int offset = process.FindOffset(alias);
                if (offset == -1)
                    return null;
                return this[offset];
            }
        }

        public MemPtr this[IEnumerable<Int32> offsets]
        {
            get
            {
                if (address == 0)
                    throw new ArgumentNullException();
                Int32 val = address;
                var data = new byte[4];
                foreach (Int32 offset in offsets)
                    if (val == 0)
                        return null;
                    else
                    {
                        process.ReadBytes(val+offset, data);
                        val = BitConverter.ToInt32(data, 0);
                    }
                return new MemPtr(process, val);
            }
        }

        public T Read<T>() where T: struct
        {
            return Read<T>(1)[0];
        }

        public T[] Read<T>(int count) where T: struct
        {
            return process.Read<T>(address, count);
        }

        public void Write<T>(T[] data) where T: struct
        {
            hasValue = false;
            process.Write(address, data);
        }

        public void Write<T>(T data) where T: struct
        {
            Write(new[] { data });
        }

        public string ToAString(int length)
        {
            var data = new byte[length + 1];
            process.ReadBytes(address, data);
            string str = Encoding.ASCII.GetString(data);
            int pos = str.IndexOf('\0');
            return pos != -1 ? str.Substring(0, pos) : str;
        }

        public string ToString(int length)
        {
            var data = new byte[length * 2 + 2];
            process.ReadBytes(address, data);
            string str = Encoding.Unicode.GetString(data);
            int pos = str.IndexOf('\0');
            return pos != -1 ? str.Substring(0, pos) : str;
        }

        public string String
        {
            get
            {
                return ToString(64);
            }
            set
            {
                hasValue = false;
                var strBytes = new List<byte>(Encoding.Unicode.GetBytes(value));
                strBytes.AddRange(new byte[] { 0, 0 });
                process.WriteBytes(address, strBytes.ToArray());
            }
        }

        public string AString
        {
            get
            {
                return ToAString(64);
            }
            set
            {
                hasValue = false;
                var strBytes = new List<byte>(Encoding.ASCII.GetBytes(value));
                strBytes.Add(0);
                process.WriteBytes(address, strBytes.ToArray());
            }
        }

        public byte[] Bytes
        {
            set
            {
                if (value.Length < 4)
                    hasValue = false;
                else
                {
                    hasValue = true;
                    _value = BitConverter.ToInt32(value, 0);
                }
                process.WriteBytes(address, value);
            }
        }

        public byte[] ToBytes(int count)
        {
            byte[] result = new byte[count];
            process.ReadBytes(address, result);
            return result;
        }

        public byte Byte
        {
            get
            {
                byte[] data = new byte[1];
                process.ReadBytes(address, data);
                return data[0];
            }
            set
            {
                hasValue = false;
                process.WriteBytes(address, new[] { value });
            }
        }

        public bool Boolean
        {
            get
            {
                return Byte != 0;
            }
            set
            {
                Byte = (byte)(value ? 1 : 0);
            }
        }

        public Int16 Int16
        {
            get
            {
                var data = new byte[2];
                process.ReadBytes(address, data);
                return BitConverter.ToInt16(data, 0);
            }
            set
            {
                hasValue = false;
                process.WriteBytes(address, BitConverter.GetBytes(value));
            }
        }

        public UInt16 UInt16
        {
            get
            {
                return (UInt16)Int16;
            }
            set
            {
                Int16 = (Int16)value;
            }
        }

        public Int64 Int64
        {
            get
            {
                var data = new byte[8];
                process.ReadBytes(address, data);
                hasValue = true;
                _value = BitConverter.ToInt32(data, 0);
                return BitConverter.ToInt64(data, 0);
            }
            set
            {
                hasValue = true;
                var data = BitConverter.GetBytes(value);
                _value = BitConverter.ToInt32(data, 0);
                process.WriteBytes(address, data);
            }
        }

        public UInt64 UInt64
        {
            get
            {
                return (UInt64)Int64;
            }
            set
            {
                Int64 = (Int64)value;
            }
        }

        public Single Single
        {
            get
            {
                var data = new byte[4];
                process.ReadBytes(address, data);
                hasValue = true;
                _value = BitConverter.ToInt32(data, 0);
                return BitConverter.ToSingle(data, 0);
            }
            set
            {
                var data = BitConverter.GetBytes(value);
                hasValue = true;
                _value = BitConverter.ToInt32(data, 0);
                process.WriteBytes(address, data);
            }
        }

        public Double Double
        {
            get
            {
                var data = new byte[8];
                process.ReadBytes(address, data);
                hasValue = true;
                _value = BitConverter.ToInt32(data, 0);
                return BitConverter.ToDouble(data, 0);
            }
            set
            {
                var data = BitConverter.GetBytes(value);
                hasValue = true;
                _value = BitConverter.ToInt32(data, 0);
                process.WriteBytes(address, data);
            }
        }

        public IEnumerable<MemPtr> ToPtrArray(Int32 arrayOffset, Int32 lengthOffset, Int32 elementSize, Int32 ptrOffset, bool skipNulls = true)
        {
            if (elementSize % 4 != 0)
                throw new ArgumentException();
            int el = elementSize / 4;

            int length = (this + lengthOffset).Int32;
            Int32[] addresses = (this + arrayOffset+ptrOffset).Read<Int32>(el * (length-1)+1);
            for (int i = 0; i < addresses.Length; i += el)
                if (!skipNulls || addresses[i] != 0)
                    yield return new MemPtr(process, addresses[i]);
        }

        public IEnumerable<MemPtr> ToPtrArray(Int32 arrayOffset, Int32 lengthOffset, bool skipNulls = true)
        {
            return ToPtrArray(arrayOffset, lengthOffset, 4, 0);
        }

        public IEnumerable<MemPtr> ToPtrArray(Int32 length, bool skipNulls = true)
        {
            Int32[] addresses = Read<Int32>(length);
            for (int i = 0; i < length; ++i)
                if (!skipNulls || addresses[i] != 0)
                    yield return new MemPtr(process, addresses[i]);
        }

        public HashTable ToHashTable(Int32 arrayOffset, Int32 countOffset, Int32 hashOffset, Int32 nextOffset, Int32 keyOffset, Int32 valueOffset)
        {
            return new HashTable(this, arrayOffset, countOffset, hashOffset, nextOffset, keyOffset, valueOffset);
        }

        public override bool Equals(object obj)
        {
            MemPtr ptr = obj as MemPtr;
            return ptr != null && process.Equals(ptr.process) && address == ptr.address;
        }

        public static bool operator true(MemPtr ptr)
        {
            return ptr != null && ptr.address != 0;
        }

        public static bool operator false(MemPtr ptr)
        {
            return ptr == null || ptr.address == 0;
        }

        public static bool operator !(MemPtr ptr)
        {
            return ptr == null || ptr.Address == 0;
        }

        public override int GetHashCode()
        {
            return process.GetHashCode() + address.GetHashCode();
        }

        public void Dispose()
        {
            if (disposeHandle != null)
                disposeHandle();
            disposeHandle = null;
            address = 0;
        }

        public IntPtr CreateRemoteThread(int arg = 0)
        {
            return process.CreateRemoteThread(address, arg);
        }
    }

    public class HashTable : IEnumerable<KeyValuePair<UInt32, MemPtr>>
    {
        MemPtr ptr, arrayPtr;
        Int32 arrayOffset;
        Int32 nextOffset;
        Int32 keyOffset;
        Int32 valueOffset;
        Int32 countOffset;

        Int32 itemOffset, itemSize;

        Int32 count = -1;
        UInt32 hash;

        internal HashTable(MemPtr ptr, Int32 arrayOffset, Int32 countOffset, Int32 hashOffset, Int32 nextOffset, Int32 keyOffset, Int32 valueOffset)
        {
            this.ptr = ptr;
            this.arrayOffset = arrayOffset;
            this.nextOffset = nextOffset;
            this.keyOffset = keyOffset;
            this.valueOffset = valueOffset;
            this.countOffset = countOffset;

            itemOffset = Math.Min(nextOffset, Math.Min(valueOffset, keyOffset));
            itemSize = Math.Max(nextOffset, Math.Max(valueOffset, keyOffset)) - itemOffset + 4;

            hash = (ptr + hashOffset).UInt32;
        }

        public int Count
        {
            get
            {
                if (hash == 0)
                    return 0;
                if (count == -1)
                    count = (ptr + countOffset).Int32;
                return count;
            }
        }

        public MemPtr this[UInt32 key]
        {
            get
            {
                if (hash == 0)
                    return null;
                if (arrayPtr == null)
                    arrayPtr = ptr + arrayOffset;
                for (MemPtr list = arrayPtr + (int)(key % hash) * 4; list; )
                {
                    byte[] data = list[itemOffset].ToBytes(itemSize);
                    if (BitConverter.ToUInt32(data, keyOffset-itemOffset) == key)
                        return list.Memory[BitConverter.ToInt32(data, valueOffset-itemOffset)];
                    list = list.Memory[BitConverter.ToInt32(data, nextOffset - itemOffset)];
                }

                return null;
            }
        }


        public IEnumerator<KeyValuePair<uint, MemPtr>> GetEnumerator()
        {
            if (hash == 0)
                yield break;
            if (arrayPtr == null)
                arrayPtr = ptr + arrayOffset + 0;
            foreach (MemPtr listHead in arrayPtr.ToPtrArray((int)hash))
                for (MemPtr list = listHead; list; )
                {
                    byte[] data = list[itemOffset].ToBytes(itemSize);
                    yield return new KeyValuePair<uint, MemPtr>(BitConverter.ToUInt32(data, keyOffset - itemOffset), list.Memory[BitConverter.ToInt32(data, valueOffset - itemOffset)]);
                    list = list.Memory[BitConverter.ToInt32(data, nextOffset - itemOffset)];
                }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator) this.GetEnumerator();
        }
    }
}
