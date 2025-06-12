using AODB.Common.RDBObjects;
using Assimp;
using Assimp.Unmanaged;
using System;
using System.Collections.Generic;
using System.IO;

namespace AODB.Encoding
{
    public class ModelExporter
    {
        public static string ExportTexture(RdbController rdbController, RdbTextureType rdbTextureType, int textureId, string path, out string textureName)
        {
            textureName = string.Empty;
            try
            {
                textureName = rdbController.Get<InfoObject>(1).Types[ResourceTypeId.Texture].TryGetValue(textureId, out string rdbName) ? rdbName.Trim('\0') : $"UnnamedTex_{textureId}";

                Image image = null;
                switch (rdbTextureType)
                {
                    case RdbTextureType.SkinTexture:
                        image = rdbController.Get<SkinTexture>(textureId);
                        break;
                    case RdbTextureType.IconTexture:
                        image = rdbController.Get<IconTexture>(textureId);
                        break;
                    case RdbTextureType.GroundTexture:
                        image = rdbController.Get<GroundTexture>(textureId);
                        break;
                    case RdbTextureType.WallTexture:
                        image = rdbController.Get<WallTexture>(textureId);
                        break;
                    case RdbTextureType.AOTexture:
                        image = rdbController.Get<AOTexture>(textureId);
                        break;
                    default:
                        break;
                }

                File.WriteAllBytes($"{path}\\{textureName}", image.JpgData);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string ExportAbiff(RdbController rdbController, RdbMeshType rdbMeshType, int meshId, string exportPath, out Scene scene)
        {
            scene = null;

            try
            {
                if (!Directory.Exists(exportPath))
                    throw new DirectoryNotFoundException();

                var rdbMesh = rdbMeshType == RdbMeshType.RdbMesh ? rdbController.Get<RDBMesh>(meshId) : rdbController.Get<RDBMesh2>(meshId);
                scene = AbiffImporter.ToAssimpScene(rdbMesh.RDBMesh_t, out var uvAnims, out var transKeys, out var rotKeys);

                SetAndExportTextures(rdbController, scene, exportPath);
                FixTransformsAbiff(scene);
                new AssimpContext().ExportFile(scene, $"{exportPath}\\{GetInfoObjectName(rdbController, ResourceTypeId.RdbMesh, meshId).Replace(".abiff","")}.fbx", "fbx");

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static void SetAndExportTextures(RdbController rdbController, Scene scene, string path)
        {
            foreach (var mat in scene.Materials)
            {
                if (TryGetNonTextureProperty(mat, "DiffuseId", out int textureId))
                {
                    ExportTexture(rdbController, RdbTextureType.AOTexture, textureId, path, out var name);
                    AddMaterialTexture(mat, TextureType.Diffuse, name);
                }

                if (TryGetNonTextureProperty(mat, "EmissionId", out textureId))
                {
                    ExportTexture(rdbController, RdbTextureType.AOTexture, textureId, path, out var name);
                    AddMaterialTexture(mat, TextureType.Emissive, name);
                }
            }
        }

        private static void FixTransformsAbiff(Scene scene)
        {
            foreach (var node in scene.RootNode.Children)
            {
                node.Transform = new Matrix4x4(
                -100f, 0, 0, 0,
                0, 100f, 0, 0,
                0, 0, 100f, 0,
                0, 0, 0, 1);
            }

        }
        public static string ExportCir(RdbController rdbController, int meshId, int animId, string path, out Scene scene)
        {
            return ExportCir(rdbController, meshId, path, out scene, animId);
        }

        public static string ExportCir(RdbController rdbController, int meshId, string path, out Scene scene, int animId = 0)
        {
            scene = null;

            try
            {
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException();

                var catMesh = rdbController.Get<RDBCatMesh>(meshId);
                scene = CirImporter.ToAssimpScene(catMesh, new List<AnimData>
                {
                    new AnimData
                    {
                        CatAnim = rdbController.Get<CATAnim>(animId),
                        Name = "test"
                    }
                });


                scene.RootNode.Children[0].Name = GetInfoObjectName(rdbController, ResourceTypeId.CatMesh, meshId).Replace(".cir", "");

                var catAnim = rdbController.Get<CATAnim>(animId);

                //if (animId != 0)
                //{
                //    var anim = new Animation();
                //    anim.Name = catAnim.Name;
                //    anim.TicksPerSecond = 0.001;

                //    foreach (var boneData in catAnim.Animation.BoneData)
                //    {
                //        var nodeChannel = new NodeAnimationChannel();

                //        nodeChannel.PreState = AnimationBehaviour.Constant;
                //        nodeChannel.PostState = AnimationBehaviour.Constant;
                //        var boneId = boneData.BoneId;
                //        nodeChannel.NodeName = $"Bone_{boneId}_{catMesh.Joints[boneId].Name}";
                //        var posKey = boneData.TranslationKeys.Select(x => new VectorKey { Time = x.Time, Value = x.Position.ToAssimp() });
                //        var rotKey = boneData.RotationKeys.Select(x => new QuaternionKey { Time = x.Time, Value = x.Rotation.ToAssimp() });

                //        if (posKey.Count() != 0)
                //        {
                //            nodeChannel.PositionKeys.AddRange(posKey);
                //        }

                //        if (rotKey.Count() != 0)
                //        {
                //            nodeChannel.RotationKeys.AddRange(rotKey);
                //        }

                //        nodeChannel.ScalingKeys.Add(new VectorKey { Time = 0, Value = new Vector3D(1, 1, 1) });

                //        anim.NodeAnimationChannels.Add(nodeChannel);
                //    }

                //    scene.Animations.Add(anim);
                //}

                SetAndExportTextures(rdbController, scene, path);

                var context = new AssimpContext();
                context.ExportFile(scene, $"{path}\\{GetInfoObjectName(rdbController, ResourceTypeId.CatMesh, meshId).Replace(".cir", "")}.fbx", "fbx");

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        //private static void SetSkeleton(RdbController rdbController, Scene scene, CATAnim catAnim, RDBCatMesh catMesh)
        //{
        //    Node[] boneNodes = new Node[catAnim.Animation.BoneData.Count];
        //    Console.WriteLine(catAnim.Animation.BoneData.Count);
        //    Console.WriteLine(catMesh.Joints.Count);
        //    for (int i = 0; i < catAnim.BoneCount; i++)
        //    {
        //        var bone = $"Bone_{i}_{catMesh.Joints[i].Name}";
        //        boneNodes[i] = new Node(bone);
        //        Console.WriteLine(i);

        //        if (catAnim.Animation.BoneData.Any(x => x.BoneId == i))
        //        {
        //            var boneData = catAnim.Animation.BoneData.FirstOrDefault(x => x.BoneId == i);
        //            boneNodes[i].Transform = new Matrix4x4(boneData.RotationKeys[0].Rotation.ToAssimp().GetMatrix()) * Matrix4x4.FromTranslation(boneData.TranslationKeys[0].Position.ToAssimp());

        //        }
        //        //   boneNodes[i].Transform = new Matrix4x4();
        //    }

        //    for (int i = 0; i < catMesh.Joints.Count; i++)
        //    {
        //        var currentJoint = catMesh.Joints[i];

        //        if (currentJoint.ChildJoints.Length == 0)
        //            continue;

        //        for (int j = 0; j < currentJoint.ChildJoints.Length; j++)
        //        {
        //            boneNodes[i].Children.Add(boneNodes[currentJoint.ChildJoints[j]]);
        //        }
        //    }

        //    foreach (var b in boneNodes.Except(boneNodes.SelectMany(x => x.Children)))
        //        scene.RootNode.Children[0].Children.Add(b);
        //}

        private static bool AddMaterialTexture(Material mat, TextureType textureType, string textureName, bool onlySetFilePath = false)
        {
            if (string.IsNullOrEmpty(textureName))
                return false;

            TextureSlot textureSlot = new TextureSlot
            {
                FilePath = textureName,
                TextureType = textureType,
                TextureIndex = 0
            };

            int texIndex = textureSlot.TextureIndex;

            string texName = Material.CreateFullyQualifiedName(AiMatKeys.TEXTURE_BASE, textureType, texIndex);

            MaterialProperty texNameProp = mat.GetProperty(texName);

            if (texNameProp == null)
                mat.AddProperty(new MaterialProperty(AiMatKeys.TEXTURE_BASE, textureSlot.FilePath, textureType, texIndex));
            else
                texNameProp.SetStringValue(textureSlot.FilePath);

            if (onlySetFilePath)
                return true;

            string mappingName = Material.CreateFullyQualifiedName(AiMatKeys.MAPPING_BASE, textureType, texIndex);
            string uvIndexName = Material.CreateFullyQualifiedName(AiMatKeys.UVWSRC_BASE, textureType, texIndex);
            string blendFactorName = Material.CreateFullyQualifiedName(AiMatKeys.TEXBLEND_BASE, textureType, texIndex);
            string texOpName = Material.CreateFullyQualifiedName(AiMatKeys.TEXOP_BASE, textureType, texIndex);
            string uMapModeName = Material.CreateFullyQualifiedName(AiMatKeys.MAPPINGMODE_U_BASE, textureType, texIndex);
            string vMapModeName = Material.CreateFullyQualifiedName(AiMatKeys.MAPPINGMODE_V_BASE, textureType, texIndex);
            string texFlagsName = Material.CreateFullyQualifiedName(AiMatKeys.TEXFLAGS_BASE, textureType, texIndex);

            MaterialProperty mappingNameProp = mat.GetProperty(mappingName);
            MaterialProperty uvIndexNameProp = mat.GetProperty(uvIndexName);
            MaterialProperty blendFactorNameProp = mat.GetProperty(blendFactorName);
            MaterialProperty texOpNameProp = mat.GetProperty(texOpName);
            MaterialProperty uMapModeNameProp = mat.GetProperty(uMapModeName);
            MaterialProperty vMapModeNameProp = mat.GetProperty(vMapModeName);
            MaterialProperty texFlagsNameProp = mat.GetProperty(texFlagsName);

            if (mappingNameProp == null)
            {
                mappingNameProp = new MaterialProperty(AiMatKeys.MAPPING_BASE, (int)textureSlot.Mapping);
                mappingNameProp.TextureIndex = texIndex;
                mappingNameProp.TextureType = textureType;
                mat.AddProperty(mappingNameProp);
            }
            else
            {
                mappingNameProp.SetIntegerValue((int)textureSlot.Mapping);
            }

            if (uvIndexNameProp == null)
            {
                uvIndexNameProp = new MaterialProperty(AiMatKeys.UVWSRC_BASE, textureSlot.UVIndex);
                uvIndexNameProp.TextureIndex = texIndex;
                uvIndexNameProp.TextureType = textureType;
                mat.AddProperty(uvIndexNameProp);
            }
            else
            {
                uvIndexNameProp.SetIntegerValue(textureSlot.UVIndex);
            }

            if (blendFactorNameProp == null)
            {
                blendFactorNameProp = new MaterialProperty(AiMatKeys.TEXBLEND_BASE, textureSlot.BlendFactor);
                blendFactorNameProp.TextureIndex = texIndex;
                blendFactorNameProp.TextureType = textureType;
                mat.AddProperty(blendFactorNameProp);
            }
            else
            {
                blendFactorNameProp.SetFloatValue(textureSlot.BlendFactor);
            }

            if (texOpNameProp == null)
            {
                texOpNameProp = new MaterialProperty(AiMatKeys.TEXOP_BASE, (int)textureSlot.Operation);
                texOpNameProp.TextureIndex = texIndex;
                texOpNameProp.TextureType = textureType;
                mat.AddProperty(texOpNameProp);
            }
            else
            {
                texOpNameProp.SetIntegerValue((int)textureSlot.Operation);
            }

            if (uMapModeNameProp == null)
            {
                uMapModeNameProp = new MaterialProperty(AiMatKeys.MAPPINGMODE_U_BASE, (int)textureSlot.WrapModeU);
                uMapModeNameProp.TextureIndex = texIndex;
                uMapModeNameProp.TextureType = textureType;
                mat.AddProperty(uMapModeNameProp);
            }
            else
            {
                uMapModeNameProp.SetIntegerValue((int)textureSlot.WrapModeU);
            }

            if (vMapModeNameProp == null)
            {
                vMapModeNameProp = new MaterialProperty(AiMatKeys.MAPPINGMODE_V_BASE, (int)textureSlot.WrapModeV);
                vMapModeNameProp.TextureIndex = texIndex;
                vMapModeNameProp.TextureType = textureType;
                mat.AddProperty(vMapModeNameProp);
            }
            else
            {
                vMapModeNameProp.SetIntegerValue((int)textureSlot.WrapModeV);
            }

            if (texFlagsNameProp == null)
            {
                texFlagsNameProp = new MaterialProperty(AiMatKeys.TEXFLAGS_BASE, textureSlot.Flags);
                texFlagsNameProp.TextureIndex = texIndex;
                texFlagsNameProp.TextureType = textureType;
                mat.AddProperty(texFlagsNameProp);
            }
            else
            {
                texFlagsNameProp.SetIntegerValue(textureSlot.Flags);
            }

            return true;
        }

        private static string GetInfoObjectName(RdbController rdbController, ResourceTypeId resourceTypeId, int id)
        {
            return rdbController.Get<InfoObject>(1).Types[resourceTypeId].TryGetValue(id, out string rdbName) ? rdbName.Trim('\0') : $"Unnamed_{id}";
        }

        private static bool TryGetNonTextureProperty(Material material, string propertyName, out int value)
        {
            value = default;

            if (!material.HasNonTextureProperty(propertyName))
                return false;

            value = material.GetNonTextureProperty(propertyName).GetIntegerValue();
            return true;
        }

        public enum RdbMeshType
        {
            RdbMesh,
            RdbMesh2
        }

        public enum RdbTextureType
        {
            AOTexture,
            WallTexture,
            SkinTexture,
            IconTexture,
            GroundTexture,
        }
    }
}
