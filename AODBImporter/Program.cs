using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using AODB.Common;
using AODB.Common.DbClasses;
using AODB.Common.RDBObjects;
using AODB.Common.Structs;
using AODB.Encoding;
using Assimp;
using CommandLine;

namespace AODBImporter
{
    internal class Program
    {
        [Verb("mass_import_texture")]
        private class MassImportTextureOptions : BaseOptions
        {
            [Option("path", Required = true, HelpText = "Path to directory to be imported.")]
            public string Path { get; set; }
        }

        [Verb("mass_import_ground_texture")]
        private class MassImportGroundTextureOptions : BaseOptions
        {
            [Option("path", Required = true, HelpText = "Path to directory to be imported.")]
            public string Path { get; set; }
        }

        [Verb("import_texture")]
        private class ImportTextureOptions : BaseOptions
        {
            [Option("path", Required = true, HelpText = "Path to image to be imported.")]
            public string Path { get; set; }

            [Option("id", Default = -1, HelpText = "RecordId of the texture. Do not use if replacing a texture with an image matching the name of an existing texture.")]
            public int Id { get; set; }
        }

        [Verb("import_fbx")]
        private class ImportFBXOptions : BaseOptions
        {
            [Option("path", Required = true, HelpText = "Path to FBX to be imported.")]
            public string Path { get; set; }

            [Option("id", Default = -1, HelpText = "RecordId of the mesh. Do not use if replacing a mesh with the same filename.")]
            public int Id { get; set; }
        }

        private class BaseOptions
        {
            [Option("aopath", Required = true, HelpText = "The path to the Anarchy Online folder.")]
            public string AOPath { get; set; }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ImportFBXOptions, ImportTextureOptions, MassImportTextureOptions, MassImportGroundTextureOptions>(args)
              .WithParsed<ImportFBXOptions>(options => RunImportFBX(options))
              .WithParsed<ImportTextureOptions>(options => RunImportTexture(options, ResourceTypeId.Texture))
              .WithParsed<MassImportTextureOptions>(options => RunMassImportTexture(options, ResourceTypeId.Texture))
              .WithParsed<MassImportGroundTextureOptions>(options => RunMassGroundImportTexture(options))
              .WithNotParsed(HandleParseError);
        }

        private static void RunImportFBX(ImportFBXOptions opts)
        {
            if (!File.Exists(opts.Path))
            {
                Console.Error.WriteLine($"Unable to find file {opts.Path}");
                return;
            }

            Directory.SetCurrentDirectory(opts.AOPath);

            var db = new NativeDbLite();

            db.LoadDatabase(Path.Combine(opts.AOPath, "cd_image/data/db/ResourceDatabase.idx"));

            try
            {
                var infoObject = db.Get<InfoObject>(1);


                RDBMesh_t mesh = AbiffImporter.LoadFromFBX(opts.Path, infoObject, out Dictionary<int, Material> mats);

                if (opts.Id > 0)
                {
                    var existingMesh = db.Get<RDBMesh>(opts.Id);
                    existingMesh.RDBMesh_t = mesh;
                    db.PutRaw((int)ResourceTypeId.RdbMesh, opts.Id, 8008, existingMesh.Serialize());
                }
                else if (infoObject.Types[ResourceTypeId.RdbMesh].TryGetKey(Path.GetFileName(opts.Path).Replace(".fbx", ".abiff"), out int recordId))
                {
                    var existingMesh = db.Get<RDBMesh>(opts.Id);
                    existingMesh.RDBMesh_t = mesh;
                    db.PutRaw((int)ResourceTypeId.RdbMesh, opts.Id, 8008, existingMesh.Serialize());
                }
                else
                {
                    Console.Error.WriteLine($"Could not find mesh to replace.");
                    return;
                }

                foreach (var texture in mats)
                    db.PutRaw((int)ResourceTypeId.Texture, texture.Key, 8008, File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(opts.Path), texture.Value.TextureDiffuse.FilePath)));

                db.PutRaw((int)ResourceTypeId.InfoObject, 1, 8008, infoObject.Serialize());
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Failed to import mesh.");
            }
            finally
            {
                db.Dispose();
            }

            Console.WriteLine("Done.");
        }

        private static void RunImportTexture(ImportTextureOptions opts, ResourceTypeId resourceType)
        {
            if (!File.Exists(opts.Path))
            {
                Console.Error.WriteLine($"Unable to find file {opts.Path}");
                return;
            }

            Directory.SetCurrentDirectory(opts.AOPath);

            var db = new NativeDbLite();

            db.LoadDatabase(Path.Combine(opts.AOPath, "cd_image/data/db/ResourceDatabase.idx"));

            try
            {
                var infoObject = db.Get<InfoObject>(1);

                if (opts.Id > 0)
                {
                    db.PutRaw((int)resourceType, opts.Id, 8008, File.ReadAllBytes(opts.Path));
                }
                else if (infoObject.Types[resourceType].TryGetKey(Path.GetFileName(opts.Path), out int recordId))
                {
                    db.PutRaw((int)resourceType, recordId, 8008, File.ReadAllBytes(opts.Path));
                }
                else
                {
                    Console.Error.WriteLine($"Could not find texture to replace.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to import texture.");
            }
            finally
            {
                db.Dispose();
            }

            Console.WriteLine("Done.");
        }

        private static void RunMassImportTexture(MassImportTextureOptions opts, ResourceTypeId resourceType)
        {
            if (!Directory.Exists(opts.Path))
            {
                Console.Error.WriteLine($"Unable to find directory {opts.Path}");
                return;
            }

            Directory.SetCurrentDirectory(opts.AOPath);

            var db = new NativeDbLite();

            db.LoadDatabase(Path.Combine(opts.AOPath, "cd_image/data/db/ResourceDatabase.idx"));
            try
            {
                var infoObject = db.Get<InfoObject>(1);
                foreach (string file in Directory.GetFiles(opts.Path))
                {
                    if (infoObject.Types[resourceType].TryGetKey(Path.GetFileName(file), out int recordId))
                    {
                        db.PutRaw((int)resourceType, recordId, 8008, File.ReadAllBytes(file));
                        Console.WriteLine($"Replaced texture {Path.GetFileName(file)}");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Could not find texture to replace. {Path.GetFileName(file)}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to import texture.");
            }
            finally
            {
                db.Dispose();
            }

            Console.WriteLine("Done.");
        }

        private static void RunMassGroundImportTexture(MassImportGroundTextureOptions opts)
        {
            if (!Directory.Exists(opts.Path))
            {
                Console.Error.WriteLine($"Unable to find directory {opts.Path}");
                return;
            }

            Directory.SetCurrentDirectory(opts.AOPath);

            var db = new NativeDbLite();

            db.LoadDatabase(Path.Combine(opts.AOPath, "cd_image/data/db/ResourceDatabase.idx"));

            try
            {
                foreach (string file in Directory.GetFiles(opts.Path))
                {
                    if(int.TryParse(Path.GetFileName(file).Replace(".png", ""), out int id))
                    {
                        byte[] header = db.GetRaw((int)ResourceTypeId.GroundTexture, id).Take(24).ToArray();

                        db.PutRaw((int)ResourceTypeId.GroundTexture, id, 8008, header.Concat(File.ReadAllBytes(file)).ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to import texture.");
            }
            finally
            {
                db.Dispose();
            }

            Console.WriteLine("Done.");
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.Read();
        }
    }
}
