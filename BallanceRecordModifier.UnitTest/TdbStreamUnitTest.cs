using Xunit;

namespace BallanceRecordModifier.UnitTest
{
    public class TdbStreamUnitTest
    {
        [Fact]
        public void TestEmpty()
        {
            var tdbStream = new TdbStream();
            Assert.Equal(0, tdbStream.Read(new byte[1]));
            tdbStream = new TdbStream(System.Array.Empty<byte>());
            Assert.Equal(0, tdbStream.Read(new byte[1]));
        }
        
        
    }
}