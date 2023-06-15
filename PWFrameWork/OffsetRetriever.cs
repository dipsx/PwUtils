using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace PWFrameWork
{
    struct OffsetPattern
    {
        public string name;
        public byte[] prefix, suffix;
    }

    public class OffsetRetriever
    {
        List<OffsetPattern> patterns = new List<OffsetPattern>();
        List<string> names = new List<string>();
        public OffsetRetriever()
        {
        }

        static byte[] FromHexString(string str)
        {
            if (str.Length % 2 != 0)
                throw new ArgumentException("Not a hex string");
            str = str.ToUpper();
            byte[] result = new byte[str.Length / 2];
            for (int i = 0; i < result.Length; ++i)
                result[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            return result;
        }

        public OffsetRetriever Add(string name, string pattern)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException();
            if (names.Contains(name))
                throw new ArgumentException("Duplicate name");
            string[] parts = pattern.Split('?');
            if (parts.Length != 2 || parts[0].Length == 0 && parts[1].Length == 0)
                throw new ArgumentException("Pattern format:  \"HexPrefix?HexSuffix\"");

            OffsetPattern p = new OffsetPattern();
            p.name = name;
            p.prefix = FromHexString(parts[0]);
            p.suffix = FromHexString(parts[1]);
            patterns.Add(p);
            names.Add(name);
            return this;
        }

        public OffsetRetriever Add(string patterns)
        {
            foreach (var pat in patterns.Split(','))
            {
                string[] v = pat.Split(':');
                if (v.Length != 2)
                    throw new ArgumentException("Pattern syntax: name:pattern");
                Add(v[0].Trim(), v[1].Trim());
            }

            return this;
        }

        public Dictionary<string, int> FindOffsets(Process process)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            string fileName = process.MainModule.FileName;
            byte[] buffer;
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
            }
            int pos = 0;
            foreach (OffsetPattern p in patterns)
            {
                if (p.prefix.Length > 0)
                {
                    while (pos < buffer.Length)
                    {
                        pos = Array.IndexOf(buffer, p.prefix[0], pos);
                        if (pos == -1 || pos + p.prefix.Length + 4 + p.suffix.Length >= buffer.Length)
                        {
                            pos = buffer.Length;
                            break;
                        }
                        bool f = true;
                        for (int i = 1; i < p.prefix.Length; ++i)
                            if (buffer[pos + i] != p.prefix[i])
                            {
                                f = false;
                                break;
                            }
                        if (f)
                            for (int i = 0; i < p.suffix.Length; ++i)
                                if (buffer[pos + p.prefix.Length + 4 + i] != p.suffix[i])
                                {
                                    f = false;
                                    break;
                                }

                        if (f)
                        {
                            result.Add(p.name, BitConverter.ToInt32(buffer, pos + p.prefix.Length));
                            pos += p.prefix.Length + 4 + p.suffix.Length;
                            break;
                        }
                        ++pos;
                    }
                }
                else
                {
                    pos += 4;
                    while (pos < buffer.Length)
                    {
                        pos = Array.IndexOf(buffer, p.suffix[0], pos);
                        if (pos == -1 || pos + p.suffix.Length >= buffer.Length)
                        {
                            pos = buffer.Length;
                            break;
                        }
                        bool f = true;
                        for (int i = 1; i < p.suffix.Length; ++i)
                            if (buffer[pos + i] != p.suffix[i])
                            {
                                f = false;
                                break;
                            }
                        if (f)
                        {
                            result.Add(p.name, BitConverter.ToInt32(buffer, pos - 4));
                            pos += p.suffix.Length;
                            break;
                        }
                        ++pos;
                    }
                }
            }
            return result;
        }
    }
}
