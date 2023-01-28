using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AODB.Common.DbClasses;
using AODB.Common.Structs;
using Quaternion = AODB.Common.Structs.Quaternion;
using AVector3 = Assimp.Vector3D;
using AQuaternion = Assimp.Quaternion;

namespace AODB.Common
{
    public class AbiffConverter
    {
        #region Export

        public static Scene CreateScene(RDBMesh_t rdbMesh)
        {
            Scene scene = new Scene();
            scene.RootNode = BuildRoot(rdbMesh);
            BuildSceneObjects(scene, rdbMesh);

            scene.Materials.Add(new Material());

            Node oldRoot = scene.RootNode;
            scene.RootNode = new Node("Root");
            scene.RootNode.Children.Add(oldRoot);

            return scene;
        }

        private static Node BuildRoot(RDBMesh_t rdbMesh)
        {
            RDBMesh_t.RRefFrame_t refFrameClass = rdbMesh.GetMember<RDBMesh_t.RRefFrame_t>();

            if(refFrameClass == null)
            {
                return new Node("Root");
            }
            else
            {
                Node refFrame = new Node("RRefFrame");

                AVector3 scale = new Vector3(refFrameClass.scale, refFrameClass.scale, refFrameClass.scale);
                AQuaternion rotation = refFrameClass.local_rot;
                AVector3 position = refFrameClass.local_pos;
                refFrame.Transform = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(position) * Matrix4x4.FromScaling(scale);

                return refFrame;
            }
        }

        private static void BuildSceneObjects(Scene scene, RDBMesh_t rdbMesh)
        {
            Dictionary<int, Node> sceneObjects = new Dictionary<int, Node>();

            foreach(object member in rdbMesh.Members)
            {
                switch(member)
                {
                    case RDBMesh_t.RTriMesh_t triMeshClass:
                        BuildTriMesh(scene, rdbMesh, triMeshClass);
                        break;
                }
            }
        }

        private static void BuildTriMesh(Scene scene, RDBMesh_t rdbMesh, RDBMesh_t.RTriMesh_t triMeshClass)
        {
            Node node = new Node("TriMesh");

            AVector3 scale = new Vector3(triMeshClass.scale, triMeshClass.scale, triMeshClass.scale);
            AQuaternion rotation = triMeshClass.local_rot;
            AVector3 position = triMeshClass.local_pos;
            node.Transform = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(position) * Matrix4x4.FromScaling(scale);

            scene.RootNode.Children.Add(node);
            RDBMesh_t.FAFTriMeshData_t triMeshDataClass = rdbMesh.Members[triMeshClass.data] as RDBMesh_t.FAFTriMeshData_t;
            BuildMeshes(scene, rdbMesh, node, triMeshDataClass);
        }

        private static void BuildMeshes(Scene scene, RDBMesh_t rdbMesh, Node node, RDBMesh_t.FAFTriMeshData_t triMeshClass)
        {
            foreach(int meshIdx in triMeshClass.mesh)
            {
                RDBMesh_t.SimpleMesh simpleMeshClass = rdbMesh.Members[meshIdx] as RDBMesh_t.SimpleMesh;
                RDBMesh_t.TriList triListClass = rdbMesh.Members[simpleMeshClass.trilist] as RDBMesh_t.TriList;

                Mesh mesh = new Mesh(simpleMeshClass.name);
                mesh.Vertices.AddRange(simpleMeshClass.Vertices.Select(x => (AVector3)x.Position));
                mesh.Normals.AddRange(simpleMeshClass.Vertices.Select(x => (AVector3)x.Normals));
                mesh.TextureCoordinateChannels[0].AddRange(simpleMeshClass.Vertices.Select(x => new AVector3(x.UVs.X, -x.UVs.Y, 0)));
                mesh.SetIndices(triListClass.Triangles, 3);

                scene.Meshes.Add(mesh);
                node.MeshIndices.Add(scene.Meshes.IndexOf(mesh));
            }
        }

        #endregion Export

        public static RDBMesh_t LoadFromFBX(string fileName)
        {
            AssimpContext importer = new AssimpContext();

            Scene scene = importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality);

            RDBMesh_t rdbMesh = Create(scene);

            importer.Dispose();

            return rdbMesh;
        }

        private static RDBMesh_t Create(Scene scene)
        {
            RDBMesh_t rdbMesh = new RDBMesh_t();

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
                AddMesh(rdbMesh, mesh, scene.Materials);
            }

            List<RDBMesh_t.RTriMesh_t> triMeshes = rdbMesh.GetMembers<RDBMesh_t.RTriMesh_t>();
            refFrame.chld_cnt = triMeshes.Count;
            refFrame.chld = triMeshes.Select(x => rdbMesh.Members.IndexOf(x)).ToArray();

            return rdbMesh;
        }

        private static void AddMesh(RDBMesh_t rdbMesh, Mesh mesh, List<Material> materials)
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
                rst_type = new int[] { 29 },
                rst_value = new int[] { 1 },
                tch_count = 1,
                tch_type = new int[] { 0 },
                tstv_count = 0,
                tstm_count = new int[] { 0 },
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
                inst = (uint)Math.Floor(double.Parse(materials[mesh.MaterialIndex].Name))
            };

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
    }
}
