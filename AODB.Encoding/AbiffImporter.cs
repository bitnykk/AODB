using AODB.Common.DbClasses;
using AODB.Common.RDBObjects;
using AODB.Common.Structs;
using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AODB.Common.DbClasses.RDBMesh_t;
using static AODB.Common.DbClasses.RDBMesh_t.FAFAnim_t;
using AQuaternion = Assimp.Quaternion;
using AVector3 = Assimp.Vector3D;
using Quaternion = AODB.Common.Structs.Quaternion;

namespace AODB.Encoding
{
    public class AbiffImporter
    {
        public static Scene ToAssimpScene(RDBMesh_t rdbMesh, out Dictionary<int, UVKey[]> uvAnims)
        {
            AbiffExporter exporter = new AbiffExporter(rdbMesh);
            return exporter.CreateScene(out uvAnims);
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

            var root = ProcessNode(scene, scene.RootNode, rdbMesh, infoObject, mats);

            return rdbMesh;
        }

        private static Transform ProcessNode(Scene scene, Node node, RDBMesh_t rdbMesh, InfoObject infoObject, Dictionary<int, Material> mats)
        {
            Transform transform = null;

            var childrenWithMeshes = node.Children.Where(x => x.HasMeshes).ToList();
            if (childrenWithMeshes.Any())
            {
                Dictionary<int, FAFMaterial_t> materialMap = new Dictionary<int, FAFMaterial_t>();
                var triMesh = AddTriMesh(rdbMesh, scene, node, infoObject, mats);
                var triMeshData = rdbMesh.Members[triMesh.data] as FAFTriMeshData_t;

                foreach (Node childNode in childrenWithMeshes)
                {
                    foreach(var mesh in AddMesh(rdbMesh, scene, childNode, infoObject, materialMap, mats))
                        triMeshData.AddMesh(rdbMesh.Members.IndexOf(mesh));
                }

                childrenWithMeshes[0].Transform.Decompose(out AVector3 scale, out AQuaternion rotation, out AVector3 translation);
                triMeshData.anim_pos = translation.ToAODB();
                triMeshData.anim_rot = rotation.ToAODB();

                transform = triMesh;
            }
            else if (node.Name.StartsWith("Attractor") || node.Name.StartsWith("eff"))
            {
                transform = AddAttractor(rdbMesh, node);
            }
            else if (node != scene.RootNode && (!node.HasMeshes || node.HasChildren))
            {
                transform = AddEmptyTransform(rdbMesh, node);
            }

            foreach (Node childNode in node.Children)
            {
                Transform childTransform = ProcessNode(scene, childNode, rdbMesh, infoObject, mats);

                if (childTransform != null)
                    transform?.AddChild(rdbMesh.Members.IndexOf(childTransform));
            }

            if (transform != null)
            {
                if (transform is FAFAttractor_t)
                {
                    var matrix = node.Parent.Transform * node.Transform;
                    matrix.Transpose();
                    transform.anim_matrix = matrix.FromAssimpMatrix();
                }
                else
                {
                    var matrix = node.Transform;
                    matrix.Transpose();
                    transform.anim_matrix = matrix.FromAssimpMatrix();
                }
            }

            return transform;
        }

        private static Transform AddEmptyTransform(RDBMesh_t rdbMesh, Node node)
        {
            RRefFrame_t refFrame = new RRefFrame_t();
            rdbMesh.Members.Add(refFrame);

            return refFrame;
        }

        private static Transform AddAttractor(RDBMesh_t rdbMesh, Node node)
        {
            RRefFrame_t refFrame = new RRefFrame_t();

            rdbMesh.Members.Add(refFrame);

            RRefFrameConnector refFrameConnector = new RRefFrameConnector
            {
                name = node.Name,
                originator = rdbMesh.Members.IndexOf(refFrame)
            };

            rdbMesh.Members.Add(refFrameConnector);

            refFrame.conn = rdbMesh.Members.IndexOf(refFrameConnector);

            return refFrame;
        }

        private static FAFMaterial_t AddMaterial(RDBMesh_t rdbMesh, Material material, InfoObject infoObject, Dictionary<int, Material> mats)
        {
            AnarchyTexCreator_t texCreator = new AnarchyTexCreator_t()
            {
                type = (uint)ResourceTypeId.Texture,
                inst = (uint)GetMatId(material, infoObject)
            };
            rdbMesh.Members.Add(texCreator);

            FAFTexture_t texture = new FAFTexture_t()
            {
                name = "unnamed",
                version = 1,
                creator = rdbMesh.Members.IndexOf(texCreator)
            };

            rdbMesh.Members.Add(texture);

            RDeltaState deltaState = new RDeltaState()
            {
                name = "noname",
                tch_count = 1,
                tch_type = new uint[] { (uint)TextureChannelType.Diffuse },
                tch_text = new int[] { rdbMesh.Members.IndexOf(texture) }
            };
            deltaState.AddRenderStateType(D3DRenderStateType.D3DRS_SPECULARENABLE, 1);

            if (material.IsTwoSided)
                deltaState.AddRenderStateType(D3DRenderStateType.D3DRS_CULLMODE, (int)D3DCULL.D3DCULL_NONE);

            if (material.Opacity < 1)
                deltaState.AddRenderStateType(D3DRenderStateType.D3DRS_ALPHABLENDENABLE, 1);

            rdbMesh.Members.Add(deltaState);

            FAFMaterial_t aoMaterial = new FAFMaterial_t()
            {
                ambi = new Color()
                {
                    A = material.ColorAmbient.A,
                    R = material.ColorAmbient.R,
                    B = material.ColorAmbient.B,
                    G = material.ColorAmbient.G
                },
                diff = new Color()
                {
                    A = material.ColorDiffuse.A,
                    R = material.ColorDiffuse.R,
                    B = material.ColorDiffuse.B,
                    G = material.ColorDiffuse.G
                },
                emis = new Color()
                {
                    A = material.ColorEmissive.A,
                    R = material.ColorEmissive.R,
                    B = material.ColorEmissive.B,
                    G = material.ColorEmissive.G
                },
                name = material.Name,
                opac = material.Opacity,
                shin = material.Shininess,
                shin_str = material.ShininessStrength,
                spec = new Color()
                {
                    A = material.ColorSpecular.A,
                    R = material.ColorSpecular.R,
                    B = material.ColorSpecular.B,
                    G = material.ColorSpecular.G
                },
                version = 1,
                delta_state = rdbMesh.Members.IndexOf(deltaState)
            };

            rdbMesh.Members.Add(aoMaterial);

            if (!mats.ContainsKey((int)texCreator.inst))
                mats.Add((int)texCreator.inst, material);

            return aoMaterial;
        }

        private static RTriMesh_t AddTriMesh(RDBMesh_t rdbMesh, Scene scene, Node node, InfoObject infoObject, Dictionary<int, Material> mats)
        {
            node.Transform.Decompose(out AVector3 scale, out AQuaternion rotation, out AVector3 translation);

            RTriMesh_t triMesh = new RTriMesh_t()
            {
                prio = 3,
                local_pos = translation.ToAODB(),
                local_rot = rotation.ToAODB(),
            };

            rdbMesh.Members.Add(triMesh);

            FAFTriMeshData_t triMeshData = new FAFTriMeshData_t()
            {
                name = node.Name,
            };

            rdbMesh.Members.Add(triMeshData);


            BVolume_t bVolume = new BVolume_t()
            {
                max_pos = new Vector3(0.5f, 1.5f, 0.5f),
                min_pos = new Vector3(-0.5f, -0.5f, -0.5f),
                sph_pos = new Vector3(0, 0, 0),
                sph_radius = 1,
            };

            rdbMesh.Members.Add(bVolume);

            triMesh.data = rdbMesh.Members.IndexOf(triMeshData);
            triMeshData.bvol = rdbMesh.Members.IndexOf(bVolume);

            return triMesh;
        }

        private static List<SimpleMesh> AddMesh(RDBMesh_t rdbMesh, Scene scene, Node node, InfoObject infoObject, Dictionary<int, FAFMaterial_t> materialMap, Dictionary<int, Material> mats)
        {
            List<SimpleMesh> meshes = new List<SimpleMesh>();

            int i = 0;
            foreach (int meshIndex in node.MeshIndices)
            {
                Mesh mesh = scene.Meshes[meshIndex];

                TriList triList = new TriList()
                {
                    triangles = BuildTriangleArray(mesh)
                };

                rdbMesh.Members.Add(triList);

                if (!materialMap.TryGetValue(mesh.MaterialIndex, out FAFMaterial_t material) )
                {
                    material = AddMaterial(rdbMesh, scene.Materials[mesh.MaterialIndex], infoObject, mats);
                    materialMap.Add(mesh.MaterialIndex, material);
                }

                SimpleMesh simpleMesh = new SimpleMesh()
                {
                    name = "",
                    vb_desc = BuildVertexDescriptor(mesh.Vertices.Count),
                    vertices = BuildVertexArray(mesh),
                    material = rdbMesh.Members.IndexOf(material),
                    trilist = rdbMesh.Members.IndexOf(triList)
                };

                rdbMesh.Members.Add(simpleMesh);

                meshes.Add(simpleMesh);
            }

            return meshes;
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
