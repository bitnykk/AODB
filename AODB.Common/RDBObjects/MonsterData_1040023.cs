using AODB.Common.DbClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1040023)]
    public class MonsterData : RDBObject
    {
        public Dictionary<int, uint> Stats = new Dictionary<int, uint>();
        public Dictionary<int, List<int>> Anims = new Dictionary<int, List<int>>();

        public override void Deserialize(BinaryReader reader)
        {
            ReadKeyValues(reader);
        }

        public void ReadKeyValues(BinaryReader reader)
        {
            while(true)
            {
                int key = reader.ReadInt32();
                reader.ReadInt32();

                switch (key)
                {
                    case 14:
                        ReadAnims(reader);
                        break;
                    case 15:
                        ReadStats(reader); 
                        break;
                    case 41:
                        break;
                    case 20:
                        return; // Don't parse the rest
                    default:
                        Console.WriteLine($"Unhandled key: {key}");
                        return;
                }
            }
        }

        public void ReadStats(BinaryReader reader)
        {
            int numStats = reader.Read3F1();

            for (int i = 0; i < numStats; i++)
                Stats.Add(reader.ReadInt32(), reader.ReadUInt32());
        }

        public void ReadAnims(BinaryReader reader)
        {
            int numActions = reader.Read3F1();

            for (int i = 0; i < numActions; i++)
            {
                List<int> anims = new List<int>();

                int actionId = reader.ReadInt32();
                int numAnims = reader.Read3F1();

                for (int j = 0; j < numAnims; j++)
                    anims.Add(reader.ReadInt32());

                Anims.Add(actionId, anims);
            }
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
