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
            Parser.Default.ParseArguments<ImportTextureOptions>(args)
              .WithParsed<ImportTextureOptions>(options => RunImportTexture(options, ResourceTypeId.Texture))
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
                var RDBNames = db.Get<InfoObject>(1);
                string blah = Path.GetFileName(opts.Path);

                if (opts.Id > 0)
                {
                    db.PutRaw((int)resourceType, opts.Id, 1, File.ReadAllBytes(opts.Path));
                }
                else if(RDBNames.Types[resourceType].TryGetKey(Path.GetFileName(opts.Path) + '\0', out int recordId))
                {
                    db.PutRaw((int)resourceType, recordId, 1, File.ReadAllBytes(opts.Path));
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
            Console.Read();
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.Read();
        }
    }
}
