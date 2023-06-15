using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PWFrameWork
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public class OffsetAttribute : Attribute
    {
        Int32 _value = 0;
        public Int32 Value
        {
            get
            {
                return _value;
            }
        }

        string _alias = null;
        public string Alias
        {
            get
            {
                return _alias;
            }
        }

        public OffsetAttribute(Int32 offsetValue)
        {
            _value = offsetValue;
        }

        public OffsetAttribute(string offsetAlias)
        {
            _alias = offsetAlias;
        }
    }

    public partial class MemPtr
    {
        static List<Type> _validTypes;
        static MemPtr()
        {
            _validTypes = new List<Type>
            {
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64),
                typeof(MemPtr),
                typeof(Byte),
                typeof(Single),
                typeof(Double),
            };
        }

        public T ReadStruct<T>() where T : new()
        {
            T result = new T();

            var offsets = new List<int>();
            var props = new List<PropertyInfo>();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
                if(_validTypes.Contains(prop.PropertyType))
                {
                    object[] attrs = prop.GetCustomAttributes(typeof(OffsetAttribute), true);
                    if (attrs.Length == 0)
                        continue;
                    OffsetAttribute off = (OffsetAttribute)attrs[0];
                    int offset = off.Value;
                    if (off.Alias != null)
                        offset = process.FindOffset(off.Alias);
                    props.Add(prop);
                    offsets.Add(offset);
                }

            if (props.Count == 0)
                return result;

            Type t = props[0].PropertyType;
            int minOffset = offsets[0], maxOffset = minOffset + (t.Equals(typeof(MemPtr)) ? 4 : Marshal.SizeOf(t));
            for (int i = 1; i < props.Count; ++i)
                if (minOffset > offsets[i])
                    minOffset = offsets[i];
                else
                {
                    int offset = offsets[i] + (t.Equals(typeof(MemPtr)) ? 4 : Marshal.SizeOf(t));
                    if(maxOffset < offset)
                        maxOffset = offset;
                }

            byte[] data = this[minOffset].ToBytes(maxOffset-minOffset);

            for(int i=0; i<props.Count; ++i)
            {
                t = props[i].PropertyType;
                object value = null;
                int offset = offsets[i] - minOffset;
                if(t.Equals(typeof(Int16)))
                    value = BitConverter.ToInt16(data, offset);
                else if(t.Equals(typeof(Int32)))
                    value = BitConverter.ToInt32(data, offset);
                else if(t.Equals(typeof(Int64)))
                    value = BitConverter.ToInt64(data, offset);
                else if(t.Equals(typeof(UInt16)))
                    value = BitConverter.ToUInt16(data, offset);
                else if(t.Equals(typeof(UInt32)))
                    value = BitConverter.ToUInt32(data, offset);
                else if(t.Equals(typeof(UInt64)))
                    value = BitConverter.ToUInt64(data, offset);
                else if(t.Equals(typeof(MemPtr)))
                    value = process[BitConverter.ToInt32(data, offset)];
                else if(t.Equals(typeof(Single)))
                    value = BitConverter.ToSingle(data, offset);
                else if(t.Equals(typeof(Double)))
                    value = BitConverter.ToDouble(data, offset);

                if (value != null)
                    props[i].SetValue(result, value, new object[0]);
            }

            return result;
        }
    }
}
