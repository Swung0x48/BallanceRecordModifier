using System;
using Xunit;

namespace BallanceRecordModifier.UnitTest
{
    public class TdbReaderWriterTest
    {
        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01")] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default")] // Encoded "Mr. Default" with trailing null character

        public void TestReadString(string encoded, string result)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            using var tdbStream = new TdbStream(false, true, encodedBytes);
            using var tdbReader = new TdbReader(tdbStream);
            Assert.Equal(result, tdbReader.ReadString());
        }
    }
}