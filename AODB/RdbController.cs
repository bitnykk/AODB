using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AODB.Common.DbClasses;

namespace AODB
{
    public class RdbController : IDisposable
    {
        private DbController _dbController;

        public Dictionary<uint, Dictionary<uint, ulong>> RecordTypeToId => _dbController.GetRecords();

        private bool disposedValue;

        public RdbController(string path)
        {
            _dbController = new DbController(Path.Combine(path, "cd_image/data/db/ResourceDatabase.idx"));
        }

        public RDBObject Get(uint type, uint instance)
        {
            var result = _dbController.Get(type, instance);
            if (result == null)
                return null;
            using (var reader = new BinaryReader(new MemoryStream(result)))
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

        public T Get<T>(uint instance) where T : RDBObject, new()
        {
            var type = typeof(T).GetCustomAttribute<RDBRecordAttribute>().RecordTypeID;
            return (T)Get(type, instance);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _dbController.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public enum ResourceTypeId
    {
        RdbMesh = 1010001,
        Texture = 1010004,
    }
}
