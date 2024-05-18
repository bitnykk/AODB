using AODB.Common.DbClasses;
using AODB.Common.Structs;
using System;
using System.Collections.Generic;
using System.IO;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1000026)]
    public class PlayfieldDynels : RDBObject
    {
        public List<PlayfieldDynel> Dynels = new List<PlayfieldDynel>();

        public override void Deserialize(BinaryReader reader)
        {
            Dynels = new List<PlayfieldDynel>();

            int numDynels = reader.ReadInt32();
            for (int i = 0; i < numDynels; i++)
            {
                byte[] buffer = reader.ReadBytes(reader.ReadInt32());
                using (BinaryReader bufferReader = new BinaryReader(new MemoryStream(buffer)))
                {
                    Dynels.Add(ParseStatel(bufferReader));
                }
            }
        }

        private PlayfieldDynel ParseStatel(BinaryReader reader)
        {
            PlayfieldDynel dynel = new PlayfieldDynel();

            dynel.IdentityType = reader.ReadInt32();
            dynel.IdentityInstance = reader.ReadInt32();

            reader.ReadInt32();

            //Playfield Identity
            reader.ReadInt32();
            reader.ReadInt32();

            dynel.PlayfieldId = reader.ReadInt32();

            dynel.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            //Heading - W, Z, Y, X
            reader.ReadSingle();
            reader.ReadSingle();
            reader.ReadSingle();
            reader.ReadSingle();

            //Unk
            reader.ReadInt32();

            dynel.TemplateId = reader.ReadInt32();

            dynel.DescriptorData = reader.ReadBytes(reader.ReadInt32());

            return dynel;
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }

    public class PlayfieldDynel
    {
        public int IdentityType;
        public int IdentityInstance;
        public int PlayfieldId;
        public Vector3 Position;
        public int TemplateId;
        public byte[] DescriptorData;
    }
}
