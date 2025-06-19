using AODB.Common.DbClasses;
using AODB.Common.Structs;
using Assimp;
using Assimp.Unmanaged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static AODB.Common.DbClasses.RDBMesh_t;
using static AODB.Common.DbClasses.RDBMesh_t.FAFAnim_t;
using AQuaternion = Assimp.Quaternion;
using AVector3 = Assimp.Vector3D;
using Quaternion = AODB.Common.Structs.Quaternion;

namespace AODB.Encoding
{
    internal class AbiffExporter
    {
        private RDBMesh_t _rdbMesh;
        private Scene _scene = null;

        private Dictionary<FAFMaterial_t, int> _matMap = new Dictionary<FAFMaterial_t, int>();
        private Dictionary<int, UVKey[]> _uvKeys = new Dictionary<int, UVKey[]>();
        public AbiffExporter(RDBMesh_t rdbMesh)
        {
            _rdbMesh = rdbMesh;
        }

        public Scene CreateScene(out Dictionary<int, UVKey[]> uvKeys)
        {
            _scene = new Scene();
            BuildSceneObjects();

            if(!_scene.HasMaterials)
                _scene.Materials.Add(new Material());

            Node oldRoot = _scene.RootNode;
            _scene.RootNode = new Node("Root");
            _scene.RootNode.Children.Add(oldRoot);

            uvKeys = _uvKeys;

            return _scene;
        }

        private void BuildSceneObjects()
        {
            Dictionary<int, Node> sceneObjects = new Dictionary<int, Node>();

            for (int i = 0; i < _rdbMesh.Members.Count; i++)
            {
                Node sceneObject;

                switch (_rdbMesh.Members[i])
                {
                    case RTriMesh_t triMeshClass:
                        sceneObject = BuildTriMesh(triMeshClass);
                        break;
                    case RRefFrame_t refFrameClass:
                        sceneObject = BuildRefFrame(refFrameClass);
                        break;
                    default:
                        continue;
                }

                if (_rdbMesh.Members[i] is Transform transform && transform.conn != -1)
                    sceneObject.Name = ((RRefFrameConnector)_rdbMesh.Members[transform.conn]).name;

                sceneObjects.Add(i, sceneObject);
            }

            // Fix inheritance
            List<Transform> transforms = _rdbMesh.GetMembers<Transform>();

            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].chld_cnt == 0)
                    continue;

                foreach (int childIdx in transforms[i].chld)
                {
                    if (sceneObjects.TryGetValue(childIdx, out Node node))
                    {
                        if (sceneObjects.TryGetValue(i, out var parent))
                        {
                            //node.Transform = parent.Transform * node.Transform;
                            parent.Children.Add(node);
                        }
                    }
                }
            }

            _scene.RootNode = sceneObjects.Values.First();
        }

        private Node BuildRefFrame(RRefFrame_t refFrameClass)
        {
            Node refFrame = new Node("RRefFrame");

            AVector3 scale = new AVector3(refFrameClass.scale, refFrameClass.scale, refFrameClass.scale);
            AQuaternion rotation = refFrameClass.local_rot.ToAssimp();
            AVector3 position = refFrameClass.local_pos.ToAssimp();

            if (refFrameClass.anim_matrix.values != null)
            {
                refFrame.Transform = refFrameClass.anim_matrix.Transpose().ToAssimpMatrix() * ((Transform)_rdbMesh.Members[0]).anim_matrix.Transpose().ToAssimpMatrix();
            }

            return refFrame;
        }

        private Node BuildTriMesh(RTriMesh_t triMeshClass)
        {
            FAFTriMeshData_t triMeshDataClass = _rdbMesh.Members[triMeshClass.data] as FAFTriMeshData_t;

            Node node = new Node(triMeshDataClass.name);

            var hasAnims = BuildFAFAnim(node, triMeshDataClass.anim_pos, triMeshDataClass.anim_rot, triMeshClass);

            BuildMeshes(node, triMeshClass, triMeshDataClass, hasAnims, out List<int> sceneMeshesIdx);
            BuildUVKeys(sceneMeshesIdx, triMeshClass);

            AVector3 scale = new AVector3(triMeshClass.scale, triMeshClass.scale, triMeshClass.scale);
            AQuaternion rotation = triMeshClass.local_rot.ToAssimp();
            AVector3 position = triMeshClass.local_pos.ToAssimp();

            node.Transform = Matrix4x4.FromTranslation(position) * new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromScaling(scale);
            //node.Transform = triMeshClass.anim_matrix.Transpose().ToAssimpMatrix();
            return node;
        }

        private bool BuildFAFAnim(Node node, Vector3 animPos, Quaternion animRot, RTriMesh_t triMeshClass)
        {
            if (triMeshClass.anim <= 0)
                return false;

            FAFAnim_t animClass = _rdbMesh.Members[triMeshClass.anim] as FAFAnim_t;

            if (animClass.num_trans_keys > 1 || animClass.num_rot_keys > 1) // For some reason some meshes have 1 keyframe animation data, so we just skip over those
            {
                Animation animation = new Animation();
                var nodeAnimationChannel = new NodeAnimationChannel();
                nodeAnimationChannel.NodeName = node.Name;

                var quartKeys = animClass.RotKeys.Select(x => new QuaternionKey { Time = x.Time, Value = x.Rotation.ToAssimp() });
                var vecKeys = animClass.TransKeys.Select(x => new VectorKey { Time = x.Time, Value = x.Translation.ToAssimp() });

                if (quartKeys.Count() != 0)
                {
                    nodeAnimationChannel.RotationKeys.AddRange(quartKeys);
                }

                if (vecKeys.Count() != 0)
                {
                    nodeAnimationChannel.PositionKeys.AddRange(vecKeys);
                }

                nodeAnimationChannel.ScalingKeys.Add(new VectorKey { Time = 0, Value = new AVector3(1, 1, 1) });
                animation.NodeAnimationChannels.Add(nodeAnimationChannel);
                _scene.Animations.Add(animation);

                return true;
            }

            return false;
        }


        private void BuildUVKeys(List<int> sceneMeshesIdx, RTriMesh_t triMeshClass)
        {
            if (sceneMeshesIdx.Count() == 0)
                return;

            if (triMeshClass.anim <= 0)
                return;

            FAFAnim_t animClass = _rdbMesh.Members[triMeshClass.anim] as FAFAnim_t;

            if (animClass.num_uv_keys <= 0)
                return;

            foreach (var sceneMeshIdx in sceneMeshesIdx)
                _uvKeys.Add(sceneMeshIdx, animClass.UVKeys);
        }

        private void BuildMeshes(Node node, RTriMesh_t triMeshClass, FAFTriMeshData_t triMeshDataClass, bool hasAnims, out List<int> sceneMeshesIdx)
        {
            sceneMeshesIdx = new List<int>();

            foreach (int meshIdx in triMeshDataClass.mesh)
            {
                SimpleMesh simpleMeshClass = _rdbMesh.Members[meshIdx] as SimpleMesh;
                TriList triListClass = _rdbMesh.Members[simpleMeshClass.trilist] as TriList;

                Mesh mesh = new Mesh($"{simpleMeshClass.name}_{meshIdx}");

                Matrix4x4 matrix;
                if (!hasAnims)
                {
                    AQuaternion rot = triMeshDataClass.anim_rot.ToAssimp();
                    AVector3 pos = triMeshDataClass.anim_pos.ToAssimp();

                    var rotationMatrix = new Matrix4x4(rot.GetMatrix());
                    var translationMatrix = Matrix4x4.FromTranslation(pos);

                    matrix = rotationMatrix * translationMatrix;
                }
                else
                {
                    matrix = Matrix4x4.Identity;
                }

                mesh.Vertices.AddRange(simpleMeshClass.Vertices.Select(x => matrix * x.Position.ToAssimp()));
                mesh.Normals.AddRange(simpleMeshClass.Vertices.Select(x => x.Normal.ToAssimp()));
                mesh.TextureCoordinateChannels[0].AddRange(simpleMeshClass.Vertices.Select(x => new AVector3(x.UVs.X, 1f-x.UVs.Y, 0)));
                mesh.UVComponentCount[0] = 2;
                mesh.SetIndices(triListClass.Triangles, 3);

                if (simpleMeshClass.material >= 0)
                {
                    FAFMaterial_t materialClass = _rdbMesh.Members[simpleMeshClass.material] as FAFMaterial_t;
                    mesh.MaterialIndex = BuildMaterial(materialClass);
                }

                _scene.Meshes.Add(mesh);

                var sceneMeshIdx = _scene.Meshes.IndexOf(mesh);

                sceneMeshesIdx.Add(sceneMeshIdx);
                node.MeshIndices.Add(sceneMeshIdx);
            }
        }

        private int BuildMaterial(FAFMaterial_t materialClass)
        {
            if (_matMap.TryGetValue(materialClass, out int idx))
                return idx;

            Material mat = new Material();
            mat.Name = materialClass.name;
            mat.ColorDiffuse = materialClass.diff.ToAssimp();
            mat.ColorSpecular = materialClass.spec.ToAssimp();
            mat.ColorAmbient = materialClass.ambi.ToAssimp();
            mat.ColorEmissive = materialClass.emis.ToAssimp();
            mat.Shininess = materialClass.shin;
            mat.Opacity = materialClass.opac;

            if (materialClass.delta_state >= 0)
            {
                RDeltaState deltaStateClass = _rdbMesh.Members[materialClass.delta_state] as RDeltaState;;

                for (int i = 0; i < deltaStateClass.rst_count; i++)
                {
                    switch((D3DRenderStateType)deltaStateClass.rst_type[i])
                    {
                        case D3DRenderStateType.D3DRS_CULLMODE: //TwoSided
                            mat.IsTwoSided = true;
                            break;
                        case D3DRenderStateType.D3DRS_ALPHABLENDENABLE: //Apply Alpha
                            mat.AddProperty(new MaterialProperty("ApplyAlpha", deltaStateClass.rst_value[i] == 1));
                            break;
                        case D3DRenderStateType.D3DRS_SPECULARENABLE:
                            mat.ColorSpecular = new Color4D(); //Disable specular
                            break;
                        default:
                            Console.WriteLine($"Unhandled render state - {(D3DRenderStateType)deltaStateClass.rst_type[i]}: {deltaStateClass.rst_value[i]}");
                            break;
                    }
                }

                for (int i = 0; i < deltaStateClass.tch_count; i++)
                {
                    FAFTexture_t textureClass = _rdbMesh.Members[deltaStateClass.tch_text[i]] as FAFTexture_t;

                    if (_rdbMesh.Members[textureClass.creator] is AnarchyTexCreator_t texCreatorClass)
                    {
                        if ((TextureChannelType)deltaStateClass.tch_type[i] == TextureChannelType.Diffuse)
                            mat.AddProperty(new MaterialProperty("DiffuseId", (int)texCreatorClass.inst));
                        else if ((TextureChannelType)deltaStateClass.tch_type[i] == TextureChannelType.Emissive)
                            mat.AddProperty(new MaterialProperty("EmissionId", (int)texCreatorClass.inst));
                        else
                            Console.WriteLine($"Unhandled texture channel: {deltaStateClass.tch_type[i]}");
                    }
                }
            }

            int matIdx = _scene.MaterialCount;

            _matMap.Add(materialClass, matIdx);

            _scene.Materials.Add(mat);

            return matIdx;
        }
    }
}
