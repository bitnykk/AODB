using AODB.Common.DbClasses;
using AODB.Common.Structs;
using System;
using System.Collections.Generic;
using System.IO;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 3030002)]
    public class Skeleton : RDBObject
    {
        public List<Bone> Bones;

        public override void Deserialize(BinaryReader reader)
        {
            int fileSize = reader.ReadInt32();
            int dbIdentityType = reader.ReadInt32();
            int dbIdentityInst = reader.ReadInt32();
            int Unk1 = reader.ReadInt32();
            int Unk2 = reader.ReadInt32();

            int numBones = reader.ReadInt32();

            Bones = new List<Bone>();
            for (int i = 0; i < numBones; i++)
            {
                Bone bone = new Bone();

                bone.Name = new string(reader.ReadChars(reader.ReadInt32()));
                bone.Rotation = reader.ReadQuaternion();
                bone.Position = reader.ReadVector3();

                int numChildJoints = reader.ReadInt32();
                int[] childJoints = new int[numChildJoints];

                for (int j = 0; j < numChildJoints; j++)
                    childJoints[j] = reader.ReadInt32();

                bone.Children = childJoints;

                Bones.Add(bone);
            }
        }


        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public class Bone
        {
            public string Name;
            public Quaternion Rotation;
            public Vector3 Position;
            public int[] Children;
        }
    }
}
