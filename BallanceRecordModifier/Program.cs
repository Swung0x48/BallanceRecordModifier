using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BallanceRecordModifier
{
    //[MemoryDiagnoser]
    class Program
    {
        //[Benchmark]
        static async Task Main(string[] args)
        {
            /*var arr = await File.ReadAllBytesAsync("Database.tdb");
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
                Console.WriteLine($"Parse Completed.");
            }
            
            for (var i = 0; i < vaList.Count; i++)
            {
                await using (var stream = new FileStream("Database.frankenstein.tdb", i == 0 ? FileMode.Truncate : FileMode.Append))
                {
                    var byteArray = await vaList[i].ToByteArray();
                    stream.Write(byteArray, 0, byteArray.Length);
                }
                Console.Write($"\r{i + 1}/{vaList.Count} arrays written.");
            }
            Console.WriteLine();
            Console.WriteLine($"Write Completed.");*/
            // BenchmarkRunner.Run<Benchmark>();
            await using var fileStream = new FileStream("Database.tdb", FileMode.Open);
            var tdbStream = new TdbStream(fileStream);
            using var tdbReader = new TdbReader(tdbStream);
            Console.WriteLine(tdbReader.ReadString());
            var chunkSize = tdbReader.ReadInt32();
            Console.WriteLine($"chunkSize: {chunkSize}");
            byte[] buffer = new byte[chunkSize];
            tdbReader.Read(buffer);
        }
    }
}