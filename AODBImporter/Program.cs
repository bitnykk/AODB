using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AODB.Common;
using AODB.Common.DbClasses;
using AODB.Common.RDBObjects;
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

        [Verb("import_texture")]
        private class ImportTextureOptions : BaseOptions
        {
            [Option("path", Required = true, HelpText = "Path to image to be imported.")]
            public string Path { get; set; }

            [Option("id", Default = -1, HelpText = "RecordId of the texture. Do not use if replacing a texture with an image matching the name of an existing texture.")]
            public int Id { get; set; }
        }

        private class BaseOptions
        {
            [Option("aopath", Required = true, HelpText = "The path to the Anarchy Online folder.")]
            public string AOPath { get; set; }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ImportTextureOptions, MassImportTextureOptions>(args)
              .WithParsed<ImportTextureOptions>(options => RunImportTexture(options, ResourceTypeId.Texture))
              .WithParsed<MassImportTextureOptions>(options => RunMassImportTexture(options, ResourceTypeId.Texture))
              .WithNotParsed(HandleParseError);
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
                else if(infoObject.Types[resourceType].TryGetKey(Path.GetFileName(opts.Path), out int recordId))
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

                foreach(string file in Directory.GetFiles(opts.Path)) 
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

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.Read();
        }
    }
}
