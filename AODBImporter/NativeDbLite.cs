using AODB.Common.DbClasses;
using AODB.Common.RDBObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AODBImporter
{
    internal class NativeDbLite : IDisposable
    {
        [DllImport("DatabaseController.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "??0BlockDatabase_t@@QAE@_N@Z")]
        public static extern IntPtr blockDbConstructor(IntPtr pThis, bool unk);

        [DllImport("DatabaseController.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Open@BlockDatabase_t@@QAEHPBD_N1@Z")]
        public static extern int blockDbOpen(IntPtr pThis, [MarshalAs(UnmanagedType.LPStr)] string path, bool a3, bool a4);

        [DllImport("DatabaseController.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Get@BlockDatabase_t@@QAEHIIAAIPAPAD@Z")]
        public static extern int blockDbGet(IntPtr pThis, int type, int instance, out int size, out IntPtr data);

        [DllImport("DatabaseController.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Put@BlockDatabase_t@@QAEHIIIPBD@Z")]
        public static extern int blockDbPut(IntPtr pThis, int type, int instance, int size, IntPtr data);

        [DllImport("DatabaseController.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Close@BlockDatabase_t@@QAEHXZ")]
        public static extern int blockDbClose(IntPtr pThis);

        private IntPtr pThis;
        private bool _disposedValue;


        public NativeDbLite()
        {
        }

        public void LoadDatabase(string path)
        {
            var buffer = new byte[0x4096];
            pThis = Marshal.AllocHGlobal(buffer.Length);
            blockDbConstructor(pThis, true);

            if (blockDbOpen(pThis, path, true, true) == -1)
                throw new Exception("Failed to open database.");
        }

        public RDBObject Get(int type, int instance)
        {
            var result = GetRaw(type, instance);
            if (result == null)
                return null;
            using (var reader = new BinaryReader(new MemoryStream(result), Encoding.GetEncoding(1252)))
            {
                var recordType = reader.ReadInt32();
                var recordInst = reader.ReadInt32();
                var version = reader.ReadInt32();

                var dbObj = RDBObject.GetRdbObjectForRecordType(type);

                dbObj.RecordType = recordType;
                dbObj.RecordId = recordInst;
                dbObj.RecordVersion = version;

                dbObj.Deserialize(reader);

                return dbObj;
            }
        }

        public byte[] GetRaw(int type, int record)
        {
            var result = blockDbGet(pThis, type, record, out var size, out var data);
            if (result == -1 || data == IntPtr.Zero)
                return null;

            byte[] managedArray = new byte[size];
            Marshal.Copy(data, managedArray, 0, (int)size);
            Marshal.FreeHGlobal((IntPtr)result);

            return managedArray;
        }

        public T Get<T>(int instance) where T : RDBObject, new()
        {
            var type = typeof(T).GetCustomAttribute<RDBRecordAttribute>().RecordTypeID;
            return (T)Get(type, instance);
        }

        public void Put(int type, int record, byte[] data)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            var result = blockDbPut(pThis, type, record, data.Length, unmanagedPointer);
            Marshal.FreeHGlobal(unmanagedPointer);

            if (result == -1)
                throw new Exception($"Db Put failed for record {type}:{record}");
        }

        public void PutRaw(int type, int instance, uint version, byte[] rawBytes)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(type);
                    writer.Write(instance);
                    writer.Write(version);
                    writer.Write(rawBytes);
                    Put(type, instance, stream.ToArray());
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                blockDbClose(pThis);
                Marshal.FreeHGlobal(pThis);
                _disposedValue = true;
            }
        }

        ~NativeDbLite()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
