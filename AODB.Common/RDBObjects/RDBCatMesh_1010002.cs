using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AODB.Common.Structs;
using AODB.Common.DbClasses;
using Quaternion = AODB.Common.Structs.Quaternion;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1010002)]
    public class RDBCatMesh : RDBObject
    {
        public byte[] Unk1;
        public List<Texture> Textures;
        public int Unk2;
        public int Identifier;
        public int Version;
        public float Unk3;
        public float Unk4;
        public float Unk5;
        public List<Material> Materials;
        public List<UnknownFloatStruct> UnknownFloats;
        public List<Joint> Joints;
        public List<MeshGroup> MeshGroups;
        public int Unk6;
        public List<Attractor> Attractors;

        public override void Deserialize(BinaryReader reader)
        {
            Unk1 = reader.ReadBytes(32);

            Textures = new List<Texture>();
            int textureCount = reader.ReadInt32() / 1009 - 1;

            for (int i = 0; i < textureCount; i++)
            {
                Textures.Add(new Texture
                {
                    Name = new string(reader.ReadChars(32)).Replace("\u0000", ""),
                    Texture1 = reader.ReadInt32(),
                    Texture2 = reader.ReadInt32(),
                    Texture3 = reader.ReadInt32()
                });
            }

            Unk2 = reader.ReadInt32();

            Identifier = reader.ReadInt32();
            Version = reader.ReadInt32();

            Unk3 = reader.ReadSingle();
            Unk4 = reader.ReadSingle();
            Unk5 = reader.ReadSingle();

            int numMaterials = reader.ReadInt32();

            Materials = new List<Material>();
            for (int i = 0; i < numMaterials; i++)
            {
                Material material = new Material();

                material.Name = new string(reader.ReadChars(reader.ReadInt32()));

                material.Unknown2 = reader.ReadInt32();

                material.TextureName = new string(reader.ReadChars(reader.ReadInt32()));

                //Probably flags but yolo
                if (material.Unknown2 == 2 || material.Unknown2 == 6 || material.Unknown2 == 10)
                    material.EnvTextureName = new string(reader.ReadChars(reader.ReadInt32()));

                material.Color1 = reader.ReadRGBColor();
                material.Color2 = reader.ReadRGBColor();
                material.Color3 = reader.ReadRGBColor();
                material.Color4 = reader.ReadRGBColor();

                material.Unknown3 = reader.ReadSingle();
                material.Unknown4 = reader.ReadSingle();
                material.Unknown5 = reader.ReadSingle();

                Materials.Add(material);
            }

            var unknown1Count = reader.ReadInt32();
            UnknownFloats = new List<UnknownFloatStruct>();
            for (int i = 0; i < unknown1Count; i++)
            {
                UnknownFloats.Add(new UnknownFloatStruct
                {
                    Unk1 = reader.ReadSingle(),
                    Unk2 = reader.ReadSingle(),
                    Unk3 = reader.ReadSingle(),
                    Unk4 = reader.ReadSingle(),
                });
            }

            int numJoints = reader.ReadInt32();
            Joints = new List<Joint>();
            for (int i = 0; i < numJoints; i++)
            {
                Joint joint = new Joint();

                joint.Name = new string(reader.ReadChars(reader.ReadInt32()));
                joint.Scale = reader.ReadSingle();
                int numChildJoints = reader.ReadInt32();
                int[] childJoints = new int[numChildJoints];

                for (int j = 0; j < numChildJoints; j++)
                    childJoints[j] = reader.ReadInt32();

                joint.ChildJoints = childJoints;

                Joints.Add(joint);
            }

            int groupCount = reader.ReadInt32();
            MeshGroups = new List<MeshGroup>();
            for (int i = 0; i < groupCount; i++)
            {
                MeshGroup group = new MeshGroup();

                group.Name = new string(reader.ReadChars(reader.ReadInt32()));

                int numMeshes = reader.ReadInt32();

                List<Mesh> meshes = new List<Mesh>();

                for (int j = 0; j < numMeshes; j++)
                {
                    Mesh mesh = new Mesh();
                    mesh.MaterialId = reader.ReadInt32();

                    int vertexCount = reader.ReadInt32();

                    List<Vertex> vertices = new List<Vertex>();
                    for (int k = 0; k < vertexCount; k++)
                    {
                        Vertex vertex = new Vertex();

                        vertex.RelToJoint1 = reader.ReadVector3();
                        vertex.RelToJoint2 = reader.ReadVector3();

                        vertex.Position = reader.ReadVector3();
                        vertex.Normal = reader.ReadVector3();

                        vertex.Uvs = new Vector2()
                        {
                            X = reader.ReadSingle(),
                            Y = 1f - reader.ReadSingle(),
                        };

                        vertex.Joint1 = reader.ReadInt32();
                        vertex.Joint2 = reader.ReadInt32();
                        vertex.Joint1Weight = reader.ReadSingle();

                        vertices.Add(vertex);
                    }

                    mesh.Vertices = vertices;

                    var numTriangles = reader.ReadInt32();

                    int[] triangles = new int[numTriangles];
                    for (int k = 0; k < numTriangles; k++)
                        triangles[k] = reader.ReadInt16();

                    mesh.Triangles = triangles;

                    meshes.Add(mesh);
                }

                group.Meshes = meshes;
                MeshGroups.Add(group);

                if (group.Name != "-noselgroup-")
                {
                    group.Unk = reader.ReadInt32();
                    int numThings = reader.ReadInt32();

                    for (int j = 0; j < numThings; j++)
                    {
                        group.UnknownThings.Add(new UnknownThing
                        {
                            Name = new string(reader.ReadChars(reader.ReadInt32())),
                            Unk1 = reader.ReadSingle(),
                            Unk2 = reader.ReadSingle(),
                            Unk3 = reader.ReadSingle(),
                            Unk4 = reader.ReadSingle(),
                            Unk5 = reader.ReadSingle(),
                            Unk6 = reader.ReadSingle(),
                            Unk7 = reader.ReadSingle(),
                            Unk8 = reader.ReadSingle(),
                            Unk9 = reader.ReadInt32(),
                        });
                    }
                }
            }

            Unk6 = reader.ReadInt32();

            int numAttractors = reader.ReadInt32();

            Attractors = new List<Attractor>();
            for (int i = 0; i < numAttractors; i++)
            {
                Attractors.Add(new Attractor
                {
                    Name = new string(reader.ReadChars(reader.ReadInt32())),
                    Position = reader.ReadVector3(),
                    Rotation = reader.ReadQuaternion(),
                    Scale = reader.ReadSingle(),
                    Unknown = reader.ReadInt32(), // Likely relative bone
                });
            }
        }

        public override byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Unk1);

                    writer.Write((Textures.Count + 1) * 1009);

                    foreach(Texture texture in Textures)
                    {
                        writer.WriteFixedSizeString(texture.Name, 32);
                        writer.Write(texture.Texture1);
                        writer.Write(texture.Texture2);
                        writer.Write(texture.Texture3);
                    }

                    writer.Write(Unk2);
                    writer.Write(Identifier);
                    writer.Write(Version);

                    writer.Write(Unk3);
                    writer.Write(Unk4);
                    writer.Write(Unk5);

                    writer.Write(Materials.Count);

                    foreach (Material material in Materials)
                    {
                        writer.WritePrefixedUTF8String(material.Name);
                        
                        writer.Write(material.Unknown2);

                        writer.WritePrefixedUTF8String(material.TextureName);

                        if (material.Unknown2 == 2 || material.Unknown2 == 6 || material.Unknown2 == 10)
                            writer.WritePrefixedUTF8String(material.EnvTextureName);

                        writer.Write(material.Color1);
                        writer.Write(material.Color2);
                        writer.Write(material.Color3);
                        writer.Write(material.Color4);

                        writer.Write(material.Unknown3);
                        writer.Write(material.Unknown4);
                        writer.Write(material.Unknown5);
                    }

                    writer.Write(UnknownFloats.Count);
                    foreach (UnknownFloatStruct unkFloatStruct in UnknownFloats)
                    {
                        writer.Write(unkFloatStruct.Unk1);
                        writer.Write(unkFloatStruct.Unk2);
                        writer.Write(unkFloatStruct.Unk3);
                        writer.Write(unkFloatStruct.Unk4);
                    }

                    writer.Write(Joints.Count);
                    foreach (Joint joint in Joints)
                    {
                        writer.WritePrefixedUTF8String(joint.Name);
                        writer.Write(joint.Scale);

                        writer.Write(joint.ChildJoints.Length);
                        foreach (int child in joint.ChildJoints)
                            writer.Write(child);
                    }

                    writer.Write(MeshGroups.Count);
                    foreach (MeshGroup meshGroup in MeshGroups)
                    {
                        writer.WritePrefixedUTF8String(meshGroup.Name);

                        writer.Write(meshGroup.Meshes.Count);

                        foreach (Mesh mesh in meshGroup.Meshes)
                        {
                            writer.Write(mesh.MaterialId);

                            writer.Write(mesh.Vertices.Count);

                            foreach(Vertex vertex in mesh.Vertices)
                            {
                                writer.Write(vertex.RelToJoint1);
                                writer.Write(vertex.RelToJoint2);
                                writer.Write(vertex.Position);
                                writer.Write(vertex.Normal);
                                writer.Write(vertex.Uvs);

                                writer.Write(vertex.Joint1);
                                writer.Write(vertex.Joint2);
                                writer.Write(vertex.Joint1Weight);
                            }

                            writer.Write(mesh.Triangles.Length);

                            foreach (ushort tri in mesh.Triangles)
                                writer.Write(tri);
                        }

                        if(meshGroup.Name != "-noselgroup-")
                        {
                            writer.Write(meshGroup.UnknownThings.Count);

                            foreach (UnknownThing thing in meshGroup.UnknownThings)
                            {
                                writer.WritePrefixedUTF8String(thing.Name);
                                writer.Write(thing.Unk1);
                                writer.Write(thing.Unk2);
                                writer.Write(thing.Unk3);
                                writer.Write(thing.Unk4);
                                writer.Write(thing.Unk5);
                                writer.Write(thing.Unk6);
                                writer.Write(thing.Unk7);
                                writer.Write(thing.Unk8);
                                writer.Write(thing.Unk9);
                            }
                        }
                    }

                    writer.Write(Unk6);

                    writer.Write(Attractors.Count);

                    foreach (Attractor attractor in Attractors)
                    {
                        writer.WritePrefixedUTF8String(attractor.Name);
                        writer.Write(attractor.Position);
                        writer.Write(attractor.Rotation);
                        writer.Write(attractor.Scale);
                        writer.Write(attractor.Unknown);
                    }    

                    return stream.ToArray();
                }
            }
        }


        public class Texture
        {
            public string Name;
            public int Texture1;
            public int Texture2;
            public int Texture3;
        }

        public class Material
        {
            public string Name;
            public int Unknown2;
            public string TextureName;
            public string EnvTextureName;
            public Color Color1;
            public Color Color2;
            public Color Color3;
            public Color Color4;
            public float Unknown3;
            public float Unknown4;
            public float Unknown5;
        }

        public class Joint
        {
            public string Name;
            public float Scale;
            public int[] ChildJoints;
        }

        public class Vertex
        {
            public Vector3 RelToJoint1;
            public Vector3 RelToJoint2;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 Uvs;
            public int Joint1;
            public int Joint2;
            public float Joint1Weight;
        }

        public class Mesh
        {
            public int MaterialId;
            public List<Vertex> Vertices;
            public int[] Triangles;
        }

        public class MeshGroup
        {
            public string Name;
            public List<Mesh> Meshes;
            public int Unk;
            public List<UnknownThing> UnknownThings;
        }

        public class UnknownFloatStruct
        {
            public float Unk1;
            public float Unk2;
            public float Unk3;
            public float Unk4;
        }

        public class UnknownThing
        {
            public string Name;
            public float Unk1;
            public float Unk2;
            public float Unk3;
            public float Unk4;
            public float Unk5;
            public float Unk6;
            public float Unk7;
            public float Unk8;
            public int Unk9;
        }

        public class Attractor
        {
            public string Name;
            public Vector3 Position;
            public Quaternion Rotation;
            public float Scale;
            public int Unknown;
        }
    }
}