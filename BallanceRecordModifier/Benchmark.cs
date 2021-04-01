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
        private string _pathToInputFile =
            "path/to/Database.tdb";
        private string _pathToOutputFile =
            "/path/to/Database.out.tdb";
        
        [Benchmark]
        public async Task LegacyMethod()
        {
            var arr = await File.ReadAllBytesAsync(_pathToInputFile);
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
#if DEBUG 
                    Console.Write($"\r{i + 1} arrays read.");
#endif
                }
            }
            catch (InvalidOperationException e)
            {
#if DEBUG
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine("Parse Completed.");
#endif
            }
            
            for (var i = 0; i < vaList.Count; i++)
            {
                await using (var stream = new FileStream(_pathToOutputFile, i == 0 ? FileMode.Truncate : FileMode.Append))
                {
                    var byteArray = await vaList[i].ToByteArray();
                    stream.Write(ByteManipulator.Encode(byteArray), 0, byteArray.Length);
                }
#if DEBUG
                Console.Write($"\r{i + 1}/{vaList.Count} arrays written.");
#endif
            }
#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Write Completed.");
            Console.WriteLine();
#endif

        }

        [Benchmark]
        public async Task NewMethod()
        {
            await using var fs = File.Open(_pathToInputFile, FileMode.Open, FileAccess.Read);
            var tdbStream = new TdbStream(false, true, fs);
            var tdbReader = new TdbReader(tdbStream);

            var virtoolsArrays = new List<VirtoolsArray>();
            var populateTasks = new List<Task>();

            try
            {
                for (var i = 1;; i++)
                {
                    if (tdbStream.Position == tdbStream.Length)
                        throw new EndOfStreamException();
                    var virtoolsArray = await VirtoolsArray.CreateAsync(tdbReader, false);
#if DEBUG
                    await Task.Run(() => Console.WriteLine($"Creating array #{i}: {virtoolsArray.SheetName}"));
#endif
                    virtoolsArrays.Add(virtoolsArray);
                    var chunk = new byte[virtoolsArray.ChunkSize];
                    
                    tdbStream.Read(chunk);
                    
                    var memory = new ReadOnlyMemory<byte>(chunk);
                    populateTasks.Add(virtoolsArray.PopulateAsync(memory));
#if DEBUG
                    await Task.Run(() =>
                        Console.WriteLine($"Starting populating array #{i} asynchronously: {virtoolsArrays[i - 1].SheetName}"));
#endif
                }
            }
            catch (EndOfStreamException)
            {
#if DEBUG
                Console.WriteLine("End of the stream reached.");
#endif
            }

#if DEBUG
            Console.WriteLine("Waiting all populating process to finish...");
#endif

            Task.WaitAll(populateTasks.ToArray());
#if DEBUG
            Console.WriteLine("All populating process to finished!");
#endif

            virtoolsArrays.ForEach(e =>
            {
                string json = JsonConvert.SerializeObject(e, Formatting.Indented);
                File.WriteAllText($"{e.SheetName}.json", json);
            });

            var stream = new FileStream(_pathToOutputFile, FileMode.Truncate);

            var chunkGeneratingTasks = new List<Task<byte[]>>();
            virtoolsArrays.ForEach(async array =>
            {
#if DEBUG
                Console.WriteLine($"Preparing binary data to write for {array.SheetName}");
#endif
                chunkGeneratingTasks.Add(array.ToArrayAsync());
            });

            foreach (var task in chunkGeneratingTasks)
            {
#if DEBUG
                Console.WriteLine("Waiting preparing process to finish...");
#endif
                task.Wait();
#if DEBUG
                Console.Write("Writing current fragment to file...");
#endif
                stream.Write(task.Result);
            }
            stream.Close();
        }
    }
}