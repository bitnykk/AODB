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
            if (args.Length == 0)
            {
                Console.WriteLine("Command use : Test \"X:Your\\Ao\\Path\" [optional:Type]");
            }
            else
            {
                int ICount = 0;
                string aoPath = args[0];
                Directory.SetCurrentDirectory(aoPath);
                _rdbController = new RdbController(aoPath);
                var types = _rdbController.Get<InfoObject>(1).Types;
                if (args.Length == 1)
                {
                    Console.WriteLine("Available TYPES :");                    
                    foreach (var type in types)
                    {
                        ICount = ICount + 1;
                        Console.WriteLine(type);
                    }
                }
                else
                {
                    string CatType = args[1];
                    var CatList = new ResourceTypeId[0];
                    switch (CatType)
                    {
                        case "RdbMesh": // 1010001
                            CatList = new ResourceTypeId[] { ResourceTypeId.RdbMesh };
                            break;
                        case "CatMesh": // 1010002
                            CatList = new ResourceTypeId[] { ResourceTypeId.CatMesh };
                            break;
                        case "Anim": // 1010003
                            CatList = new ResourceTypeId[] { ResourceTypeId.Anim };
                            break;
                        case "Texture": // 1010004
                            CatList = new ResourceTypeId[] { ResourceTypeId.Texture };
                            break;
                        case "Icon": // 1010008
                            CatList = new ResourceTypeId[] { ResourceTypeId.Icon };
                            break;
                        case "SkinTexture": // 1010011
                            CatList = new ResourceTypeId[] { ResourceTypeId.SkinTexture };
                            break;
                        default: // 00000000
                            Console.WriteLine("Error : wrong type ...");
                            break;
                    }                    
                    foreach (var resource in CatList)
                    {
                        Console.WriteLine("Stored "+ CatType + " :");
                        foreach (var rdbMeshKey in _rdbController.RecordTypeToId[(int)resource].Keys)
                        {
                            ICount = ICount + 1;
                            if (!types[resource].TryGetValue(rdbMeshKey, out string name))
                                Console.WriteLine(rdbMeshKey+" : Unamed");
                            else
                                Console.WriteLine(rdbMeshKey+" : "+name);
                        }
                    }
                }
                _rdbController.Dispose();
                Console.WriteLine(ICount+" item(s) listed");
                Console.Read();
            }
        }
    }
}
