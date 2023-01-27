using AODB.Common.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AODB.Common
{
    public static class BinaryWriterExtensions
    {
        public static void WriteNullTerminatedString(this BinaryWriter writer, string value)
        {
            writer.Write(Encoding.UTF8.GetBytes(value));
            writer.Write((byte)0);
        }

        public static void WritePrefixedUTF8String(this BinaryWriter writer, string value)
        {
            byte[] strbytes = Encoding.UTF8.GetBytes(value);

            writer.Write(strbytes.Length);
            writer.Write(strbytes);
        }

        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public static void Write(this BinaryWriter writer, Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, Matrix matrix)
        {
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    writer.Write(matrix.values[x, y]);
                }
            }
        }

        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
        }
    }
}
