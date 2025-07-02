using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using AODB;
using AODB.Common.Enums;
using AODB.Common.RDBObjects;
using AODB.Common.Structs;

namespace Test
{
    internal class Program
    {
        public static RdbController _rdbController;
        public static Dictionary<ResourceTypeId, Dictionary<int, string>> RDBNames;
        private static LoadingBar _loadingBar;

        static void Main(string[] args)
        {

            string aoPath = "D:\\Funcom\\Anarchy Online 5";

            Directory.SetCurrentDirectory(aoPath);
            _rdbController = new RdbController(aoPath);

            var recordTypes = _rdbController.RecordTypeToId[1000020].Keys;

            _loadingBar = new LoadingBar(recordTypes.Count());

            Dictionary<int, byte[]> itemBlob = new Dictionary<int, byte[]>();

            foreach (var itemId in recordTypes)
            {
                itemBlob.Add(itemId, _rdbController.GetRaw(1000020, itemId));
            }

            ConcurrentBag<ItemEntry> items = new ConcurrentBag<ItemEntry>();
            Parallel.ForEach(itemBlob, item =>
            {
                items.Add(new ItemEntry(item.Value));
                _loadingBar.Increment();
            });


            Console.ReadLine();
        }
    }
}
