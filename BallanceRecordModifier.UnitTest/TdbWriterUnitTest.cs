using System;
using System.IO;
using Xunit;

namespace BallanceRecordModifier.UnitTest
{
    public class TdbWriterTest
    {
        [Fact]
        public void TestException()
        {
            var tdbStream = new TdbStream(false, true);
            Assert.Throws<ArgumentException>(() => new TdbWriter(tdbStream));
        }

        [Theory]
        [InlineData("DB_Highscore_Lv01")]
        [InlineData("Mr. Default")]
        public void TestWriteString(string input)
        {
            var tdbStream = new TdbStream(false, false);
            using var tdbWriter = new TdbWriter(tdbStream);
            tdbWriter.Write(input);
            // tdbStream.ReadAsEncoded = false;
            tdbStream.Seek(0, SeekOrigin.Begin);
            using var tdbReader = new TdbReader(tdbStream);
            Assert.Equal(input, tdbReader.ReadString());
        }
    }
}