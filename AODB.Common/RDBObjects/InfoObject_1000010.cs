using AODB.Common.DbClasses;
using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AODB.RDBObjects
{
    [RDBRecord(RecordTypeID = 1000010)]
    public class InfoObject : RDBObject
    {
        private Dictionary<int, Dictionary<int, string>> types = null;
        public Dictionary<int, Dictionary<int, string>> Types { get { return types; } }

        public override void Deserialize(BinaryReader reader)
        {
            using (reader)
            {
                int typeCount = reader.ReadInt32();

                types = new Dictionary<int, Dictionary<int, string>>();
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
                    types.Add(typeID, instances);
                }
            }

        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
