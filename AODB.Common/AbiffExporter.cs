
using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AODB.Common.DbClasses;
using AODB.Common.Structs;
using Quaternion = AODB.Common.Structs.Quaternion;
using AVector3 = Assimp.Vector3D;
using AQuaternion = Assimp.Quaternion;
using static AODB.Common.DbClasses.RDBMesh_t.FAFAnim_t;

namespace AODB.Common
{
    internal class AbiffExporter
    {
        private RDBMesh_t _rdbMesh;
        private Scene _scene = null;

        private Dictionary<RDBMesh_t.FAFMaterial_t, int> _matMap = new Dictionary<RDBMesh_t.FAFMaterial_t, int>();
        private Dictionary<int, UVKey[]> _uvKeys = new Dictionary<int, UVKey[]>();

        public AbiffExporter(RDBMesh_t rdbMesh)
        {
            _rdbMesh = rdbMesh;
        }

        public Scene CreateScene(out Dictionary<int, UVKey[]> uvKeys)
        {
            _scene = new Scene();
            _scene.RootNode = new Node("Root");
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

            for(int i = 0; i < _rdbMesh.Members.Count; i++)
            {
                Node sceneObject;

                switch (_rdbMesh.Members[i])
                {
                    case RDBMesh_t.RTriMesh_t triMeshClass:
                        sceneObject = BuildTriMesh(triMeshClass);
                        break;
                    case RDBMesh_t.RRefFrame_t refFrameClass:
                        sceneObject = BuildRefFrame(refFrameClass);
                        break;
                    default:
                        continue;
                }

                sceneObjects.Add(i, sceneObject);
            }

            //Fix inheritance
            List<RDBMesh_t.Transform> transforms = _rdbMesh.GetMembers<RDBMesh_t.Transform>();

            for(int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].chld_cnt == 0)
                    continue;

                foreach (int childIdx in transforms[i].chld)
                {
                    if(sceneObjects.TryGetValue(childIdx, out Node node))
                        sceneObjects[i].Children.Add(node);
                }
            }
        }


        private Node BuildRefFrame(RDBMesh_t.RRefFrame_t refFrameClass)
        {
            Node refFrame = new Node("RRefFrame");

            AVector3 scale = new Vector3(refFrameClass.scale, refFrameClass.scale, refFrameClass.scale);
            AQuaternion rotation = refFrameClass.local_rot;
            AVector3 position = refFrameClass.local_pos;
            refFrame.Transform = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(position) * Matrix4x4.FromScaling(scale);

            return refFrame;
        }

        private Node BuildTriMesh(RDBMesh_t.RTriMesh_t triMeshClass)
        {
            Node node = new Node("TriMesh");

            _scene.RootNode.Children.Add(node);
            RDBMesh_t.FAFTriMeshData_t triMeshDataClass = _rdbMesh.Members[triMeshClass.data] as RDBMesh_t.FAFTriMeshData_t;
            BuildMeshes(node, triMeshClass, triMeshDataClass);

            AVector3 scale = new Vector3(triMeshClass.scale, triMeshClass.scale, triMeshClass.scale);
            AQuaternion rotation = triMeshClass.local_rot;
            AVector3 position = triMeshClass.local_pos;
            node.Transform = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(position) * Matrix4x4.FromScaling(scale);

            return node;
        }

        private void BuildMeshes(Node groupNode, RDBMesh_t.RTriMesh_t triMeshClass, RDBMesh_t.FAFTriMeshData_t triMeshDataClass)
        {
            foreach (int meshIdx in triMeshDataClass.mesh)
            {
                RDBMesh_t.SimpleMesh simpleMeshClass = _rdbMesh.Members[meshIdx] as RDBMesh_t.SimpleMesh;
                RDBMesh_t.TriList triListClass = _rdbMesh.Members[simpleMeshClass.trilist] as RDBMesh_t.TriList;

                Node node = new Node(simpleMeshClass.name);
                Mesh mesh = new Mesh(simpleMeshClass.name);

                mesh.Vertices.AddRange(simpleMeshClass.Vertices.Select(x => (AVector3)x.Position));
                mesh.Normals.AddRange(simpleMeshClass.Vertices.Select(x => (AVector3)x.Normals));
                mesh.TextureCoordinateChannels[0].AddRange(simpleMeshClass.Vertices.Select(x => new AVector3(x.UVs.X, 1f-x.UVs.Y, 0)));
                mesh.UVComponentCount[0] = 2;
                mesh.SetIndices(triListClass.Triangles, 3);

                if (simpleMeshClass.material >= 0)
                {
                    RDBMesh_t.FAFMaterial_t materialClass = _rdbMesh.Members[simpleMeshClass.material] as RDBMesh_t.FAFMaterial_t;
                    mesh.MaterialIndex = BuildMaterial(materialClass);
                }

                AQuaternion rotation = triMeshDataClass.anim_rot;
                AVector3 position = triMeshDataClass.anim_pos;
                node.Transform = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(position);

                _scene.Meshes.Add(mesh);

                int sceneMeshIdx = _scene.Meshes.IndexOf(mesh);
                node.MeshIndices.Add(sceneMeshIdx);

                groupNode.Children.Add(node);

                if(triMeshClass.anim > 0)
                {
                    RDBMesh_t.FAFAnim_t animClass = _rdbMesh.Members[triMeshClass.anim] as RDBMesh_t.FAFAnim_t;

                    if(animClass.num_uv_keys > 0)
                        _uvKeys.Add(sceneMeshIdx, animClass.UVKeys);
                }
            }
        }

        private int BuildMaterial(RDBMesh_t.FAFMaterial_t materialClass)
        {
            if (_matMap.TryGetValue(materialClass, out int idx))
                return idx;

            Material mat = new Material();
            mat.Name = materialClass.name;
            mat.ColorDiffuse = materialClass.diff;
            mat.ColorSpecular = materialClass.spec;
            mat.ColorAmbient = materialClass.ambi;
            mat.ColorEmissive = materialClass.emis;
            mat.Shininess = materialClass.shin;
            mat.Opacity = materialClass.opac;

            if (materialClass.delta_state >= 0)
            {
                RDBMesh_t.RDeltaState deltaStateClass = _rdbMesh.Members[materialClass.delta_state] as RDBMesh_t.RDeltaState;;

                for (int i = 0; i < deltaStateClass.rst_count; i++)
                {
                    switch(deltaStateClass.rst_type[i])
                    {
                        case 22: //TwoSided
                            mat.IsTwoSided = true;
                            break;
                        case 27: //Apply Alpha
                            mat.AddProperty(new MaterialProperty("ApplyAlpha", deltaStateClass.rst_value[i] == 1));
                            break;
                    }
                }

                for (int i = 0; i < deltaStateClass.tch_count; i++)
                {
                    RDBMesh_t.FAFTexture_t textureClass = _rdbMesh.Members[deltaStateClass.tch_text[i]] as RDBMesh_t.FAFTexture_t;

                    if (_rdbMesh.Members[textureClass.creator] is RDBMesh_t.AnarchyTexCreator_t texCreatorClass)
                    {
                        if (deltaStateClass.tch_type[i] == 0)
                            mat.AddProperty(new MaterialProperty("DiffuseId", (int)texCreatorClass.inst));
                        else if (deltaStateClass.tch_type[i] == 1)
                            mat.AddProperty(new MaterialProperty("EmissionId", (int)texCreatorClass.inst));
                    }
                }
            }

            int matIdx = _scene.MaterialCount;
            _scene.Materials.Add(mat);

            return matIdx;
        }
    }
}
