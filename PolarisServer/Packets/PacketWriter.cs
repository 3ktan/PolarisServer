﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class PacketWriter : BinaryWriter
    {
        public PacketWriter()
            : base(new MemoryStream())
        {
        }

        public PacketWriter(Stream s)
            : base(s)
        {
        }

        public void WriteMagic(uint magic, uint xor, uint sub)
        {
            var encoded = (magic + sub) ^ xor;
            Write(encoded);
        }

        public void WriteAscii(string str, uint xor, uint sub)
        {
            if (str.Length == 0)
            {
                WriteMagic(0, xor, sub);
            }
            else
            {
                // Magic, followed by string, followed by null terminator,
                // followed by padding characters if needed.
                var charCount = (uint) str.Length;
                var padding = 4 - (charCount & 3);

                WriteMagic(charCount + 1, xor, sub);
                Write(Encoding.ASCII.GetBytes(str));
                for (var i = 0; i < padding; i++)
                    Write((byte) 0);
            }
        }

        internal void WritePosition(PSOLocation location)
        {
            Write(Helper.FloatToHalfPrecision(location.A));
            Write(Helper.FloatToHalfPrecision(location.B));
            Write(Helper.FloatToHalfPrecision(location.C));
            Write(Helper.FloatToHalfPrecision(location.FacingAngle));
            Write(Helper.FloatToHalfPrecision(location.X));
            Write(Helper.FloatToHalfPrecision(location.Y));
            Write(Helper.FloatToHalfPrecision(location.Z));
        }

        public void WriteUtf16(string str, uint xor, uint sub)
        {
            if (str.Length == 0)
            {
                WriteMagic(0, xor, sub);
            }
            else
            {
                // Magic, followed by string, followed by null terminator,
                // followed by a padding character if needed.
                var charCount = (uint) str.Length + 1;
                var padding = (charCount & 1);

                WriteMagic(charCount, xor, sub);
                Write(Encoding.GetEncoding("UTF-16").GetBytes(str));
                Write((ushort) 0);
                if (padding != 0)
                    Write((ushort) 0);
            }
        }

        public void WriteFixedLengthASCII(string str, int charCount)
        {
            var writeAmount = Math.Min(str.Length, charCount);
            var paddingAmount = charCount - writeAmount;

            if (writeAmount > 0)
            {
                var chopped = writeAmount != str.Length ? str.Substring(0, writeAmount) : str;

                Write(Encoding.GetEncoding("ASCII").GetBytes(chopped));
            }

            if (paddingAmount > 0)
            {
                for (var i = 0; i < paddingAmount; i++)
                    Write((byte) 0);
            }
        }

        public void WriteFixedLengthUtf16(string str, int charCount)
        {
            var writeAmount = Math.Min(str.Length, charCount);
            var paddingAmount = charCount - writeAmount;

            if (writeAmount > 0)
            {
                var chopped = writeAmount != str.Length ? str.Substring(0, writeAmount) : str;

                Write(Encoding.GetEncoding("UTF-16").GetBytes(chopped));
            }

            if (paddingAmount > 0)
            {
                for (var i = 0; i < paddingAmount; i++)
                    Write((ushort) 0);
            }
        }

        public void Write(PSOLocation s)
        {
            Write(Helper.FloatToHalfPrecision(s.A));
            Write(Helper.FloatToHalfPrecision(s.B));
            Write(Helper.FloatToHalfPrecision(s.C));
            Write(Helper.FloatToHalfPrecision(s.FacingAngle));
            Write(Helper.FloatToHalfPrecision(s.X));
            Write(Helper.FloatToHalfPrecision(s.Y));
            Write(Helper.FloatToHalfPrecision(s.Z));
        }

        public void WritePlayerHeader(uint id)
        {
            Write(id);
            Write((uint) 0);
            Write((ushort) 4);
            Write((ushort) 0);
        }

        public unsafe void WriteStruct<T>(T structure) where T : struct
        {
            var strArr = new byte[Marshal.SizeOf(structure)];

            fixed (byte* ptr = strArr)
            {
                Marshal.StructureToPtr(structure, (IntPtr) ptr, false);
            }

            Write(strArr);
        }

        public byte[] ToArray()
        {
            var ms = (MemoryStream) BaseStream;
            return ms.ToArray();
        }
    }
}