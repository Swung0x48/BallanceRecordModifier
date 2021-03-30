using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace BallanceRecordModifier
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        [Benchmark]
        public async Task Benchmarking()
        {
            var arr = await File.ReadAllBytesAsync("Database.tdb");
            var bm = await ByteManipulator.Create(arr);
            
            var vaList = new List<VirtoolsArrayLegacy>();
            try
            {
                for (var i = 0; ; i++)
                {
                    var va = await VirtoolsArrayLegacy.Create(bm);
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
                    stream.Write(ByteManipulator.Encode(byteArray), 0, byteArray.Length);
                }
                Console.Write($"\r{i + 1}/{vaList.Count} arrays written.");
            }
            Console.WriteLine();
            Console.WriteLine($"Write Completed.");
        }
    }
}