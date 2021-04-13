using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace BallanceRecordModifier
{
    class Program
    {
#if RELEASE
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
#endif
        
#if DEBUG
        static async Task Main(string[] args)
        {
             var benchmark = new Benchmark();
             await benchmark.LegacyMethod();
             await benchmark.NewMethod();
        }
#endif

    }
}