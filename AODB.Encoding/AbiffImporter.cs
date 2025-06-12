using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AODB.Common.DbClasses;
using AODB.Common.Structs;
using Quaternion = AODB.Common.Structs.Quaternion;
using static AODB.Common.DbClasses.RDBMesh_t.FAFAnim_t;
using AODB.Common.RDBObjects;

namespace AODB.Encoding
{
    public class AbiffImporter
    {
        public static Scene ToAssimpScene(RDBMesh_t rdbMesh, out Dictionary<int, UVKey[]> uvAnims, out Dictionary<string, List<VectorKey>> transKeys, out Dictionary<string, List<QuaternionKey>> rotKeys)
        {
            AbiffExporter exporter = new AbiffExporter(rdbMesh);
            return exporter.CreateScene(out uvAnims, out transKeys, out rotKeys);
        }

        public static RDBMesh_t LoadFromFBX(string fileName, InfoObject infoObject, out Dictionary<int, Material> mats)
        {
            AssimpContext importer = new AssimpContext();

            Scene scene = importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality);

            RDBMesh_t rdbMesh = Create(scene, infoObject, out mats);

            importer.Dispose();

            return rdbMesh;
        }

        private static RDBMesh_t Create(Scene scene, InfoObject infoObject, out Dictionary<int, Material> mats)
        {
            RDBMesh_t rdbMesh = new RDBMesh_t();
            mats = new Dictionary<int, Material>();

            RDBMesh_t.RRefFrame_t refFrame = new RDBMesh_t.RRefFrame_t()
            {
                anim = -1,
                anim_matrix = Matrix.Empty,
                conn = -1,
                grp_mask = -1,
                local_pos = Vector3.Zero,
                local_rot = Quaternion.Identity,
                scale = 1
            };

            rdbMesh.Members.Add(refFrame);

            foreach (Mesh mesh in scene.Meshes)
            {
                AddMesh(rdbMesh, mesh, scene.Materials, infoObject, mats);
            }

            List<RDBMesh_t.RTriMesh_t> triMeshes = rdbMesh.GetMembers<RDBMesh_t.RTriMesh_t>();
            refFrame.chld_cnt = (uint)triMeshes.Count;
            refFrame.chld = triMeshes.Select(x => rdbMesh.Members.IndexOf(x)).ToArray();

            return rdbMesh;
        }

        private static void AddMesh(RDBMesh_t rdbMesh, Mesh mesh, List<Material> materials, InfoObject infoObject, Dictionary<int, Material> mats)
        {
            Console.WriteLine(mesh.Vertices.Count);
            Console.WriteLine(mesh.Faces.Count);

            RDBMesh_t.RTriMesh_t triMesh = new RDBMesh_t.RTriMesh_t()
            {
                anim = -1,
                anim_matrix = Matrix.Empty,
                chld_cnt = 0,
                conn = -1,
                delta_state = -1,
                enable_light = false,
                grp_mask = -1,
                is_cloned = false,
                local_pos = Vector3.Zero,
                local_rot = Quaternion.Identity,
                //local_rot = Quaternion.FromAxisAngleRad(Vector3.right, (float)((Math.PI / 180) * -90)),
                prelight_list_size = 0,
                prio = 3,
                scale = 1f
            };

            rdbMesh.Members.Add(triMesh);

            RDBMesh_t.FAFTriMeshData_t triMeshData = new RDBMesh_t.FAFTriMeshData_t()
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

            RDBMesh_t.SimpleMesh simpleMesh = new RDBMesh_t.SimpleMesh()
            {
                material = -1,
                name = "",
                vb_desc = BuildVertexDescriptor(mesh.Vertices.Count),
                version = 1,
                vertices = BuildVertexArray(mesh)
            };

            rdbMesh.Members.Add(simpleMesh);

            RDBMesh_t.TriList triList = new RDBMesh_t.TriList()
            {
                triangles = BuildTriangleArray(mesh)
            };

            rdbMesh.Members.Add(triList);

            RDBMesh_t.BVolume_t bVolume = new RDBMesh_t.BVolume_t()
            {
                max_pos = new Vector3(0.5f, 1.5f, 0.5f),
                min_pos = new Vector3(-0.5f, -0.5f, -0.5f),
                sph_pos = new Vector3(0, 0, 0),
                sph_radius = 1,
                version = 1
            };

            rdbMesh.Members.Add(bVolume);

            RDBMesh_t.FAFMaterial_t material = new RDBMesh_t.FAFMaterial_t()
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
                name = $"Material_{mesh.Name}",
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

            RDBMesh_t.RDeltaState deltaState = new RDBMesh_t.RDeltaState()
            {
                version = 1,
                name = "noname",
                rst_count = 1,
                rst_type = new uint[] { 29 },
                rst_value = new uint[] { 1 },
                tch_count = 1,
                tch_type = new uint[] { 0 },
                tstv_count = 0,
                tstm_count = new uint[] { 0 },
            };

            rdbMesh.Members.Add(deltaState);

            RDBMesh_t.FAFTexture_t texture = new RDBMesh_t.FAFTexture_t()
            {
                name = $"Texture_{mesh.Name}",
                version = 1
            };

            rdbMesh.Members.Add(texture);

            RDBMesh_t.AnarchyTexCreator_t texCreator = new RDBMesh_t.AnarchyTexCreator_t()
            {
                type = 1010004,
                inst = (uint)GetMatId(materials[mesh.MaterialIndex], infoObject)
            };

            mats.Add((int)texCreator.inst, materials[mesh.MaterialIndex]);

            rdbMesh.Members.Add(texCreator);

            triMesh.data = rdbMesh.Members.IndexOf(triMeshData);
            triMeshData.mesh = new int[] { rdbMesh.Members.IndexOf(simpleMesh) } ;
            triMeshData.bvol = rdbMesh.Members.IndexOf(bVolume);
            simpleMesh.trilist = rdbMesh.Members.IndexOf(triList);
            simpleMesh.material = rdbMesh.Members.IndexOf(material);
            material.delta_state = rdbMesh.Members.IndexOf(deltaState);
            deltaState.tch_text = new int[] { rdbMesh.Members.IndexOf(texture) };
            texture.creator = rdbMesh.Members.IndexOf(texCreator);
        }

        private static byte[] BuildVertexDescriptor(int numVertices)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(0x10);
                    writer.Write(0x10000);
                    writer.Write(0x112);
                    writer.Write(numVertices);

                    return stream.ToArray();
                }
            }
        }

        private static byte[] BuildVertexArray(Mesh mesh)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {

                    writer.Write(mesh.Vertices.Count * 32);

                    for(int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        writer.Write(mesh.Vertices[i].X);
                        writer.Write(mesh.Vertices[i].Y);
                        writer.Write(mesh.Vertices[i].Z);

                        writer.Write(mesh.Normals[i].X);
                        writer.Write(mesh.Normals[i].Y);
                        writer.Write(mesh.Normals[i].Z);

                        writer.Write(mesh.TextureCoordinateChannels[0][i].X);
                        writer.Write(-mesh.TextureCoordinateChannels[0][i].Y);

                        //Console.WriteLine($"({mesh.Vertices[i].X}, {mesh.Vertices[i].Y}, {mesh.Vertices[i].Z}) | ({mesh.TextureCoordinateChannels[0][i].X}, {mesh.TextureCoordinateChannels[0][i].Y})");
                    }

                    return stream.ToArray();
                }
            }
        }

        private static byte[] BuildTriangleArray(Mesh mesh)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    short[] indices = mesh.GetShortIndices();
                    writer.Write(indices.Length * 2);

                    foreach (short index in indices)
                        writer.Write(index);

                    return stream.ToArray();
                }
            }
        }

        private static int GetMatId(Material material, InfoObject infoObject)
        {
            if (material.TextureDiffuse.FilePath == null)
                return 0;

            if (infoObject.Types[ResourceTypeId.Texture].TryGetKey(material.TextureDiffuse.FilePath, out int key))
            {
                Console.WriteLine($"Texture {material.TextureDiffuse.FilePath} found at key {key}");

                return key;
            }

            var keys = infoObject.Types[ResourceTypeId.Texture].Keys.ToArray();

            for (int i = 0; i < keys.Length - 1; i++)
            {
                int nextKey = keys[i] + 1;
                if (nextKey != keys[i + 1])
                {
                    Console.WriteLine($"Adding new InfoObject key. Texture:{nextKey} = {material.TextureDiffuse.FilePath}");

                    if (infoObject.Types[ResourceTypeId.Texture].ContainsKey(nextKey))
                        continue;

                    infoObject.Types[ResourceTypeId.Texture].Add(nextKey, material.TextureDiffuse.FilePath);

                    return nextKey;
                }
            }

            return 0;
        }
    }
}
