using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AODB.Common.DbClasses;
using AODB.Common.Structs;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1010003)]
    public class CATAnim : RDBObject
    {
        public string Name;
        public AnimationIdentifier[] AnimationIdentifiers;
        public float Unknown1;
        public int Unknown2;
        public float Unknown3;
        public int BoneCount;
        public AnimationData Animation;

        public override void Deserialize(BinaryReader reader)
        {
            byte[] nameBytes = reader.ReadBytes(32);
            Name = Encoding.ASCII.GetString(nameBytes);

            int animationIdentifiersCount = reader.ReadInt32();
            AnimationIdentifiers = new AnimationIdentifier[animationIdentifiersCount];
            for (int i = 0; i < animationIdentifiersCount; i++)
            {
                AnimationIdentifier animationIdentifier = new AnimationIdentifier();
                animationIdentifier.Type = reader.ReadInt32();
                var nameBytes2 = reader.ReadBytes(32);
                // Ignore all bytes after first 0
                for (int j = 0; j < nameBytes2.Length; j++)
                {
                    if (nameBytes2[j] == 0)
                    {
                        nameBytes2 = nameBytes2.Take(j).ToArray();
                        break;
                    }
                }
                animationIdentifier.Name = Encoding.ASCII.GetString(nameBytes2);
                AnimationIdentifiers[i] = animationIdentifier;
                //Console.WriteLine($"Animation Identifier: {animationIdentifier.Type}, {animationIdentifier.Name}");
            }

            var fileType = reader.ReadInt32();

            if (fileType != 3)
            {
                throw new InvalidDataException("This is not an animation file");
            }

            var version = reader.ReadInt32();
            var compressed = false;

            if ((version & 0x1000000) != 0)
            {
                version = (int)(version & ~0x1000000u);
                compressed = true;
            }

            Unknown1 = version >= 262 ? reader.ReadSingle() : reader.ReadInt32();
            Unknown2 = reader.ReadInt32();
            Unknown3 = reader.ReadSingle();

            BoneCount = reader.ReadInt32();
            Animation = new AnimationData();
            Animation.BoneData = new List<BoneData>();

            if (!compressed)
            {
                throw new Exception("Uncompressed animations are not supported");
            }

            byte rotationOffset = reader.ReadByte();
            byte positionOffset = reader.ReadByte();

            var blobs1 = ParseZlibBlobs(reader, 0x20, 1);
            var blobs2 = ParseZlibBlobs(reader, version >= 262 ? 32 : 20, 1);
            var blobs3 = ParseZlibBlobs(reader, rotationOffset, 4);
            var blobs4 = ParseZlibBlobs(reader, 0x20, 1);
            var blobs5 = ParseZlibBlobs(reader, positionOffset, 3);

            for (int i = 0; i < BoneCount; i++)
            {
                var boneId = GetNextValueFromBlob(blobs1);
                var unk = GetNextValueFromBlob(blobs1);
                var unk1 = GetNextValueFromBlob(blobs1);

                var boneData = new BoneData
                {
                    BoneId = boneId,
                    Unknown1 = unk,
                    Unknown2 = unk1
                };


                var rotationKeys = GetNextValueFromBlob(blobs1);

                //Console.WriteLine("Bone: " + boneId);
                //Console.WriteLine($"  Rotations(${rotationKeys}):");
                boneData.RotationKeys = new List<RotationKey>();

                for (int j = 0; j < rotationKeys; j++)
                {
                    var timestampInt = GetNextValueFromBlob(blobs2);
                    float timestamp = version >= 262 ? ReinterpretCast<int, float>(timestampInt) : timestampInt;
                    var offset = (float)(1 << (rotationOffset - 1));

                    var x = (float)GetNextValueFromBlob(blobs3);
                    x -= offset;

                    var y = (float)GetNextValueFromBlob(blobs3);
                    y -= offset;

                    var z = (float)GetNextValueFromBlob(blobs3);
                    z -= offset;

                    var w = (float)GetNextValueFromBlob(blobs3);
                    w -= offset;

                    var length = y * y + x * x + z * z + w * w;
                    length = (float)Math.Sqrt(length);
                    length = 1.0f / length;

                    x = length * x;
                    y = length * y;
                    z = length * z;
                    w = length * w;

                    //Console.WriteLine($"    Timestamp: {timestamp:F2}, x: {x:F3}, y: {y:F3}, z: {z:F3}, w: {w:F3}");

                    boneData.RotationKeys.Add(new RotationKey()
                    {
                        Time = timestamp,
                        Rotation = new Quaternion(x, y, z, w).Normalize(),
                    });
                }

                var xStart = ReinterpretCast<int, float>(GetNextValueFromBlob(blobs4));
                var xEnd = ReinterpretCast<int, float>(GetNextValueFromBlob(blobs4));
                var yStart = ReinterpretCast<int, float>(GetNextValueFromBlob(blobs4));
                var yEnd = ReinterpretCast<int, float>(GetNextValueFromBlob(blobs4));
                var zStart = ReinterpretCast<int, float>(GetNextValueFromBlob(blobs4));
                var zEnd = ReinterpretCast<int, float>(GetNextValueFromBlob(blobs4));

                var positionKeys = GetNextValueFromBlob(blobs1);

                boneData.TranslationKeys = new List<TranslationKey>();

                //Console.WriteLine($"  Positions(${positionKeys}):");
                for (int j = 0; j < positionKeys; j++)
                {
                    var timestampInt = GetNextValueFromBlob(blobs2);
                    float timestamp = version >= 262 ? ReinterpretCast<int, float>(timestampInt) : timestampInt;
                    var posX = (float)GetNextValueFromBlob(blobs5);
                    var posY = (float)GetNextValueFromBlob(blobs5);
                    var posZ = GetNextValueFromBlob(blobs5);
                    var offset = (1 << positionOffset) - 1;

                    posX /= offset;
                    var x = posX * (xEnd - xStart) + xStart;
                    posY /= offset;
                    var y = posY * (yEnd - yStart) + yStart;

                    double z = posZ;
                    if (posZ < 0)
                    {
                        z += 4294967300.0;
                    }

                    z /= offset;

                    z = z * (zEnd - zStart) + zStart;

                    boneData.TranslationKeys.Add(new TranslationKey()
                    {
                        Time = timestamp,
                        Position = new Vector3(x, y, z),
                    });

                    //Console.WriteLine($"    Timestamp: {timestamp:F2}, x: {x:F3}, y: {y:F3}, z: {z:F3}");
                }

                Animation.BoneData.Add(boneData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TResult ReinterpretCast<TInput, TResult>(TInput value) where TInput : unmanaged where TResult : unmanaged
        {
            unsafe
            {
                return *(TResult*)&value;
            }
        }

        private unsafe int GetNextValueFromBlob(AnimationBlob blob)
        {
            var v7 = 0;

            if (blob.EncodingSize == 1)
            {
                v7 = blob.Blobs[blob.CurrentBlob].ByteData[blob.CurrentIndex];
            }
            else 
            {
                fixed (byte* v10 = &blob.Blobs[blob.CurrentBlob].ByteData[blob.CurrentIndex])
                {
                    if  (blob.EncodingSize == 2)
                    {
                        var v6 = v10[0];
                        var v4 = v10[1];
                        v7 = v4 | (v6 << 8);
                    }
                    else if (blob.EncodingSize == 3)
                    {
                        var v9 = *(int*)v10;
                        var v5 = v10[1];
                        var v4 = v10[2];
                        var v3 = v9 << 8;
                        var v6 = v3 | v5;
                        v7 = v4 | (v6 << 8);
                    }
                    else if (blob.EncodingSize == 4)
                    {
                        var v2 = (*v10 << 8) | v10[1];
                        var v3 = v10[2];
                        var v4 = v10[3];
                        var v5 = v2 << 8;
                        var v6 = v3 | v5;
                        v7 = v4 | (v6 << 8);
                    }
                }
            }

            blob.Blobs[blob.CurrentBlob].Field10 += v7;

            int result = blob.Field4 & blob.Blobs[blob.CurrentBlob].Field10;

            if (++blob.CurrentBlob == blob.Count)
            {
                blob.CurrentBlob = 0;
                blob.CurrentIndex += blob.EncodingSize;
            }

            return result;
        }

        private class BlobStruct
        {
            public byte[] ByteData;
            public int Field4;
            public int Field8;
            public int FieldC;
            public int Field10;
        }

        private class AnimationBlob
        {
            public int Field0;
            public int Field4;
            public int EncodingSize;
            public int CurrentBlob;
            public int CurrentIndex;
            public int Count;
            public int Field1C;
            public int Field20;
            public int Field24;
            public List<BlobStruct> Blobs;
        }

        private AnimationBlob ParseZlibBlobs(BinaryReader reader, int unk, int blobCount)
        {
            AnimationBlob blob = new AnimationBlob();
            blob.Field20 = 0;
            blob.CurrentBlob = 0;
            blob.CurrentIndex = 0;
            blob.Field0 = unk;

            if (unk == 32)
            {
                blob.Field4 = -1;
            }
            else
            {
                blob.Field4 = (1 << unk) - 1;
            }

            blob.EncodingSize = (unk + 7) >> 3;
            blob.Count = blobCount;


            List<BlobStruct> blobs = new List<BlobStruct>();
            for (int i = 0; i < blobCount; i++)
            {
                var compressedSize = reader.ReadInt32();
                var uncompressedSize = reader.ReadInt32();
                var compressedData = reader.ReadBytes(compressedSize);

                using (MemoryStream decompressedFileStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream = new DeflateStream(new MemoryStream(compressedData), CompressionMode.Decompress))
                    {
                        decompressionStream.BaseStream.ReadByte();
                        decompressionStream.BaseStream.ReadByte();
                        decompressionStream.CopyTo(decompressedFileStream);
                    }


                    var bytes = decompressedFileStream.GetBuffer();
                    BlobStruct blobStruct = new BlobStruct();
                    blobStruct.ByteData = bytes;
                    blobStruct.Field10 = 0;

                    blobs.Add(blobStruct);
                }
            }

            blob.Blobs = blobs;

            return blob;
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}

public struct AnimationIdentifier
{
    public int Type;
    public string Name;
}

public struct TranslationKey
{
    public float Time;
    public Vector3 Position;
}

public struct RotationKey
{
    public float Time;
    public Quaternion Rotation;
}

public struct BoneData
{
    public int BoneId;
    public int Unknown1;
    public int Unknown2;
    public List<TranslationKey> TranslationKeys;
    public List<RotationKey> RotationKeys;
}

public struct AnimationData
{
    public List<BoneData> BoneData;
}