﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
            var arr = await File.ReadAllBytesAsync("Database.9.tdb");
            var bm = ByteManipulator.Create(arr);
            
            var vaList = new List<VirtoolsArray>();
            try
            {
                for (var i = 0; ; i++)
                {
                    var va = VirtoolsArray.Create(bm);
                    va.SetHeader(bm);
                    va.PopulateCells(bm);
            
                    vaList.Add(va);
            
                    string json = JsonConvert.SerializeObject(va, Formatting.Indented);
                    await File.WriteAllTextAsync($"{va.SheetName}.json", json);
                    Console.Write($"\r{i + 1}/? Completed.");
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine($"Parse Completed. Elapsed {sw.ElapsedMilliseconds}ms.");
                Console.WriteLine($"Memory usage {GC.GetTotalMemory(false) / 1024}KB.");
            }
            sw.Reset();
            sw.Start();
            // Utils.GoToUrl("https://www.baidu.com");
            
            for (var i = 0; i < vaList.Count; i++)
            {
                using (var stream = new FileStream("Database.frankenstein.tdb", i == 0 ? FileMode.Truncate : FileMode.Append))
                {
                    stream.Write(ByteManipulator.Encode(vaList[i].ToByteArray()), 0, vaList[i].ToByteArray().Length);
                }
                Console.Write($"\r{i + 1}/{vaList.Count} Completed.");
            }
            Console.WriteLine();
            Console.WriteLine($"Write Completed. Elapsed {sw.ElapsedMilliseconds}ms.");
            Console.WriteLine($"Memory usage {GC.GetTotalMemory(false) / 1024}KB.");
            await File.WriteAllBytesAsync("Database.new.tdb", bm.Array);
        }
    }
}