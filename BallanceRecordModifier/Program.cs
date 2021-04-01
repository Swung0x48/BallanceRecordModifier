using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;

namespace BallanceRecordModifier
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if RELEASE
            BenchmarkRunner.Run<Benchmark>();
#endif
#if DEBUG
             var benchmark = new Benchmark();
             await benchmark.LegacyMethod();
             await benchmark.NewMethod();
#endif
        }
    }
}