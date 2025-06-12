using Assimp;
using System.Collections.Generic;
using AODB.Common.RDBObjects;

namespace AODB.Encoding
{
    public class CirConvert
    {
        public static Scene ToAssimpScene(RDBCatMesh castMesh)
        {
            CirExport exporter = new CirExport(castMesh, null);
            return exporter.CreateScene();
        }

        public static Scene ToAssimpScene(RDBCatMesh castMesh, List<AnimData> animData)
        {
            CirExport exporter = new CirExport(castMesh, animData);
            return exporter.CreateScene();
        }
    }
}
