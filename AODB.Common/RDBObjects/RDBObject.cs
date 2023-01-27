using AODB.Common;
using AODB.Common.DbClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AODB
{
    public abstract class RDBObject
    {
        public int RecordType { get; set; }

        public int RecordId { get; set; }
        public int RecordVersion { get; set; }

        private static Dictionary<uint, Type> _objectTypes = null;

        public static Dictionary<uint, Type> ObjectTypes
        {
            get
            {
                if (_objectTypes == null)
                {
                    var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<RDBRecordAttribute>() != null);

                    _objectTypes = new Dictionary<uint, Type>();

                    foreach (var type in types)
                    {
                        var recordTypeId = type.GetCustomAttribute<RDBRecordAttribute>().RecordTypeID;
                        if (!_objectTypes.ContainsKey(recordTypeId))
                            _objectTypes.Add(recordTypeId, type);
                    }
                }

                return _objectTypes;
            }
        }

        protected AODBSerializer _serializer;

        public RDBObject()
        {
            _serializer = new AODBSerializer();
        }

        public static RDBObject GetRdbObjectForRecordType(uint recordTypeId)
        {
            if (ObjectTypes.TryGetValue(recordTypeId, out var rdbObjectType))
            {
                return (RDBObject)Activator.CreateInstance(rdbObjectType);
            }

            return null;
        }

        public abstract void Deserialize(BinaryReader reader);

        public abstract byte[] Serialize();
    }
}
