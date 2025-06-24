using AODB.Common.DbClasses;
using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AODB.Common.DbClasses.RDBMesh_t;

namespace AODB.Encoding
{
    public static class Extensions
    {
        public static void AddRenderStateType(this RDeltaState deltaState, D3DRenderStateType renderStateType, uint value)
        {

            if (deltaState.rst_type == null)
                deltaState.rst_type = new uint[1] { (uint)renderStateType };
            else
                deltaState.rst_type = deltaState.rst_type.Append((uint)renderStateType).ToArray();


            if (deltaState.rst_value == null)
                deltaState.rst_value = new uint[1] { value };
            else
                deltaState.rst_value = deltaState.rst_value.Append(value).ToArray();

            deltaState.rst_count++;
        }

        public static void AddChild(this Transform transform, int idx)
        {
            transform.chld_cnt++;

            if (transform.chld == null)
                transform.chld = new int[1] {idx};
            else
                transform.chld = transform.chld.Append(idx).ToArray();
        }

        public static void AddMesh(this FAFTriMeshData_t triMeshData, int idx)
        {
            triMeshData.num_meshes++;
            if (triMeshData.mesh == null)
                triMeshData.mesh = new int[1] { idx };
            else
                triMeshData.mesh = triMeshData.mesh.Append(idx).ToArray();
        }
    }
}
