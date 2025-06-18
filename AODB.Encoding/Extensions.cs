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
            deltaState.rst_type = deltaState.rst_type.Append((uint)renderStateType).ToArray();
            deltaState.rst_value = deltaState.rst_value.Append(value).ToArray();
            deltaState.rst_count++;
        }

        public static void AddChild(this Transform transform, int idx)
        {
            transform.chld_cnt++;
            transform.chld = transform.chld.Append(idx).ToArray();
        }

        public static void AddMesh(this FAFTriMeshData_t triMeshData, int idx)
        {
            triMeshData.num_meshes++;
            triMeshData.mesh = triMeshData.mesh.Append(idx).ToArray();
        }
    }
}
