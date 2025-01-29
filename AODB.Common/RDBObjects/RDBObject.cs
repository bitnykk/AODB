using AODB.Common;
using AODB.Common.DbClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AODB.Common.RDBObjects
{
    public enum ResourceTypeId
    {
        InfoObject = 1000010,
        RDBItem = 1000020,
        PlayfieldDynels = 1000026,
        SkillTrickle = 1000204,
        RdbMesh = 1010001,
        CatMesh = 1010002,
        Anim = 1010003,
        Texture = 1010004,
        GroundTexture = 1010006,
        Icon = 1010008,
        WallTexture = 1010009,
        SkinTexture = 1010011,
        MonsterData = 1040023
    }

    public abstract class RDBObject
    {
        public int RecordType { get; set; }

        public int RecordId { get; set; }
        public int RecordVersion { get; set; }

        private static Dictionary<int, Type> _objectTypes = null;

        public static Dictionary<int, Type> ObjectTypes
        {
            get
            {
                if (_objectTypes == null)
                {
                    var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<RDBRecordAttribute>() != null);

                    _objectTypes = new Dictionary<int, Type>();

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

        public static RDBObject GetRdbObjectForRecordType(int recordTypeId)
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
