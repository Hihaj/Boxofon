using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Boxofon.Web.Helpers
{
    // Adapted from http://www.codeproject.com/Tips/76650/Base32-base32url-base64url-and-z-base-32-encoding
    public class ZBase32
    {
        public const char StandardPaddingChar = '=';
        public const string ZBase32Alphabet = "ybndrfg8ejkmcpqxot1uwisza345h769";

        public char PaddingChar;
        public bool UsePadding;
        public bool IsCaseSensitive;
        public bool IgnoreWhiteSpaceWhenDecoding;

        private readonly string _alphabet;
        private Dictionary<string, uint> _index;

        // alphabets may be used with varying case sensitivity, thus index must not ignore case
        private static readonly Dictionary<string, Dictionary<string, uint>> Indexes = new Dictionary<string, Dictionary<string, uint>>(2, StringComparer.InvariantCulture);

        public ZBase32()
        {
            PaddingChar = StandardPaddingChar;
            UsePadding = false;
            IsCaseSensitive = false;
            IgnoreWhiteSpaceWhenDecoding = false;
            _alphabet = ZBase32Alphabet;
        }

        public string Encode(byte[] data)
        {
            var result = new StringBuilder(Math.Max((int)Math.Ceiling(data.Length * 8 / 5.0), 1));
            var emptyBuff = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var buff = new byte[8];

            // take input five bytes at a time to chunk it up for encoding
            for (int i = 0; i < data.Length; i += 5)
            {
                int bytes = Math.Min(data.Length - i, 5);

                // parse five bytes at a time using an 8 byte ulong
                Array.Copy(emptyBuff, buff, emptyBuff.Length);
                Array.Copy(data, i, buff, buff.Length - (bytes + 1), bytes);
                Array.Reverse(buff);
                var val = BitConverter.ToUInt64(buff, 0);

                for (int bitOffset = ((bytes + 1) * 8) - 5; bitOffset > 3; bitOffset -= 5)
                {
                    result.Append(_alphabet[(int)((val >> bitOffset) & 0x1f)]);
                }
            }

            if (UsePadding)
            {
                result.Append(string.Empty.PadRight((result.Length % 8) == 0 ? 0 : (8 - (result.Length % 8)), PaddingChar));
            }
            return result.ToString();
        }

        public byte[] Decode(string input)
        {
            if (IgnoreWhiteSpaceWhenDecoding)
            {
                input = Regex.Replace(input, "\\s+", "");
            }

            if (UsePadding)
            {
                if (input.Length % 8 != 0)
                {
                    throw new ArgumentException("Invalid length for a base32 string with padding.");
                }
                input = input.TrimEnd(PaddingChar);
            }

            // index the alphabet for decoding only when needed
            EnsureAlphabetIndexed();

            var ms = new MemoryStream(Math.Max((int)Math.Ceiling(input.Length * 5 / 8.0), 1));

            // take input eight bytes at a time to chunk it up for encoding
            for (int i = 0; i < input.Length; i += 8)
            {
                int chars = Math.Min(input.Length - i, 8);
                ulong val = 0;
                int bytes = (int)Math.Floor(chars * (5 / 8.0));

                for (int charOffset = 0; charOffset < chars; charOffset++)
                {
                    uint cbyte;
                    if (!_index.TryGetValue(input.Substring(i + charOffset, 1), out cbyte))
                    {
                        throw new ArgumentException("Invalid character '" + input.Substring(i + charOffset, 1) + "' in base32 string, valid characters are: " + _alphabet);
                    }
                    val |= (((ulong)cbyte) << ((((bytes + 1) * 8) - (charOffset * 5)) - 5));
                }

                byte[] buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                ms.Write(buff, buff.Length - (bytes + 1), bytes);
            }
            return ms.ToArray();
        }

        private void EnsureAlphabetIndexed()
        {
            if (_index == null)
            {
                Dictionary<string, uint> cidx;
                string indexKey = (IsCaseSensitive ? "S" : "I") + _alphabet;
                if (!Indexes.TryGetValue(indexKey, out cidx))
                {
                    lock (Indexes)
                    {
                        if (!Indexes.TryGetValue(indexKey, out cidx))
                        {
                            cidx = new Dictionary<string, uint>(_alphabet.Length, IsCaseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
                            for (int i = 0; i < _alphabet.Length; i++)
                            {
                                cidx[_alphabet.Substring(i, 1)] = (uint)i;
                            }
                            Indexes.Add(indexKey, cidx);
                        }
                    }
                }
                _index = cidx;
            }
        }
    }
}