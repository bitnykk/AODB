using AODB.Common.DbClasses;
using System;
using System.IO;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1010006)]
    public class GroundTexture : Image
    {
        public override void Deserialize(BinaryReader reader)
        {
            reader.ReadBytes(24);
            JpgData = reader.ReadBytes((int)reader.BaseStream.Length - 24);
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
