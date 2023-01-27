using AODB.Common.DbClasses;
using System;
using System.IO;

namespace AODB.RDBObjects
{
    [RDBRecord(RecordTypeID = 1010008)]
    public class Image : RDBObject
    {
        public byte[] JpgData;

        public override void Deserialize(BinaryReader reader)
        {
            JpgData = reader.ReadBytes((int)reader.BaseStream.Length);
        }


        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
