using AODB;
using AODB.Common;
using AODB.Common.DbClasses;
using AODB.Common.RDBObjects;
using AODB.Common.Structs;
using AODB.Encoding;
using Assimp;
using Assimp.Unmanaged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        public static RdbController _rdbController;
        public static Dictionary<ResourceTypeId, Dictionary<int, string>> RDBNames;

        static void Main(string[] args)
        {
            string aoPath = "D:\\Anarchy Online";
            Directory.SetCurrentDirectory(aoPath);
            _rdbController = new RdbController(aoPath);

            int meshId = 283378;

            RDBNames = _rdbController.Get<InfoObject>(1).Types;

            ModelExporter.ExportAbiff(_rdbController, ModelExporter.RdbMeshType.RdbMesh, meshId, aoPath, out _);

            _rdbController.Dispose();

            Console.WriteLine("Done");
            Console.Read();
        }
    }
}
