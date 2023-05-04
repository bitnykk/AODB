using AODB.Common.Structs;
using System.IO;

namespace AODB.Common.DbClasses
{
    public class RDBMesh_t : DbClass
    {
        public class Transform
        {
            public Matrix anim_matrix { get; set; }
            public int grp_mask { get; set; }
            public Vector3 local_pos { get; set; }
            public Quaternion local_rot { get; set; }
            public float scale { get; set; }
            public int anim { get; set; }
            public int conn { get; set; }
            public uint chld_cnt { get; set; }
            public int[] chld { get; set; }
        }

        public class RRefFrame_t : Transform
        {
        }

        public class RTriMesh_t : Transform
        {
            public int prio { get; set; }
            public bool enable_light { get; set; }
            public int delta_state { get; set; }
            public int data { get; set; }
            public bool is_cloned { get; set; }
            public int prelight_list_size { get; set; }
        }

        public class FAFTriMeshData_t
        {
            public int version { get; set; }
            public string name { get; set; }
            public Vector3 anim_pos { get; set; }
            public Quaternion anim_rot { get; set; }
            public uint num_meshes { get; set; }
            public bool isdegen { get; set; }
            public int[] mesh { get; set; }
            public int bvol { get; set; }
        }

        public class SimpleMesh
        {
            public int version { get; set; }
            public string name { get; set; }
            public int material { get; set; }
            public int trilist { get; set; }
            [AODBSerializer.RealSize]
            public byte[] vb_desc { get; set; }
            public byte[] vertices { get; set; }

            [RDBDoNotSerialize]
            public VertexDescription VertexDescription
            {
                get { return GetVertexDescription(); }
            }

            [RDBDoNotSerialize]
            public Vertex[] Vertices
            {
                get { return GetVertices(); }
            }

            private VertexDescription GetVertexDescription()
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(vb_desc)))
                {
                    return new VertexDescription
                    {
                        Unk1 = reader.ReadInt32(),
                        Unk2 = reader.ReadInt32(),
                        Unk3 = reader.ReadInt32(),
                        NumVertices = reader.ReadInt32(),
                    };
                }
            }

            private Vertex[] GetVertices()
            {
                VertexDescription vertDesc = VertexDescription;
                Vertex[] verts = new Vertex[vertDesc.NumVertices];
                using (BinaryReader reader = new BinaryReader(new MemoryStream(vertices)))
                {
                    reader.ReadSingle(); //VertexBufferLen

                    for (int i = 0; i < vertDesc.NumVertices; i++)
                    {
                        if (vertDesc.Unk3 == 0x112)
                        {
                            verts[i] = new Vertex()
                            {
                                Position = new Vector3()
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle()
                                },
                                Normals = new Vector3()
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle()
                                },
                                UVs = new Vector2()
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle()
                                }
                            };
                        }
                        else if (vertDesc.Unk3 == 0x152)
                        {
                            verts[i] = new Vertex()
                            {
                                Position = new Vector3()
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle()
                                },
                                Normals = new Vector3()
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle()
                                },
                                Color = new Color()
                                {
                                    R = reader.ReadByte(),
                                    G = reader.ReadByte(),
                                    B = reader.ReadByte(),
                                    A = reader.ReadByte()
                                },
                                UVs = new Vector2()
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle()
                                }
                            };
                        }
                    }
                }

                return verts;
            }
        }

        public class FAFMaterial_t
        {
            public int delta_state { get; set; }
            public int version { get; set; }
            public string name { get; set; }
            public int env_texture { get; set; }
            public Color diff { get; set; }
            public Color spec { get; set; }
            public Color ambi { get; set; }
            public Color emis { get; set; }
            public float shin { get; set; }
            public float shin_str { get; set; }
            public float opac { get; set; }
        }

        public class FAFPointLight_t : Transform
        {
            public int version { get; set; }
            public byte[] light_info { get; set; }
        }

        public class FAFSpotLight_t : Transform
        {
            public int version { get; set; }
            public byte[] light_info { get; set; }
        }

        public class DefaultMaterial_t
        {
            public int version { get; set; }
            public string name { get; set; }
            public int delta_state { get; set; }
            public int env_texture { get; set; }
            public Color diff { get; set; }
            public Color spec { get; set; }
            public Color ambi { get; set; }
            public Color emis { get; set; }
            public float shin { get; set; }
            public float shin_str { get; set; }
            public float opac { get; set; }
        }

        public class RDeltaState
        {
            public uint version { get; set; }
            public string name { get; set; }
            public uint rst_count { get; set; }
            public uint[] rst_type { get; set; }
            public uint[] rst_value { get; set; }
            public uint tstv_count { get; set; }
            public uint tch_count { get; set; }
            public uint[] tch_type { get; set; }
            public int[] tch_text { get; set; }
            public uint[] tstm_count { get; set; }
            public uint[] tst_type { get; set; }
            public uint[] tst_value { get; set; }
        }

        public class FAFTexture_t
        {
            public uint version { get; set; }
            public string name { get; set; }
            public int creator { get; set; }
        }

        public class AnarchyTexCreator_t
        {
            public uint type { get; set; }
            public uint inst { get; set; }
        }

        public class TriList
        {
            public byte[] triangles { get; set; }

            [RDBDoNotSerialize]
            public int[] Triangles
            {
                get { return GetTriangles(); }
            }

            private int[] GetTriangles()
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(triangles)))
                {
                    var numTriangles = reader.ReadInt32() / 2;

                    int[] triangles = new int[numTriangles];

                    for (int i = 0; i < numTriangles; i++)
                        triangles[i] = reader.ReadInt16();

                    return triangles;
                }
            }
        }

        public class BVolume_t
        {
            public int version { get; set; }
            public Vector3 sph_pos { get; set; }
            public float sph_radius { get; set; }
            public Vector3 min_pos { get; set; }
            public Vector3 max_pos { get; set; }
        }

        public class FAFCollisionSphere_c : Transform
        {
        }

        public class FAFCollisionBox_c : Transform
        {
        }

        public class FAFAnim_t
        {
            public int version { get; set; }
            public string name { get; set; }
            public float tot_time { get; set; }
            public bool loop { get; set; }
            public int? num_rot_keys { get; set; }
            public byte[] rot_keys { get; set; }
            public int? num_trans_keys { get; set; }
            public byte[] trans_keys { get; set; }
            public int? num_vis_keys { get; set; }
            public byte[] vis_keys { get; set; }
            public int? num_uv_keys { get; set; }
            public byte[] uv_keys { get; set; }

            public UVKey[] UVKeys
            {
                get { return GetUVKeys(); }
            }

            private UVKey[] GetUVKeys()
            {
                if(num_uv_keys == null)
                    return null;

                using (BinaryReader reader = new BinaryReader(new MemoryStream(uv_keys)))
                {
                    var length = reader.ReadInt32();

                    UVKey[] uvKeys = new UVKey[num_uv_keys.Value];

                    for (int i = 0; i < num_uv_keys; i++)
                    {
                        uvKeys[i] = new UVKey
                        {
                            Tiling = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                            Offset = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                            Time = reader.ReadSingle(),
                            Unk2 = reader.ReadInt32()
                        };
                    }

                    return uvKeys;
                }
            }

            public struct UVKey
            {
                public Vector2 Tiling;
                public Vector2 Offset;
                public float Time;
                public int Unk2;
            }
        }

        public class FAFAttractor_t : RRefFrame_t
        {

        }

        public class RRefFrameConnector
        {
            public int version { get; set; }
            public string name { get; set; }
            public int originator { get; set; }

        }

        public class RLight_t : Transform
        {
            public int version { get; set; }
            public byte[] light_info { get; set; }
        }


        public class TextureFileCreator
        {
            public string texture_path { get; set; }
            public string alpha_path { get; set; }
            public uint pref_pix_format { get; set; }
            public bool mipmap_enable { get; set; }
        }

        public class Submesh
        {
            public Vertex[] Vertices;
            public int[] Triangles;
            public Vector3 BasePos;
            public Quaternion BaseRotation;
            public AOMaterial Material;
        }

        /*
        public static RDBMesh_t Create(FbxIndexer fbxIndexer, long[] geometryIds)
        {
            uint[] testTexIds = new uint[]
            {
                2000005, //Body02 //
                2000003, //Body02 // 
                2000004, //Body02 //
                2000001, //Body02 
                2000002, //Body01
                2000002, //Hair01
                2000002, //Hair02 //
                2000001, //Head01
            };

            RDBMesh_t rdbMesh = new RDBMesh_t();

            RRefFrame_t refFrame = new RRefFrame_t()
            {
                anim = -1,
                anim_matrix = Matrix.Empty,
                conn = -1,
                grp_mask = 4294967295,
                local_pos = Vector3.Zero,
                local_rot = Quaternion.Identity,
                scale = 1
            };

            rdbMesh.Members.Add(refFrame);

            int texIdx = 0;
            foreach (long geometryId in geometryIds)
            {
                rdbMesh.AddMesh(rdbMesh, fbxIndexer, geometryId, testTexIds[texIdx++]);
            }

            List<RTriMesh_t> triMeshes = rdbMesh.GetMembers<RTriMesh_t>();
            refFrame.chld_cnt = (uint)triMeshes.Count;
            refFrame.chld = triMeshes.Select(x => rdbMesh.Members.IndexOf(x)).ToArray();

            return rdbMesh;
        }

        protected void AddMesh(RDBMesh_t rdbMesh, FbxIndexer fbxIndexer, long geometryId, uint texId)
        {
            fbxIndexer.Index(geometryId, out FbxVertex[] vertices, out int[] indices);

            Console.WriteLine(vertices.Length);
            Console.WriteLine(indices.Length);

            RTriMesh_t triMesh = new RTriMesh_t()
            {
                anim = -1,
                anim_matrix = Matrix.Empty,
                chld_cnt = 0,
                conn = -1,
                delta_state = -1,
                enable_light = false,
                grp_mask = 4294967295,
                is_cloned = false,
                local_pos = Vector3.Zero,
                local_rot = Quaternion.FromAxisAngleRad(Vector3.Right, (float)((Math.PI / 180) * -90)),
                prelight_list_size = 0,
                prio = 3,
                scale = 0.015f
            };

            rdbMesh.Members.Add(triMesh);

            FAFTriMeshData_t triMeshData = new FAFTriMeshData_t()
            {
                anim_pos = Vector3.Zero,
                anim_rot = Quaternion.Identity,
                bvol = -1,
                isdegen = false,
                name = "",
                num_meshes = 1,
                version = 1
            };

            rdbMesh.Members.Add(triMeshData);

            SimpleMesh simpleMesh = new SimpleMesh()
            {
                material = -1,
                name = "",
                vb_desc = BuildVertexDescriptor(vertices),
                version = 1,
                vertices = BuildVertexArray(vertices)
            };

            rdbMesh.Members.Add(simpleMesh);

            TriList triList = new TriList()
            {
                triangles = BuildTriangleArray(indices)
            };

            rdbMesh.Members.Add(triList);

            BVolume_t bVolume = new BVolume_t()
            {
                max_pos = new Vector3(0.5f, 1.5f, 0.5f),
                min_pos = new Vector3(-0.5f, -0.5f, -0.5f),
                sph_pos = new Vector3(0, 0, 0),
                sph_radius = 1,
                version = 1
            };

            rdbMesh.Members.Add(bVolume);

            FAFMaterial_t material = new FAFMaterial_t()
            {
                ambi = new Color()
                {
                    A = 0,
                    R = 1,
                    B = 1,
                    G = 1
                },
                delta_state = -1,
                diff = new Color()
                {
                    A = 0,
                    R = 1,
                    B = 1,
                    G = 1
                },
                emis = new Color()
                {
                    A = 0,
                    R = 1,
                    B = 1,
                    G = 1
                },
                env_texture = -1,
                name = $"Material{geometryId}",
                opac = 1,
                shin = 1,
                shin_str = 0,
                spec = new Color()
                {
                    A = 0,
                    R = 0.9f,
                    B = 0.9f,
                    G = 0.9f
                },
                version = 1
            };

            rdbMesh.Members.Add(material);

            RDeltaState deltaState = new RDeltaState()
            {
                version = 1,
                Version = 1,
                name = "noname",
                rst_count = 1,
                rst_type = 29,
                rst_value = 1,
                tch_count = 1,
                tch_type = new uint[] { 0 },
                tstv_count = 0,
                tstm_count = new uint[] { 0 },
            };

            rdbMesh.Members.Add(deltaState);

            FAFTexture_t texture = new FAFTexture_t()
            {
                name = $"Texture{geometryId}",
                version = 1
            };

            rdbMesh.Members.Add(texture);

            AnarchyTexCreator_t texCreator = new AnarchyTexCreator_t()
            {
                type = 1010004,
                inst = texId
            };

            rdbMesh.Members.Add(texCreator);

            triMesh.data = rdbMesh.Members.IndexOf(triMeshData);
            triMeshData.mesh = new int[] { rdbMesh.Members.IndexOf(simpleMesh) };
            triMeshData.bvol = rdbMesh.Members.IndexOf(bVolume);
            simpleMesh.trilist = rdbMesh.Members.IndexOf(triList);
            simpleMesh.material = rdbMesh.Members.IndexOf(material);
            material.delta_state = rdbMesh.Members.IndexOf(deltaState);
            deltaState.tch_text = new int[] { rdbMesh.Members.IndexOf(texture) };
            texture.creator = rdbMesh.Members.IndexOf(texCreator);
        }

        protected byte[] BuildVertexDescriptor(FbxVertex[] vertices)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(0x10);
                    writer.Write(0x10000);
                    writer.Write(0x112);
                    writer.Write(vertices.Length);

                    return stream.ToArray();
                }
            }
        }

        protected byte[] BuildVertexArray(FbxVertex[] vertices)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    
                    writer.Write(vertices.Length * 32);

                    foreach(FbxVertex vertex in vertices)
                    {
                        writer.Write(vertex.Position.X);
                        writer.Write(vertex.Position.Y);
                        writer.Write(vertex.Position.Z);

                        writer.Write(vertex.Normal.X);
                        writer.Write(vertex.Normal.Y);
                        writer.Write(vertex.Normal.Z);

                        writer.Write(vertex.TexCoord.X);
                        writer.Write(vertex.TexCoord.Y);
                    }                  

                    return stream.ToArray();
                }
            }
        }

        protected byte[] BuildTriangleArray(int[] indices)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(indices.Length * 2);

                    foreach (int index in indices)
                        writer.Write((short)index);

                    return stream.ToArray();
                }
            }
        }*/
    }
}
