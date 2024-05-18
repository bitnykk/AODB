using AODB.Common.DbClasses;
using AODB.Common.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Remoting.Messaging;
using static AODB.Common.RDBObjects.RDBCatMesh;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1010003)]
    public class CATAnim : RDBObject
    {
        public Anim Animation;

        public struct RotKeyframe
        {
            public float Time;
            public Quaternion Rot;
        }

        public struct PosKeyframe
        {
            public float Time;
            public Vector3 Pos;
        }

        public struct Anim
        {
            public Dictionary<int, List<RotKeyframe>> RotKeyFrames;
            public Dictionary<int, List<PosKeyframe>> PosKeyFrames;
        }

        public override void Deserialize(BinaryReader reader)
        {
            int[] blockDeltas = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            reader.ReadBytes(32);

            int animcount = reader.ReadInt32();

            for (int i = 0; i < animcount; i++)
            {
                reader.ReadInt32(); //Frame?
                reader.ReadBytes(32);
            }

            int version = reader.ReadInt32();
            uint vers_flg = reader.ReadUInt32();
            uint vers = (vers_flg & 0xFEFFFFFF);
            //bool flag = (vers_flg & 0x1000000) != 0;
            bool framesAreInt = (vers_flg & 0x1) != 0;
            float duration = (vers >= 262) ? reader.ReadSingle() : reader.ReadInt32();
            float v2 = reader.ReadSingle();
            float v3 = reader.ReadSingle();

            int jointCount = reader.ReadInt32();
            byte posBitCount = reader.ReadByte();
            byte rotBitCount = reader.ReadByte();
            List<byte[]> blobs = new List<byte[]>();
            for (int b = 0; b < 10; b++)
            {
                int pLength = reader.ReadInt32();
                int unpLength = reader.ReadInt32();
                byte[] buffer = reader.ReadBytes(pLength);
                blobs.Add(Decompress(buffer));
            }

            Dictionary<int, List<PosKeyframe>> posKeyframes = new Dictionary<int, List<PosKeyframe>>();
            Dictionary<int, List<RotKeyframe>> rotKeyframes = new Dictionary<int, List<RotKeyframe>>();

            using (BinaryReader reader0 = new BinaryReader(new MemoryStream(blobs[0])))
            using (BinaryReader reader1 = new BinaryReader(new MemoryStream(blobs[1])))
            using (BinaryReader reader2 = new BinaryReader(new MemoryStream(blobs[2])))
            using (BinaryReader reader3 = new BinaryReader(new MemoryStream(blobs[3])))
            using (BinaryReader reader4 = new BinaryReader(new MemoryStream(blobs[4])))
            using (BinaryReader reader5 = new BinaryReader(new MemoryStream(blobs[5])))
            using (BinaryReader reader6 = new BinaryReader(new MemoryStream(blobs[6])))
            using (BinaryReader reader7 = new BinaryReader(new MemoryStream(blobs[7])))
            using (BinaryReader reader8 = new BinaryReader(new MemoryStream(blobs[8])))
            using (BinaryReader reader9 = new BinaryReader(new MemoryStream(blobs[9])))
            {
                for (int i = 0; i < jointCount; i++)
                {
                    blockDeltas[0] = GetOne(reader0, 32, blockDeltas[0]);
                    int boneIdx = blockDeltas[0];
                    blockDeltas[0] = GetOne(reader0, 32, blockDeltas[0]);
                    int unknown1 = blockDeltas[0];
                    blockDeltas[0] = GetOne(reader0, 32, blockDeltas[0]);
                    int unknown2 = blockDeltas[0];
                    blockDeltas[0] = GetOne(reader0, 32, blockDeltas[0]);
                    int rotKeys = blockDeltas[0];
                    blockDeltas[0] = GetOne(reader0, 32, blockDeltas[0]);
                    int posKeys = blockDeltas[0];

                    blockDeltas[6] = GetOne(reader6, 32, blockDeltas[6]);
                    float headY = BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[6]), 0);
                    blockDeltas[6] = GetOne(reader6, 32, blockDeltas[6]);
                    float tailY = BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[6]), 0);
                    blockDeltas[6] = GetOne(reader6, 32, blockDeltas[6]);
                    float headZ = BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[6]), 0);
                    blockDeltas[6] = GetOne(reader6, 32, blockDeltas[6]);
                    float tailZ = BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[6]), 0);
                    blockDeltas[6] = GetOne(reader6, 32, blockDeltas[6]);
                    float headX = BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[6]), 0);
                    blockDeltas[6] = GetOne(reader6, 32, blockDeltas[6]);
                    float tailX = BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[6]), 0);

                    posKeyframes.Add(boneIdx, new List<PosKeyframe>());
                    rotKeyframes.Add(boneIdx, new List<RotKeyframe>());

                    int frameBitCount = (vers >= 262) ? 32 : 20;
                    int bitmask1 = rotBitCount == 32 ? -1 : (1 << rotBitCount) - 1;
                    int bitmask2 = posBitCount == 32 ? -1 : (1 << posBitCount) - 1;
                    //Console.WriteLine($"Mask: {bitmask1}");
                    for (int r = 0; r < rotKeys; r++)
                    {
                        blockDeltas[1] = GetOne(reader1, frameBitCount, blockDeltas[1]);
                        blockDeltas[2] = GetOne(reader2, rotBitCount, blockDeltas[2]);
                        blockDeltas[3] = GetOne(reader3, rotBitCount, blockDeltas[3]);
                        blockDeltas[4] = GetOne(reader4, rotBitCount, blockDeltas[4]);
                        blockDeltas[5] = GetOne(reader5, rotBitCount, blockDeltas[5]);

                        float rotW = (float)blockDeltas[2] / bitmask1;
                        float rotY = (float)blockDeltas[3] / bitmask1;
                        float rotZ = (float)blockDeltas[4] / bitmask1;
                        float rotX = (float)blockDeltas[5] / bitmask1;

                        float fframe = framesAreInt ? blockDeltas[1] : BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[1]), 0);
                        //if (fframe == 0f)
                        //{
                        //    Console.WriteLine($"bpy.data.objects[\"Cube.{boneIdx:000}\"].rotation_mode = 'QUATERNION'");
                        //    Console.WriteLine($"bpy.data.objects[\"Cube.{boneIdx:000}\"].delta_rotation_quaternion = ({rotW:0.000000}, {rotX:0.000000}, {rotY:0.000000}, {rotZ:0.000000})");
                        //}

                        rotKeyframes[boneIdx].Add(new RotKeyframe()
                        {
                            Time = fframe,
                            Rot = new Quaternion(rotX, rotY, rotZ, rotW)
                        });
                    }
                    //Console.WriteLine($"Mask: {bitmask2}");
                    for (int p = 0; p < posKeys; p++)
                    {
                        blockDeltas[1] = GetOne(reader1, frameBitCount, blockDeltas[1]);
                        blockDeltas[7] = GetOne(reader7, posBitCount, blockDeltas[7]);
                        blockDeltas[8] = GetOne(reader8, posBitCount, blockDeltas[8]);
                        blockDeltas[9] = GetOne(reader9, posBitCount, blockDeltas[9]);

                        float fframe = framesAreInt ? blockDeltas[1] : BitConverter.ToSingle(BitConverter.GetBytes(blockDeltas[1]), 0);

                        float posY = (float)blockDeltas[7] / bitmask2; posY = posY * (tailY - headY) + headY;
                        float posZ = (float)blockDeltas[8] / bitmask2; posZ = posZ * (tailZ - headZ) + headZ;
                        float posX = (float)blockDeltas[9] / bitmask2; posX = posX * (tailX - headX) + headX;
                        //if (fframe == 0f)
                        //    Console.WriteLine($"bpy.data.objects[\"Cube.{boneIdx:000}\"].delta_location = ({posX:0.000000}, {posY:0.000000}, {posZ:0.000000})");

                        posKeyframes[boneIdx].Add(new PosKeyframe()
                        {
                            Time = fframe,
                            Pos = new Vector3(posX, posY, posZ)
                        });
                    }
                }
            }

            Animation = new Anim()
            {
                PosKeyFrames = posKeyframes,
                RotKeyFrames = rotKeyframes
            };
        }

        public int GetOne(BinaryReader reader, int bitcount, int delta)
        {
            int bytecount = (bitcount + 7) >> 3;
            int bitmask = bitcount == 32 ? -1 : (1 << bitcount) - 1;
            int retval = 0;
            byte[] buf;
            int v2, v3, v4, v5, v6, v9;
            switch (bytecount)
            {
                case 1:
                    retval = reader.ReadByte();
                    break;
                case 2:
                    buf = reader.ReadBytes(2);
                    v6 = buf[0];
                    v4 = buf[1];
                    retval = (v4 | (v6 << 8));
                    break;
                case 3:
                    buf = reader.ReadBytes(3);
                    v9 = buf[0];
                    v5 = buf[1];
                    v4 = buf[2];
                    v3 = v9 << 8;
                    v6 = v3 | v5;
                    retval = (v4 | (v6 << 8));
                    break;
                case 4:
                    buf = reader.ReadBytes(4);
                    v2 = (buf[0] << 8) | buf[1];
                    v3 = buf[2];
                    v4 = buf[3];
                    v5 = v2 << 8;
                    v6 = v3 | v5;
                    retval = (v4 | (v6 << 8));
                    break;
                default:
                    retval = 0;
                    break;
            }
            retval += delta;
            return bitmask & retval;
        }

        public byte[] Decompress(byte[] blob)
        {
            byte[] output;

            using (var reader = new BinaryReader(new MemoryStream(blob)))
            {
                var currBlobSize = blob.Length;
                var blobBuffer = reader.ReadBytes(currBlobSize);

                using (MemoryStream decompressedFileStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream = new DeflateStream(new MemoryStream(blobBuffer), CompressionMode.Decompress))
                    {
                        decompressionStream.BaseStream.ReadByte();
                        decompressionStream.BaseStream.ReadByte();
                        decompressionStream.CopyTo(decompressedFileStream, currBlobSize);
                    }

                    output = decompressedFileStream.GetBuffer();
                }
            }

            return output;
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
