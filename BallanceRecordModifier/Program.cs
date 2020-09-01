using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BallanceRecordModifier
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var initMem = GC.GetTotalMemory(false);
            var arr = await File.ReadAllBytesAsync("Database.tdb");
            var bm = await ByteManipulator.Create(arr);
            
            var vaList = new List<VirtoolsArray>();
            try
            {
                for (var i = 0; ; i++)
                {
                    var va = await VirtoolsArray.Create(bm);
                    await va.SetHeader(bm);
                    await va.PopulateCells(bm);
            
                    vaList.Add(va);
            
                    string json = JsonConvert.SerializeObject(va, Formatting.Indented);
                    await File.WriteAllTextAsync($"{va.SheetName}.json", json);
                    Console.Write($"\r{i + 1} arrays read.");
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine($"Parse Completed. Elapsed {sw.ElapsedMilliseconds}ms.");
                Console.WriteLine($"Memory usage {(GC.GetTotalMemory(false) - initMem) / 1024}KB.");
            }
            
            sw.Reset();
            sw.Start();
            // Utils.GoToUrl("https://www.baidu.com");

            for (var i = 0; i < vaList.Count; i++)
            {
                using (var stream = new FileStream("Database.frankenstein.tdb", i == 0 ? FileMode.Truncate : FileMode.Append))
                {
                    var byteArray = await vaList[i].ToByteArray();
                    stream.Write(ByteManipulator.Encode(byteArray), 0, byteArray.Length);
                }
                Console.Write($"\r{i + 1}/{vaList.Count} arrays written.");
            }
            Console.WriteLine();
            Console.WriteLine($"Write Completed. Elapsed {sw.ElapsedMilliseconds}ms.");
            Console.WriteLine($"Memory usage {(GC.GetTotalMemory(false) - initMem) / 1024}KB.");
            // await File.WriteAllBytesAsync("Database.new.tdb", bm.Array);
        }
    }
}