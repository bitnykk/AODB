using AODB.Common.DbClasses;
using System;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1000204)]
    public class SkillTrickle : RDBObject
    {
        public int Strength;
        public int Agility;
        public int Stamina;
        public int Intelligence;
        public int Sense;
        public int Psychic;

        public override void Deserialize(BinaryReader reader)
        {
            Strength = reader.ReadInt32();
            Agility = reader.ReadInt32();
            Stamina = reader.ReadInt32();
            Intelligence = reader.ReadInt32();
            Sense = reader.ReadInt32();
            Psychic = reader.ReadInt32();
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
