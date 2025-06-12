using Assimp;
using System.Collections.Generic;
using AODB.Common.RDBObjects;

namespace AODB.Encoding
{
    public class CirImporter
    {
        public static Scene ToAssimpScene(RDBCatMesh castMesh)
        {
            CirExporter exporter = new CirExporter(castMesh, null);
            return exporter.CreateScene();
        }

        public static Scene ToAssimpScene(RDBCatMesh castMesh, List<AnimData> animData)
        {
            CirExporter exporter = new CirExporter(castMesh, animData);
            return exporter.CreateScene();
        }
    }
}
