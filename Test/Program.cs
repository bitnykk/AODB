using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AODB;
using AODB.Common;
using AODB.Common.DbClasses;
using AODB.RDBObjects;
using Assimp;
using Assimp.Unmanaged;

namespace Test
{
    internal class Program
    {
        public static RdbController _rdbController;
        public static Dictionary<int, Dictionary<int, string>> RDBNames;

        static void Main(string[] args)
        {
            string aoPath = "D:\\Anarchy Online";
            Directory.SetCurrentDirectory(aoPath);
            _rdbController = new RdbController(aoPath);

            int meshId = 225720;
            //int meshId = 7826;

            RDBNames = _rdbController.Get<InfoObject>(1).Types;
            //Console.WriteLine($"{meshId} - {RDBNames[1010001][(int)meshId]}");
            //Console.WriteLine($"{RDBNames[1010001].Count}");

            //byte[] rawMesh = rdbController.GetRaw(1010001, 7798);
            //File.WriteAllBytes("7798.abiff", rawMesh);


            //AbiffConverter.LoadFromFBX($"C:\\Users\\tagyo\\Documents\\AOModelImport\\untitled.fbx");
            Console.WriteLine(ExportMesh(meshId) ? "Exported." : "Export Failed.");

            //List<string> vDesc = new List<string>();

            //foreach (int id in _rdbController.RecordTypeToId[(int)ResourceTypeId.RdbMesh].Keys)
            //{
            //    try
            //    {
            //        string name = RDBNames[1010001][id];
            //        Console.WriteLine($"Parsing Model: {RDBNames[1010001][id]} ({id})");
            //        RDBMesh mesh2 = _rdbController.Get<RDBMesh>(id);

            //        if (mesh2 == null)
            //            continue;

            //        if (mesh2.RDBMesh_t.GetMembers<RDBMesh_t.RTriMesh_t>().Any(x => x.delta_state > -1))
            //        {
            //            //var deltaState = mesh2.RDBMesh_t.GetMembers<RDBMesh_t.RDeltaState>().First(x => x.tch_count > 2);
            //            Console.WriteLine();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        byte[] rawMesh = _rdbController.GetRaw(1010001, id);
            //        File.WriteAllBytes($"{id}.abiff", rawMesh);
            //    }
            //}

            //File.WriteAllLines("vDesc.txt", vDesc);

            //rdbController.Get<AOTexture>(0x45D9);

            //Console.WriteLine(BitConverter.ToString(((RDBMesh_t.FAFAnim_t)mesh.RDBMesh_t.Members[13]).trans_keys).Replace("-", ""));

            //AODB.Structs.FbxIndexer.LoadFBX("C:\\Users\\tagyo\\Documents\\test.fbx", out _);

            //AbiffExporter exporter = 
            //Scene exportScene = AbiffConverter.CreateScene(mesh.RDBMesh_t);

            Console.WriteLine("Done");
            Console.Read();
        }

        public static bool ExportMesh(int meshId)
        {
            string exportPath = "C:\\Users\\tagyo\\Documents\\AOModelImport\\ExportTestTestExport.dae";

            RDBMesh mesh = _rdbController.Get<RDBMesh>(meshId);

            var anim = mesh.RDBMesh_t.Members[32] as RDBMesh_t.FAFAnim_t;

            Console.WriteLine(BitConverter.ToString(anim.vis_keys).Replace("-", ""));

            Scene scene = AbiffConverter.ToAssimpScene(mesh.RDBMesh_t, out _);

            foreach (Material mat in scene.Materials)
            {
                if (mat.HasNonTextureProperty("DiffuseId"))
                {
                    ExportTexture(exportPath, mat.GetNonTextureProperty("DiffuseId").GetIntegerValue(), out string diffuseName);

                    TextureSlot diffuse = new TextureSlot
                    {
                        FilePath = diffuseName,
                        TextureType = TextureType.Diffuse,
                        TextureIndex = 0
                    };

                    //if (mat.HasNonTextureProperty("ApplyAlpha"))
                    //    diffuse.Flags = mat.GetNonTextureProperty("ApplyAlpha").GetBooleanValue() ? (int)TextureFlags.UseAlpha : (int)TextureFlags.IgnoreAlpha;

                    AddMaterialTexture(mat, ref diffuse, false);
                }

                if (mat.HasNonTextureProperty("EmissionId"))
                {
                    ExportTexture(exportPath, mat.GetNonTextureProperty("EmissionId").GetIntegerValue(), out string emissionName);

                    TextureSlot emission = new TextureSlot
                    {
                        FilePath = emissionName,
                        TextureType = TextureType.Emissive,
                        TextureIndex = 0,
                    };

                    AddMaterialTexture(mat, ref emission, false);
                }

            }

           return new AssimpContext().ExportFile(scene, exportPath, "collada");
        }



        public static bool AddMaterialTexture(Material mat, ref TextureSlot texture, bool onlySetFilePath)
        {
            if (string.IsNullOrEmpty(texture.FilePath))
                return false;

            TextureType texType = texture.TextureType;
            int texIndex = texture.TextureIndex;

            string texName = Material.CreateFullyQualifiedName(AiMatKeys.TEXTURE_BASE, texType, texIndex);

            MaterialProperty texNameProp = mat.GetProperty(texName);

            if (texNameProp == null)
                mat.AddProperty(new MaterialProperty(AiMatKeys.TEXTURE_BASE, texture.FilePath, texType, texIndex));
            else
                texNameProp.SetStringValue(texture.FilePath);

            if (onlySetFilePath)
                return true;

            string mappingName = Material.CreateFullyQualifiedName(AiMatKeys.MAPPING_BASE, texType, texIndex);
            string uvIndexName = Material.CreateFullyQualifiedName(AiMatKeys.UVWSRC_BASE, texType, texIndex);
            string blendFactorName = Material.CreateFullyQualifiedName(AiMatKeys.TEXBLEND_BASE, texType, texIndex);
            string texOpName = Material.CreateFullyQualifiedName(AiMatKeys.TEXOP_BASE, texType, texIndex);
            string uMapModeName = Material.CreateFullyQualifiedName(AiMatKeys.MAPPINGMODE_U_BASE, texType, texIndex);
            string vMapModeName = Material.CreateFullyQualifiedName(AiMatKeys.MAPPINGMODE_V_BASE, texType, texIndex);
            string texFlagsName = Material.CreateFullyQualifiedName(AiMatKeys.TEXFLAGS_BASE, texType, texIndex);

            MaterialProperty mappingNameProp = mat.GetProperty(mappingName);
            MaterialProperty uvIndexNameProp = mat.GetProperty(uvIndexName);
            MaterialProperty blendFactorNameProp = mat.GetProperty(blendFactorName);
            MaterialProperty texOpNameProp = mat.GetProperty(texOpName);
            MaterialProperty uMapModeNameProp = mat.GetProperty(uMapModeName);
            MaterialProperty vMapModeNameProp = mat.GetProperty(vMapModeName);
            MaterialProperty texFlagsNameProp = mat.GetProperty(texFlagsName);

            if (mappingNameProp == null)
            {
                mappingNameProp = new MaterialProperty(AiMatKeys.MAPPING_BASE, (int)texture.Mapping);
                mappingNameProp.TextureIndex = texIndex;
                mappingNameProp.TextureType = texType;
                mat.AddProperty(mappingNameProp);
            }
            else
            {
                mappingNameProp.SetIntegerValue((int)texture.Mapping);
            }

            if (uvIndexNameProp == null)
            {
                uvIndexNameProp = new MaterialProperty(AiMatKeys.UVWSRC_BASE, texture.UVIndex);
                uvIndexNameProp.TextureIndex = texIndex;
                uvIndexNameProp.TextureType = texType;
                mat.AddProperty(uvIndexNameProp);
            }
            else
            {
                uvIndexNameProp.SetIntegerValue(texture.UVIndex);
            }

            if (blendFactorNameProp == null)
            {
                blendFactorNameProp = new MaterialProperty(AiMatKeys.TEXBLEND_BASE, texture.BlendFactor);
                blendFactorNameProp.TextureIndex = texIndex;
                blendFactorNameProp.TextureType = texType;
                mat.AddProperty(blendFactorNameProp);
            }
            else
            {
                blendFactorNameProp.SetFloatValue(texture.BlendFactor);
            }

            if (texOpNameProp == null)
            {
                texOpNameProp = new MaterialProperty(AiMatKeys.TEXOP_BASE, (int)texture.Operation);
                texOpNameProp.TextureIndex = texIndex;
                texOpNameProp.TextureType = texType;
                mat.AddProperty(texOpNameProp);
            }
            else
            {
                texOpNameProp.SetIntegerValue((int)texture.Operation);
            }

            if (uMapModeNameProp == null)
            {
                uMapModeNameProp = new MaterialProperty(AiMatKeys.MAPPINGMODE_U_BASE, (int)texture.WrapModeU);
                uMapModeNameProp.TextureIndex = texIndex;
                uMapModeNameProp.TextureType = texType;
                mat.AddProperty(uMapModeNameProp);
            }
            else
            {
                uMapModeNameProp.SetIntegerValue((int)texture.WrapModeU);
            }

            if (vMapModeNameProp == null)
            {
                vMapModeNameProp = new MaterialProperty(AiMatKeys.MAPPINGMODE_V_BASE, (int)texture.WrapModeV);
                vMapModeNameProp.TextureIndex = texIndex;
                vMapModeNameProp.TextureType = texType;
                mat.AddProperty(vMapModeNameProp);
            }
            else
            {
                vMapModeNameProp.SetIntegerValue((int)texture.WrapModeV);
            }

            if (texFlagsNameProp == null)
            {
                texFlagsNameProp = new MaterialProperty(AiMatKeys.TEXFLAGS_BASE, texture.Flags);
                texFlagsNameProp.TextureIndex = texIndex;
                texFlagsNameProp.TextureType = texType;
                mat.AddProperty(texFlagsNameProp);
            }
            else
            {
                texFlagsNameProp.SetIntegerValue(texture.Flags);
            }

            return true;
        }

        public static void ExportTexture(string exportPath, int texId, out string texName)
        {
            texName = RDBNames[(int)ResourceTypeId.Texture].TryGetValue(texId, out string rdbName) ? rdbName.Trim('\0') : $"UnnamedTex_{texId}";
            File.WriteAllBytes($"{Path.GetDirectoryName(exportPath)}\\{texName}", _rdbController.Get<AOTexture>(texId).JpgData);
        }
    }
}
