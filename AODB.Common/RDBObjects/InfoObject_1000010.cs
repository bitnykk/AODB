using AODB.Common.DbClasses;
using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1000010)]
    public class InfoObject : RDBObject
    {
        private Dictionary<ResourceTypeId, Dictionary<int, string>> _types = null;
        public Dictionary<ResourceTypeId, Dictionary<int, string>> Types { get { return _types; } }

        public override void Deserialize(BinaryReader reader)
        {
            using (reader)
            {
                int typeCount = reader.ReadInt32();

                _types = new Dictionary<ResourceTypeId, Dictionary<int, string>>();
                for (var i = 0; i < typeCount; i++)
                {
                    var typeID = reader.ReadInt32();
                    var instances = new Dictionary<int, string>();
                    var instanceCount = reader.ReadInt32();
                    for (var j = 0; j < instanceCount; j++)
                    {
                        var instanceID = reader.ReadInt32();
                        var nameLength = reader.ReadInt32();
                        var name = reader.ReadChars(nameLength);
                        instances.Add(instanceID, new string(name));
                    }
                    _types.Add((ResourceTypeId)typeID, instances);
                }
            }

        }

        public override byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(_types.Count);

                    foreach (var nameList in _types)
                    {
                        writer.Write((int)nameList.Key); //var typeID = reader.ReadInt32();
                        writer.Write(nameList.Value.Count); //var instanceCount = reader.ReadInt32();
                        foreach (var namePair in nameList.Value)
                        {
                            writer.Write(namePair.Key); //var instanceID = reader.ReadInt32();
                            writer.Write(namePair.Value.Length); //var nameLength = reader.ReadInt32();
                            writer.Write(Encoding.GetEncoding(1252).GetBytes(namePair.Value)); //var name = reader.ReadChars(nameLength);
                        }
                    }

                    return stream.ToArray();
                }
            }
        }
    }
}
