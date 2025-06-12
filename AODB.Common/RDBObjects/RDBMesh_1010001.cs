using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AODB.Common.Structs;
using AODB.Common.DbClasses;

namespace AODB.Common.RDBObjects
{
    [RDBRecord(RecordTypeID = 1010001)]
    public class RDBMesh : RDBObject
    {
        public int Unk1;
        public int Unk2;
        public int Unk3;
        public int Unk4;
        public RDBMesh_t RDBMesh_t;

        public List<RDBMesh_t.Submesh> SubMeshes { get; set; }

        public override void Deserialize(BinaryReader reader)
        {
            Unk1 = reader.ReadInt32();
            Unk2 = reader.ReadInt32();

            if (Unk2 == 3)
            {
                Unk3 = reader.ReadInt32();
            }
            else if (Unk2 == 2)
            {
                reader.BaseStream.Position += 80;
            }
            else if (Unk2 == 0)
            {
                Unk3 = reader.ReadInt32();
                Unk4 = reader.ReadInt32();
            }
            else
            {
                return;
            }

            RDBMesh_t = _serializer.Deserialize<RDBMesh_t>(reader);

            SubMeshes = new List<RDBMesh_t.Submesh>();

            var triMeshes = RDBMesh_t.GetMembers<RDBMesh_t.RTriMesh_t>();

            foreach (var rTriMeshT in triMeshes)
            {
                var faftMeshData = (RDBMesh_t.FAFTriMeshData_t)RDBMesh_t.Members[rTriMeshT.data];

                for (int simpleMeshIndex = 0; simpleMeshIndex < faftMeshData.mesh.Length; simpleMeshIndex++)
                {
                    var simpleMesh = (RDBMesh_t.SimpleMesh)RDBMesh_t.Members[faftMeshData.mesh[simpleMeshIndex]];

                    var vertexBufferDescription = new List<uint>();

                    using (var vbDescReader = new BinaryReader(new MemoryStream(simpleMesh.vb_desc)))
                    {
                        for (int j = 0; j < simpleMesh.vb_desc.Length / 4; j++)
                        {
                            vertexBufferDescription.Add(vbDescReader.ReadUInt32());
                        }
                    }

                    var vertexSize = 0;
                    switch (vertexBufferDescription[2])
                    {
                        case 0x152:
                            vertexSize = 36;
                            break;
                        case 0x112:
                            vertexSize = 32;
                            break;
                        default:
                            //Logger.Log("Unhandled vertex type: " + vertexBufferDescription[2].ToString("X"));
                            break;
                    }


                    Vertex[] vertices;


                    using (var vertexReader = new BinaryReader(new MemoryStream(simpleMesh.vertices)))
                    {
                        var vertexCount = vertexReader.ReadInt32() / vertexSize;

                        vertices = new Vertex[(int)vertexCount];


                        switch (vertexSize)
                        {
                            case 36:
                                for (int j = 0; j < vertexCount; j++)
                                {
                                    vertices[j] = new Vertex();

                                    vertices[j].Position = new Vector3()
                                    {
                                        X = vertexReader.ReadSingle(),
                                        Y = vertexReader.ReadSingle(),
                                        Z = vertexReader.ReadSingle()
                                    };

                                    vertices[j].Normal = new Vector3()
                                    {
                                        X = vertexReader.ReadSingle(),
                                        Y = vertexReader.ReadSingle(),
                                        Z = vertexReader.ReadSingle()
                                    };

                                    vertices[j].Color = new Color()
                                    {
                                        R = vertexReader.ReadByte(),
                                        G = vertexReader.ReadByte(),
                                        B = vertexReader.ReadByte(),
                                        A = vertexReader.ReadByte()
                                    };

                                    vertices[j].UVs = new Vector2()
                                    {
                                        X = vertexReader.ReadSingle(),
                                        Y = vertexReader.ReadSingle()
                                    };
                                }
                                break;
                            case 32:
                                for (int j = 0; j < vertexCount; j++)
                                {
                                    vertices[j] = new Vertex()
                                    {
                                        Position = new Vector3()
                                        {
                                            X = vertexReader.ReadSingle(),
                                            Y = vertexReader.ReadSingle(),
                                            Z = vertexReader.ReadSingle()
                                        },
                                        Normal = new Vector3()
                                        {
                                            X = vertexReader.ReadSingle(),
                                            Y = vertexReader.ReadSingle(),
                                            Z = vertexReader.ReadSingle()
                                        },
                                        UVs = new Vector2()
                                        {
                                            X = vertexReader.ReadSingle(),
                                            Y = vertexReader.ReadSingle()
                                        }
                                    };
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    var triList = simpleMesh.trilist;

                    var triangleData = ((RDBMesh_t.TriList)RDBMesh_t.Members[triList]).triangles;
                    var triangleReader = new BinaryReader(new MemoryStream(triangleData));

                    var size = triangleReader.ReadInt32() / 2;

                    var triangles = new int[size];
                    for (int j = 0; j < size; j++)
                    {
                        triangles[j] = triangleReader.ReadInt16();
                    }


                    RDBMesh_t.FAFMaterial_t material = null;
                    RDBMesh_t.DefaultMaterial_t defaultMaterial;

                    if (simpleMesh.material != -1)
                    {
                        var mat = RDBMesh_t.Members[simpleMesh.material];

                        if (mat is RDBMesh_t.DefaultMaterial_t)
                            defaultMaterial = (RDBMesh_t.DefaultMaterial_t)RDBMesh_t.Members[simpleMesh.material];
                        else
                            material = (RDBMesh_t.FAFMaterial_t)RDBMesh_t.Members[simpleMesh.material];

                    }
                    else
                    {
                        material = null;
                    }

                    RDBMesh_t.RDeltaState matDeltaState = null;
                    if (material != null && material.delta_state != -1)
                        matDeltaState = (RDBMesh_t.RDeltaState)RDBMesh_t.Members[material.delta_state];

                    RDBMesh_t.FAFTexture_t matTexture = null;
                    if (matDeltaState?.tch_text != null)
                        matTexture = (RDBMesh_t.FAFTexture_t)RDBMesh_t.Members[matDeltaState.tch_text[0]];


                    uint textureID = 0;
                    uint textureType = 0;
                    if (matTexture != null)
                    {
                        switch(RDBMesh_t.Members[matTexture.creator])
                        {
                            case RDBMesh_t.AnarchyTexCreator_t texCreator:
                                textureID = texCreator.inst;
                                textureType = texCreator.type;
                                break;
                            case RDBMesh_t.TextureFileCreator texFileCreator:
                                //Not really useful but interesting.
                                matTexture = null;
                                break;
                        }
                    }

                    /*
                    var vars = new Dictionary<int, int>();

                    if (matDeltaState != null && matDeltaState.rst_count >= 1)
                    {
                        for (int j = 0; j < matDeltaState.rst_value; j++)
                        {
                            vars.Add(matDeltaState.rst_type[j], matDeltaState.rst_value[j]);
                        }
                    }
                    */

                    if (matTexture == null)
                    {
                        SubMeshes.Add(new RDBMesh_t.Submesh()
                        {
                            Triangles = triangles,
                            Vertices = vertices,
                            BasePos = faftMeshData.anim_pos,
                            BaseRotation = faftMeshData.anim_rot * rTriMeshT.local_rot,
                        });
                    }
                    else
                    {
                        SubMeshes.Add(new RDBMesh_t.Submesh()
                        {
                            Triangles = triangles,
                            Vertices = vertices,
                            BasePos = faftMeshData.anim_pos,
                            BaseRotation = faftMeshData.anim_rot * rTriMeshT.local_rot,
                            Material = new AOMaterial()
                            {
                                MaterialName = material.name,
                                Texture = textureID,
                                TextureType = textureType,
                                TextureName = matTexture.name,
                                Variables = null
                            }
                        });
                    }
                }
            }


            //PointLights = serializer.DbClass.GetMembers<RDBMesh_t.FAFPointLight_t>();
        }

        public override byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Unk1);
                    writer.Write(Unk2);

                    if (Unk2 == 3)
                    {
                        writer.Write(Unk3);
                    }
                    else if (Unk2 == 0)
                    {
                        writer.Write(Unk3);
                        writer.Write(Unk4);
                    }

                    writer.Write(_serializer.Serialize(RDBMesh_t));

                    return stream.ToArray();
                }
            }
        }
    }
}