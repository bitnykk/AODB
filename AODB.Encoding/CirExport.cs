using Assimp;
using System.Collections.Generic;
using System.Linq;
using AVector3 = Assimp.Vector3D;
using AQuaternion = Assimp.Quaternion;
using AODB.Common.RDBObjects;
using Mesh = Assimp.Mesh;
using Material = Assimp.Material;
using AODB.Common.Structs;

namespace AODB.Encoding
{
    public class AnimData
    {
        public string Name;
        public CATAnim CatAnim;
    }

    internal class CirExport
    {
        private RDBCatMesh _catMesh;
        private List<AnimData> _catAnim;
        private Scene _scene;
        private Dictionary<RDBCatMesh.Material, int> _matMap;
        private List<List<RDBCatMesh.Vertex>> _vertexPerMesh;
        private List<int> _textureIds = new List<int>();
        private Node[] _boneNodes;

        public CirExport(RDBCatMesh rdbMesh, List<AnimData> noAnim = null)
        {
            _catAnim = noAnim;
            _catMesh = rdbMesh;
            _matMap = new Dictionary<RDBCatMesh.Material, int>();
            _vertexPerMesh = new List<List<RDBCatMesh.Vertex>>();
        }

        public Scene CreateScene()
        {
            _scene = new Scene();
            _scene.RootNode = new Node("Root");
            BuildMeshes(_scene.RootNode);

            Node oldRoot = _scene.RootNode;
            _scene.RootNode = new Node();
            _scene.RootNode.Children.Add(oldRoot);

            return _scene;
        }

        private void BuildMeshes(Node groupNode)
        {
            foreach (RDBCatMesh.Material mat in _catMesh.Materials)
            {
                BuildMaterial(mat, _catMesh.Textures.FirstOrDefault(x => x.Name == mat.Name));
            }

            SetupSkeletonNodes(groupNode);
            SetupBones(out Bone[] bones);


            int i = 0;

            foreach (RDBCatMesh.MeshGroup meshGroup in _catMesh.MeshGroups)
            {
                Node meshGroupNode = new Node($"{meshGroup.Name}");

                foreach (RDBCatMesh.Mesh cMesh in meshGroup.Meshes)
                {
                    var meshName = $"{meshGroupNode.Name}_{i++}";
                    Node node = new Node(meshName);
                    Mesh mesh = new Mesh(meshName);

                    mesh.Vertices.AddRange(cMesh.Vertices.Select(x => _boneNodes != null ? GetVertexSkeletonPos(x).ToAssimp() : x.Position.ToAssimp()));
                    mesh.Normals.AddRange(cMesh.Vertices.Select(x => x.Normal.ToAssimp()));

                    mesh.TextureCoordinateChannels[0].AddRange(cMesh.Vertices.Select(x => new AVector3(x.Uvs.X, x.Uvs.Y, 0)));
                    mesh.UVComponentCount[0] = 2;
                    mesh.SetIndices(cMesh.Triangles, 3);
                    mesh.MaterialIndex = cMesh.MaterialId;

                    AQuaternion rot = new AQuaternion(0, 0, 0, 0);
                    AVector3 pos = new AVector3(0, 0, 0);

                    node.Transform = new Matrix4x4(rot.GetMatrix()) * Matrix4x4.FromTranslation(pos);

                    if (bones != null)
                    {
                        BindBones(cMesh, mesh, bones);
                    }

                    _vertexPerMesh.Add(cMesh.Vertices);

                    _scene.Meshes.Add(mesh);
                    node.MeshIndices.Add(_scene.Meshes.IndexOf(mesh));
                    meshGroupNode.Children.Add(node);
                }

                if (bones != null)
                {
                    foreach (Mesh mesh in _scene.Meshes)
                    {
                        foreach (var bone in bones)
                        {
                            if (mesh.Bones.Any(x => x.Name == bone.Name))
                                continue;

                            mesh.Bones.Add(bone);
                        }
                    }
                }

                groupNode.Children.Add(meshGroupNode);
            }


            SetupAnimations();
        }

        private void SetupAnimations()
        {
            if (_catAnim == null || _catAnim.Count == 0)
                return;

            foreach (var catAnim in _catAnim)
            {
                var anim = new Animation();
                anim.Name = catAnim.Name;

                foreach (var boneData in catAnim.CatAnim.Animation.BoneData)
                {
                    var nodeChannel = new NodeAnimationChannel();

                    nodeChannel.PreState = AnimationBehaviour.Constant;
                    nodeChannel.PostState = AnimationBehaviour.Constant;
                    var boneId = boneData.BoneId;
                    nodeChannel.NodeName = $"Bone_{boneId}_{_catMesh.Joints[boneId].Name}";
                    var posKey = boneData.TranslationKeys.Select(x => new VectorKey { Time = x.Time / 1000, Value = x.Position.ToAssimp() });
                    var rotKey = boneData.RotationKeys.Select(x => new QuaternionKey { Time = x.Time / 1000, Value = x.Rotation.ToAssimp() }).ToList();

                    if (posKey.Count() != 0)
                    {
                        nodeChannel.PositionKeys.AddRange(posKey);
                    }

                    if (rotKey.Count() != 0)
                    {
                        nodeChannel.RotationKeys.AddRange(rotKey);
                    }

                    nodeChannel.ScalingKeys.Add(new VectorKey { Time = 0, Value = new Vector3D(1, 1, 1) });

                    anim.NodeAnimationChannels.Add(nodeChannel);
                }

                _scene.Animations.Add(anim);
            }
        }
        //public float QuaternionLength(Assimp.Quaternion quat)
        //{
        //    return (float)Math.Sqrt(quat.W * quat.W + quat.X * quat.X + quat.Y * quat.Y + quat.Z * quat.Z);
        //}
        //private void EnsureQuaternionContinuity(List<QuaternionKey> nodeAnim, string name)
        //{
        //    if (!name.Contains("Pelvis_ac") && !name.Contains("Bip01_ac"))
        //        return;
        //    Console.WriteLine($"{name} {nodeAnim.Count}");

        //    if (nodeAnim.Count < 2)
        //        return;


        //    for (int i = 1; i < nodeAnim.Count; i++)
        //    {
        //        float dot = Dot(nodeAnim[i-1].Value, nodeAnim[i].Value);

        //        if (dot < -0.99f)
        //        {
        //            nodeAnim[i] = new QuaternionKey
        //            {
        //                Time = nodeAnim[i].Time,
        //                Value = nodeAnim[i-1].Value,
        //            };
        //        }
        //        Console.WriteLine($"{dot} {nodeAnim[i].Value}");
        //    }
        //}

        //public static float Dot(Assimp.Quaternion a, Assimp.Quaternion b)
        //{
        //    return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        //}

        private Vector3 GetVertexSkeletonPos(RDBCatMesh.Vertex vertex)
        {
            Vector3 relToPos1 = new Vector3(vertex.RelToJoint1.X, vertex.RelToJoint1.Y, vertex.RelToJoint1.Z);
            Vector3 relToPos2 = new Vector3(vertex.RelToJoint2.X, vertex.RelToJoint2.Y, vertex.RelToJoint2.Z);

            GetGlobalTransform(_boneNodes[vertex.Joint2]).Decompose(out _, out var b2Rot, out var b2Pos);
            GetGlobalTransform(_boneNodes[vertex.Joint1]).Decompose(out _, out var b1Rot, out var b1Pos);
            
            var boneJoint1Pos = new Vector3(b1Pos.X, b1Pos.Y, b1Pos.Z);
            var boneJoint1Rot = new Common.Structs.Quaternion(b1Rot.X, b1Rot.Y, b1Rot.Z, b1Rot.W);
            var boneJoint2Pos = new Vector3(b2Pos.X, b2Pos.Y, b2Pos.Z);
            var boneJoint2Rot = new Common.Structs.Quaternion(b2Rot.X, b2Rot.Y, b2Rot.Z, b2Rot.W);
            var finalPos = Vector3.Lerp(boneJoint2Pos + boneJoint2Rot * relToPos2, boneJoint1Pos + boneJoint1Rot * relToPos1, vertex.Joint1Weight);

            return finalPos;
        }

        private void BindBones(RDBCatMesh.Mesh cMesh, Mesh mesh, Bone[] bones)
        {
            for (int i = 0; i < cMesh.Vertices.Count; i++)
            {
                var vertex = cMesh.Vertices[i];
                UpdateBone(mesh, bones[vertex.Joint1].Name, vertex.Joint1Weight, i);
                UpdateBone(mesh, bones[vertex.Joint2].Name, 1 - vertex.Joint1Weight, i);
            }
        }

        private Matrix4x4 GetGlobalTransform(Node node)
        {
            Matrix4x4 transform = node.Transform;
            while (node.Parent != null)
            {
                node = node.Parent;
                transform = transform * node.Transform;
            }
            return transform;
        }

        private void UpdateBone(Mesh mesh, string boneName, float weight, int index)
        {
            var bone = mesh.Bones.FirstOrDefault(x => x.Name == boneName);

            if (bone == null)
            {
                Bone newBone = new Bone
                {
                    Name = boneName,
                    OffsetMatrix = GetBindPose(_boneNodes.FirstOrDefault(x=>x.Name == boneName))
                };

                newBone.VertexWeights.Add(new VertexWeight
                {
                    Weight = weight,
                    VertexID = index,
                });

                mesh.Bones.Add(newBone);
            }
            else
            {
                bone.VertexWeights.Add(new VertexWeight
                {
                    Weight = weight,
                    VertexID = index
                });
            }
        }


        private void SetupSkeletonNodes(Node root)
        {
            if (_catAnim == null)
                return;

            var poseAnim = _catAnim.FirstOrDefault();

            _boneNodes = new Node[poseAnim.CatAnim.Animation.BoneData.Count];

            for (int i = 0; i < poseAnim.CatAnim.BoneCount; i++)
            {
                var bone = $"Bone_{i}_{_catMesh.Joints[i].Name}";
                _boneNodes[i] = new Node(bone);

                if (poseAnim.CatAnim.Animation.BoneData.Any(x => x.BoneId == i))
                {
                    var boneData = poseAnim.CatAnim.Animation.BoneData.FirstOrDefault(x => x.BoneId == i);
                    var rot = boneData.RotationKeys[0].Rotation;
                    var pos = boneData.TranslationKeys[0].Position;
                    _boneNodes[i].Transform = new Matrix4x4(rot.ToAssimp().GetMatrix()) * Matrix4x4.FromTranslation(pos.ToAssimp());

                    QuaternionKey quatKey = new QuaternionKey
                    {
                        Time = boneData.RotationKeys[0].Time,
                        Value = boneData.RotationKeys[0].Rotation.ToAssimp()
                    };
                    
                    VectorKey transKey = new VectorKey
                    {
                        Time = boneData.TranslationKeys[0].Time,
                        Value = boneData.TranslationKeys[0].Position.ToAssimp()
                    };
                }
            }

            for (int i = 0; i < _catMesh.Joints.Count; i++)
            {
                var currentJoint = _catMesh.Joints[i];

                if (currentJoint.ChildJoints.Length == 0)
                    continue;

                for (int j = 0; j < currentJoint.ChildJoints.Length; j++)
                {
                    _boneNodes[i].Children.Add(_boneNodes[currentJoint.ChildJoints[j]]);
                }
            }

            foreach (var b in _boneNodes.Except(_boneNodes.SelectMany(x => x.Children)))
                root.Children.Add(b);
        }

        private void SetupBones(out Bone[] bones)
        {
            bones = null;

            if (_catAnim == null)
            {
                return;
            }

            bones = new Bone[_boneNodes.Length];

            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = new Bone
                {
                    Name = $"Bone_{i}_{_catMesh.Joints[i].Name}",
                    OffsetMatrix = GetBindPose(_boneNodes[i])
                };
            }
        }

        private Matrix4x4 GetBindPose(Node node)
        {
            var bonePose = GetGlobalTransform(node);
            bonePose.Inverse();
            return node.Transform;
        }

        private int BuildMaterial(RDBCatMesh.Material catMaterial, RDBCatMesh.Texture catTexture)
        {
            Material mat = new Material();
            mat.Name = catMaterial.Name;
            mat.ColorDiffuse = catMaterial.Diffuse.ToAssimp();
            mat.ColorSpecular = catMaterial.Specular.ToAssimp();
            mat.ColorAmbient = catMaterial.Ambient.ToAssimp();
            mat.ColorEmissive = catMaterial.Emission.ToAssimp();
            mat.Shininess = catMaterial.Sheen;
            mat.Opacity = catMaterial.SheenOpacity;

            if (catTexture != null)
            {
                if (catMaterial.Unknown2 == 5)
                {
                    mat.AddProperty(new MaterialProperty("ApplyAlpha", true));

                }

                if (catTexture.Texture1 != 0)
                {
                    mat.AddProperty(new MaterialProperty("DiffuseId", catTexture.Texture1));
                    _textureIds.Add(catTexture.Texture1);
                }

                if (catTexture.Texture2 != 0)
                {
                    mat.AddProperty(new MaterialProperty("EmissionId", catTexture.Texture2));
                    _textureIds.Add(catTexture.Texture2);
                }
            }

            int matIdx = _scene.MaterialCount;
            _scene.Materials.Add(mat);
            _matMap.Add(catMaterial, _matMap.Count);
            return matIdx;
        }
    }
}
