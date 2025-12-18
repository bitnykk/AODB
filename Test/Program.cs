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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static AODB.Common.DbClasses.RDBMesh_t;
using static AODB.Common.RDBObjects.RDBCatMesh;

namespace Test
{
    internal class Program
    {
        public static RdbController _rdbController;
        public static Dictionary<ResourceTypeId, Dictionary<int, string>> RDBNames;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage : Test \"X:\\AO\\Path\" [Type] [Optional:ID]");
                return;
            }

            string aoPath = args[0];
            Directory.SetCurrentDirectory(aoPath);

            _rdbController = new RdbController(aoPath);
            var info = _rdbController.Get<InfoObject>(1);
            var types = info.Types;

            // --- 1: pure list ---
            if (args.Length == 1)
            {
                Console.WriteLine("Available TYPES :");
                foreach (var type in types)
                    Console.WriteLine(type.Key);
                goto END;
            }

            string catType = args[1];
            ResourceTypeId resourceType;

            switch (catType)
            {
                case "RdbMesh":
                    resourceType = ResourceTypeId.RdbMesh;
                    break;
                case "CatMesh":
                    resourceType = ResourceTypeId.CatMesh;
                    break;
                case "Anim":
                    resourceType = ResourceTypeId.Anim;
                    break;
                case "Texture":
                    resourceType = ResourceTypeId.Texture;
                    break;
                default:
                    Console.WriteLine("Unknown type.");
                    goto END;
            }

            // --- 2 : extraction ABIFF by ID ---
            if (args.Length >= 3)
            {
                int id = int.Parse(args[2]);

                Console.WriteLine($"Extracting ABIFF from RdbMesh ID={id}");

                Directory.CreateDirectory("export");

                // official API
                RDBMesh catMesh = _rdbController.Get<RDBMesh>(id);

                if (catMesh == null)
                {
                    Console.WriteLine("Mesh not found.");
                    goto END;
                }

                string outPath = Path.Combine("export", $"{id}.abiff");

                // official rewrite
                byte[] abiffData = catMesh.Serialize();
                File.WriteAllBytes(outPath, abiffData);

                Console.WriteLine($"ABIFF exported to {outPath}");
                goto END;
            }

            // --- 3 : list all IDs ---
            Console.WriteLine($"Stored {catType} :");
            foreach (var key in _rdbController.RecordTypeToId[(int)resourceType].Keys)
            {
                if (!types[resourceType].TryGetValue(key, out string name))
                    Console.WriteLine($"{key} : Unnamed");
                else
                    Console.WriteLine($"{key} : {name}");
            }

        END:
            _rdbController.Dispose();
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
