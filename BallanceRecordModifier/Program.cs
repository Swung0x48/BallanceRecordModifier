using System.IO;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bm = await ByteManipulator.Create("Databse.tdb");
            //await bm.SaveToFile("Balls.new.nmo");
            //await File.WriteAllBytesAsync("Balls.new.nmo", bm.Decoded);
        }
    }
}