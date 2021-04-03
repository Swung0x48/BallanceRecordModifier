using System;
using System.Collections.Concurrent;
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
        private static string _basePath =
            "/Users/swung0x48/RiderProjects/BallanceRecordModifier/BallanceRecordModifier/bin/Debug/net5.0";
        private static string _pathToInputFile =
            _basePath + "/Database.10000xEnlarged.tdb";
        private static string _pathToOutputFile =
            _basePath + "/Database.out.tdb";
        
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
            
                    // string json = JsonConvert.SerializeObject(va, Formatting.Indented);
                    // await File.WriteAllTextAsync($"{va.SheetName}.json", json);
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
                    Console.Write($"\r[{i}/-] Reading array: {virtoolsArray.SheetName}");
#endif
                    virtoolsArrays.Add(virtoolsArray);
                    var chunk = new byte[virtoolsArray.ChunkSize];
                    
                    tdbStream.Read(chunk);
                    
                    var memory = new ReadOnlyMemory<byte>(chunk);
                    populateTasks.Add(virtoolsArray.PopulateAsync(memory));
#if DEBUG
                    // await Task.Run(() =>
                    //     Console.Write($"\rStarting populating array #{i} asynchronously: {virtoolsArrays[i - 1].SheetName}"));
#endif
                }
            }
            catch (EndOfStreamException)
            {
#if DEBUG
                Console.WriteLine("\nEnd of the stream reached.");
#endif
            }

#if DEBUG
            Console.WriteLine("Waiting all parsing process to finish...");
#endif

            Task.WaitAll(populateTasks.ToArray());
#if DEBUG
            Console.WriteLine("Parse Completed.");
#endif

            // virtoolsArrays.ForEach(e =>
            // {
            //     string json = JsonConvert.SerializeObject(e, Formatting.Indented);
            //     File.WriteAllText($"{e.SheetName}.json", json);
            // });

            var stream = new FileStream(_pathToOutputFile, FileMode.Truncate);

            var serializingTasks = new BlockingCollection<Task<byte[]>>();
            var chunksToWrite = new BlockingCollection<byte[]>();

            var consumerTask = Task.Run(() =>
            {
                var i = 0;
                while (!chunksToWrite.IsCompleted)
                {
#if DEBUG
                    Console.WriteLine($"[{++i}/{virtoolsArrays.Count}] Writing chunk to file...");
#endif
                    stream.Write(chunksToWrite.Take());
                }
#if DEBUG
                Console.WriteLine("All chunks written!");
#endif
            });
            
            var waitingProducerFinishTask = Task.Run(() =>
            {
                var i = 0;
                while (!serializingTasks.IsCompleted)
                {
                    var task = serializingTasks.Take();
#if DEBUG
                    Console.WriteLine($"[{++i}/{virtoolsArrays.Count}] Waiting task to finish...");
#endif
                    task.Wait();
#if DEBUG
                    Console.WriteLine($"[{i}/{virtoolsArrays.Count}] Serializing task finished...");
#endif
                    chunksToWrite.Add(task.Result);
                }
                chunksToWrite.CompleteAdding();
#if DEBUG
                Console.WriteLine("All serializing tasks finished!");
#endif
            });

            var producerTask = Task.Run(() =>
            {
                for (var i = 0; i < virtoolsArrays.Count; i++)
                {
#if DEBUG
                    Console.WriteLine($"Serializing array: {virtoolsArrays[i].SheetName}");
#endif
                    serializingTasks.Add(virtoolsArrays[i].ToArrayAsync());
                }

                serializingTasks.CompleteAdding();
#if DEBUG
                Console.WriteLine("All serializing tasks fired!");
#endif
            });

            consumerTask.Wait();

            stream.Close();
        }
    }
}