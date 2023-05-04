using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using AODB.Common.DbClasses;
using AODB.Common.Structs;

namespace AODB.Common
{

    public class AODBSerializer
    {
        public static class AOType
        {
            public static (int Id, int Size) Bool = (0, 1);
            public static (int Id, int Size) Short = (2, 2);
            public static (int Id, int Size) UInt32 = (3, 4);
            //public static (int Id, int Size) Message = 5;
            public static (int Id, int Size) String = (6, 0);
            public static (int Id, int Size) Byte = (9, 0);
            public static (int Id, int Size) Float = (10, 4);
            public static (int Id, int Size) Vector3 = (12, 12);
            public static (int Id, int Size) Quaternion = (13, 16);
            public static (int Id, int Size) Matrix = (15, 64);
            public static (int Id, int Size) Color = (16, 12);
            public static (int Id, int Size) Int32 = (17, 4);
        }

        private List<(string Unk, string ObjName)> _names = new List<(string Unk, string ObjName)>();

        public T Deserialize<T>(BinaryReader reader) where T : DbClass, new()
        {
            List<(string Unk, string ObjName)> names = new List<(string Unk, string ObjName)>();
            T dbClass = new T();

            int numNames = reader.ReadInt32();
            for (int i = 0; i < numNames; i++)
            {
                names.Add((reader.ReadNullTerminatedString(), reader.ReadNullTerminatedString()));
            }

            int numClasses = reader.ReadInt32() + 1;
            for (int i = 0; i < numClasses; i++)
            {
                object classInst = null;

                ClassDef classDef = new ClassDef
                {
                    ClassSize = reader.ReadInt32(),
                    Unknown1 = reader.ReadInt32(),
                    Members = new List<MemberDef>()
                };

                int numMembers = reader.ReadInt32();

                for (int j = 0; j < numMembers; j++)
                {
                    MemberDef memberDef = new MemberDef();
                    memberDef.NameIndex = reader.ReadByte();
                    memberDef.Name = memberDef.NameIndex == 255 ? "Unknown" : names[memberDef.NameIndex].ObjName;
                    memberDef.ElementType = reader.ReadInt32();
                    memberDef.ElementSize = reader.ReadInt32();
                    memberDef.TotalSize = reader.ReadInt32();

                    if (memberDef.Name == "__class_id__")
                    {
                        classDef.ClassId = reader.ReadInt32();
                        classDef.Name = names[classDef.ClassId].ObjName;

                        classInst = InstantiateClass(names[classDef.ClassId].ObjName, typeof(T));

                        if (classInst == null)
                            break;

                        Console.WriteLine($"Reading class {classDef.Name}");
                        continue;
                    }

                    Console.WriteLine($"Reading member {memberDef.Name} EleType: {memberDef.ElementType} - EleSize: {memberDef.ElementSize} - TotalSize:{memberDef.TotalSize}");

                    //memberDef.Data = reader.ReadBytes(memberDef.TotalSize);
                    if(classInst != null)
                        SetElementValue(reader, classInst, memberDef.Name, memberDef.TotalSize);
                    else
                        reader.ReadBytes(memberDef.TotalSize);

                    classDef.Members.Add(memberDef);
                }

                if (i > 0)
                    dbClass.Members.Add(classInst);
            }

            _names = names;

            return dbClass;
        }

        private object InstantiateClass(string className, Type dbClassType)
        {
            Type classType = dbClassType.GetNestedTypes().SingleOrDefault(t => t.Name == className);

            if (classType == null)
            {
                Console.WriteLine($"Unable to find {className} in {dbClassType.Name}");
                return null;
            }

            return Activator.CreateInstance(classType);
        }

        private void SetElementValue(BinaryReader reader, object classInst, string memberName, int memberTotalSize)
        {
            PropertyInfo propertyInfo = classInst.GetType().GetProperty(memberName);

            if (propertyInfo == null)
            {
                Console.WriteLine($"Unable to find {memberName} in {classInst.GetType().ToString()}");
                return;
            }

            object memberValue;

            if (propertyInfo.PropertyType == typeof(short))
            {
                memberValue = reader.ReadInt16();
            }
            else if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
            {
                memberValue = reader.ReadInt32();
            }
            else if (propertyInfo.PropertyType == typeof(uint) || propertyInfo.PropertyType == typeof(int?))
            {
                memberValue = reader.ReadUInt32();
            }
            else if (propertyInfo.PropertyType == typeof(bool))
            {
                memberValue = reader.ReadBoolean();
            }
            else if (propertyInfo.PropertyType == typeof(string))
            {
                var strnLength = reader.ReadInt32();
                var charArray = reader.ReadChars(strnLength);
                memberValue = new string(charArray);
            }
            else if (propertyInfo.PropertyType == typeof(string[]))
            {
                List<string> strings = new List<string>();

                var sizeLeft = memberTotalSize;

                while (sizeLeft > 0)
                {
                    var strnLength = reader.ReadInt32();
                    sizeLeft -= strnLength + 4; // + 4 is the strnLength
                    var charArray = reader.ReadChars(strnLength);
                    strings.Add(new string(charArray));
                }

                memberValue = strings.ToArray();
            }
            else if (propertyInfo.PropertyType == typeof(float))
            {
                memberValue = reader.ReadSingle();
            }
            else if (propertyInfo.PropertyType == typeof(Vector3))
            {
                memberValue = new Vector3() { X = reader.ReadSingle(), Y = reader.ReadSingle(), Z = reader.ReadSingle() };
            }
            else if (propertyInfo.PropertyType == typeof(Quaternion))
            {
                memberValue = new Quaternion() { X = reader.ReadSingle(), Y = reader.ReadSingle(), Z = reader.ReadSingle(), W = reader.ReadSingle() };
            }
            else if (propertyInfo.PropertyType == typeof(Matrix))
            {
                var matrix = new Matrix();
                matrix.values = new float[4, 4];
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        matrix.values[x, y] = reader.ReadSingle();
                    }
                }

                memberValue = matrix;
            }
            else if (propertyInfo.PropertyType == typeof(Color))
            {
                memberValue = new Color() { R = reader.ReadSingle(), G = reader.ReadSingle(), B = reader.ReadSingle() };
            }
            else if (propertyInfo.PropertyType == typeof(byte[]))
            {
                memberValue = reader.ReadBytes(memberTotalSize);
            }
            else if (propertyInfo.PropertyType == typeof(float[]))
            {
                var size = memberTotalSize / 4;
                var floatArray = new float[size];
                for (int i = 0; i < size; i++)
                {
                    floatArray[i] = reader.ReadSingle();
                }

                memberValue = floatArray;
            }
            else if (propertyInfo.PropertyType == typeof(uint[]))
            {
                var size = memberTotalSize / 4;
                var uintArray = new uint[size];
                for (int i = 0; i < size; i++)
                {
                    uintArray[i] = reader.ReadUInt32();
                }

                memberValue = uintArray;
            }
            else if (propertyInfo.PropertyType == typeof(int[]))
            {
                var size = memberTotalSize / 4;
                var intArray = new int[size];
                for (int i = 0; i < size; i++)
                {
                    intArray[i] = reader.ReadInt32();
                }

                memberValue = intArray;
            }
            else
            {
                Console.WriteLine("Unhandled type: " + propertyInfo.PropertyType);
                reader.ReadBytes(memberTotalSize);
                memberValue = null;
            }

            propertyInfo.SetValue(classInst, memberValue);
        }

        public byte[] Serialize(DbClass dbClassObj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    Serialize(writer, dbClassObj);
                    return stream.ToArray();
                }
            }
        }

        private void Serialize(BinaryWriter writer, DbClass dbClassObj)
        {
            
            string[] names = dbClassObj.GetNames().OrderBy(x => x).ToArray();

            writer.Write(names.Length);

            foreach(string name in names)
            {
                writer.WriteNullTerminatedString("0");
                writer.WriteNullTerminatedString(name);
            }

            /*
            writer.Write(_names.Count);

            foreach (var name in _names)
            {
                writer.WriteNullTerminatedString(name.Unk);
                writer.WriteNullTerminatedString(name.ObjName);
            }

            string[] names = _names.Select(x => x.ObjName).ToArray();
            */
            writer.Write(dbClassObj.Members.Count);

            //RootClass
            writer.Write(25);
            writer.Write(1);

            writer.Write(1);
            writer.Write((byte)Array.IndexOf(names, "obj"));
            writer.Write(AOType.Int32.Id);
            writer.Write(AOType.Int32.Size);
            writer.Write(4);
            writer.Write(0);


            //Nested Classes
            foreach (object classInst in dbClassObj.Members)
            {
                Console.WriteLine($"Writing class {classInst.GetType().Name}");
                long classSizeIdx = writer.BaseStream.Position;
                writer.Write(0);
                writer.Write(1);

                PropertyInfo[] properties = classInst.GetType().GetProperties().Where(x => !x.IsDefined(typeof(RDBDoNotSerializeAttribute))).ToArray();
                writer.Write(properties.Count(x => x.GetValue(classInst) != null) + 1);

                writer.Write((byte)Array.IndexOf(names, "__class_id__"));
                writer.Write(AOType.UInt32.Id);
                writer.Write(AOType.UInt32.Size);
                writer.Write(sizeof(int));
                writer.Write(Array.IndexOf(names, classInst.GetType().Name));

                foreach (PropertyInfo property in properties)
                {
                    if(property.GetValue(classInst) == null)
                    {
                        Console.WriteLine($"\tSkipping null element {property.Name}");
                        continue;
                    }

                    Console.WriteLine($"\tWriting element {property.Name}");

                    writer.Write((byte)Array.IndexOf(names, property.Name.ToLower()));
                    WriteProperty(writer, classInst, property);
                }

                long classEndIdx = writer.BaseStream.Position;
                writer.BaseStream.Position = classSizeIdx;
                writer.Write((int)(classEndIdx - classSizeIdx - 4));
                writer.BaseStream.Position = classEndIdx;
                Console.WriteLine($"ClassSize: {((int)(classEndIdx - classSizeIdx - 4)).ToString("X4")}");
            }
        }

        private void WriteProperty(BinaryWriter writer, object classInst, PropertyInfo property)
        {
            if (property.PropertyType == typeof(short))
            {
                writer.Write(AOType.Short.Id);
                writer.Write(AOType.Short.Size);
                writer.Write(sizeof(short));
                writer.Write((short)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                writer.Write(AOType.Int32.Id);
                writer.Write(AOType.Int32.Size);
                writer.Write(sizeof(int));
                writer.Write((int)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(uint) || property.PropertyType == typeof(uint?))
            {
                writer.Write(AOType.UInt32.Id);
                writer.Write(AOType.UInt32.Size);
                writer.Write(sizeof(uint));
                writer.Write((uint)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(bool))
            {
                writer.Write(AOType.Bool.Id);
                writer.Write(AOType.Bool.Size);
                writer.Write(sizeof(bool));
                writer.Write((bool)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(string))
            {
                writer.Write(AOType.String.Id);
                writer.Write(AOType.String.Size);

                string str = (string)property.GetValue(classInst);

                writer.Write(str.Length + sizeof(int));
                writer.WritePrefixedUTF8String(str);
            }
            else if (property.PropertyType == typeof(string[]))
            {
                writer.Write(AOType.String.Id);
                writer.Write(AOType.String.Size);

                string[] strs = (string[])property.GetValue(classInst);

                writer.Write(strs.Sum(x => x.Length + sizeof(int)));
                foreach (string str in strs)
                    writer.WritePrefixedUTF8String(str);
            }
            else if (property.PropertyType == typeof(float))
            {
                writer.Write(AOType.Float.Id);
                writer.Write(AOType.Float.Size);
                writer.Write(sizeof(float));
                writer.Write((float)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(Vector3))
            {
                writer.Write(AOType.Vector3.Id);
                writer.Write(AOType.Vector3.Size);
                writer.Write(AOType.Vector3.Size);
                writer.Write((Vector3)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(Quaternion))
            {
                writer.Write(AOType.Quaternion.Id);
                writer.Write(AOType.Quaternion.Size);
                writer.Write(AOType.Quaternion.Size);
                writer.Write((Quaternion)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(Matrix))
            {
                writer.Write(AOType.Matrix.Id);
                writer.Write(AOType.Matrix.Size);
                writer.Write(AOType.Matrix.Size);
                writer.Write((Matrix)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(Color))
            {
                writer.Write(AOType.Color.Id);
                writer.Write(AOType.Color.Size);
                writer.Write(AOType.Color.Size);
                writer.Write((Color)property.GetValue(classInst));
            }
            else if (property.PropertyType == typeof(byte[]))
            {
                writer.Write(AOType.Byte.Id);

                byte[] bytes = (byte[])property.GetValue(classInst);
                writer.Write((property.GetCustomAttribute<RealSize>() == null) ? AOType.Byte.Size : bytes.Length);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            else if (property.PropertyType == typeof(float[]))
            {
                writer.Write(AOType.Float.Id);
                writer.Write(AOType.Float.Size);

                float[] floats = (float[])property.GetValue(classInst);

                writer.Write(floats.Length * sizeof(float));
                foreach(float fl in floats)
                    writer.Write(fl);
            }
            else if (property.PropertyType == typeof(uint[]))
            {
                writer.Write(AOType.UInt32.Id);
                writer.Write(AOType.UInt32.Size);

                uint[] ints = (uint[])property.GetValue(classInst);

                writer.Write(ints.Length * sizeof(uint));
                foreach (uint i in ints)
                    writer.Write(i);
            }
            else if (property.PropertyType == typeof(int[]))
            {
                writer.Write(AOType.Int32.Id);
                writer.Write(AOType.Int32.Size);

                int[] ints = (int[])property.GetValue(classInst);

                writer.Write(ints.Length * sizeof(int));
                foreach (int i in ints)
                    writer.Write(i);
            }
            else
            {
                Console.WriteLine("Unhandled type: " + property.PropertyType);
            }
        }

        public class ClassDef
        {
            public string Name { get; internal set; }
            public int ClassId { get; internal set; }
            public int ClassSize { get; internal set; }
            public int Unknown1 { get; internal set; }
            public byte[] Data { get; internal set; }

            public List<MemberDef> Members { get; internal set; }
        }

        public class MemberDef
        {
            public string Name { get; internal set; }
            public byte NameIndex { get; internal set; }
            public int ElementType { get; internal set; }
            public int ElementSize { get; internal set; }
            public int TotalSize { get; internal set; }

            public byte[] Data { get; internal set; }
        }

        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class RealSize : Attribute
        {
        }
    }
}
