using AODB.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AODB
{
    internal class DbControllerNative
    {
        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "??0BlockDatabase_t@@QAE@_N@Z")]
        public static extern IntPtr blockDbConstructor(IntPtr pThis, bool unk);

        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "?Open@BlockDatabase_t@@QAEHPBD_N1@Z")]
        public static extern int blockDbOpen(IntPtr pThis, [MarshalAs(UnmanagedType.LPStr)] string path, bool a3, bool a4);

        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "?GetNumberOfKeys@BlockDatabase_t@@QAEIXZ")]
        public static extern int blockDbGetNumKeys(IntPtr pThis);

        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "?GetIsamRecord@BlockDatabase_t@@IAEHII@Z")]
        public static extern int blockDbGetIsamRecord(IntPtr pThis, int type, int instance);

        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "?Get@BlockDatabase_t@@QAEHIIAAIPAPAD@Z")]
        public static extern int blockDbGet(IntPtr pThis, uint type, uint instance, out uint size, out IntPtr data);

        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "?Put@BlockDatabase_t@@QAEHIIIPBD@Z")]
        public static extern int blockDbPut(IntPtr pThis, uint type, uint instance, uint size, IntPtr data);

        [DllImport("DatabaseController.dll",
            CallingConvention = CallingConvention.ThisCall,
            EntryPoint = "?Close@BlockDatabase_t@@QAEHXZ")]
        public static extern int blockDbClose(IntPtr pThis);

        private Dictionary<uint, Dictionary<uint, ulong>> _records;

        private IntPtr pThis;

        public DbControllerNative(string path)
        {
            LoadRecordTypes(path);
            var buffer = new byte[2048];
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            pThis = blockDbConstructor(unmanagedPointer, true);

            var open = blockDbOpen(pThis, path, true, true);
            if (open == -1)
            {
                throw new Exception("Opening db failed..");
            }
        }

        
        private void LoadRecordTypes(string idxPath)
        {
            _records = new Dictionary<uint, Dictionary<uint, ulong>>();
            using (var reader = new BinaryReader(new FileStream(idxPath, FileMode.Open)))
            {
                var blockOffset = reader.ReadInt32_At(12);
                var dataStart = reader.ReadInt32_At(72);

                reader.BaseStream.Position = dataStart;

                while (true)
                {
                    var nextBlock = reader.ReadInt32();
                    var prevBlock = reader.ReadInt32();

                    var recordCount = reader.ReadInt16();
                    var unk1 = reader.ReadInt32();
                    var unk2 = reader.ReadInt32();
                    var unk3 = reader.ReadInt32();
                    var unk4 = reader.ReadInt32();
                    var unk5 = reader.ReadInt16();

                    for (int i = 0; i < recordCount; i++)
                    {
                        ulong offset = reader.ReadUInt32();
                        ulong offset2 = reader.ReadUInt32();
                        offset = offset << 32;
                        offset |= offset2;

                        uint type = (uint)reader.ReadInt32Rev();
                        uint inst = (uint)reader.ReadInt32Rev();

                        if (!_records.ContainsKey(type))
                            _records.Add(type, new Dictionary<uint, ulong>());

                        _records[type].Add(inst, offset);
                    }

                    if (nextBlock == 0)
                    {
                        break;
                    }

                    reader.BaseStream.Position = nextBlock;
                }
            }
        }

        public byte[] Get(uint type, uint record)
        {
            var result = blockDbGet(pThis, type, record, out var size, out var data);
            if (result == -1 || data == (IntPtr)0)
            {
                return null;
            }
            byte[] managedArray = new byte[size];
            Marshal.Copy(data, managedArray, 0, (int)size);
            Marshal.FreeHGlobal((IntPtr)result);


            return managedArray;
        }

        public void Put(uint type, uint record, byte[] data)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            var result = blockDbPut(pThis, type, record, (uint)data.Length, unmanagedPointer);
            if (result == -1)
            {
                throw new Exception($"Putting into db failed.. {type}:{record}");
            }

            Marshal.FreeHGlobal(unmanagedPointer);
        }

        public int GetNumKeys()
        {
            return blockDbGetNumKeys(pThis);
        }

        public int GetIsamRecord(int type, int instance)
        {
            return blockDbGetIsamRecord(pThis, type, instance);
        }

        public Dictionary<uint, Dictionary<uint, ulong>> GetRecords()
        {
            return _records;
        }

        public void Close()
        {
            Console.WriteLine("Closing RDB");
            blockDbClose(pThis);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
