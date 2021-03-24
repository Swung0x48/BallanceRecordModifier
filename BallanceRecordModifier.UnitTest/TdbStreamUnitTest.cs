using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
            Assert.Equal(0, tdbStream.Read(new byte[1], 0, 1));
            tdbStream = new TdbStream(System.Array.Empty<byte>());
            Assert.Equal(0, tdbStream.Read(new byte[1]));
            Assert.Equal(0, tdbStream.Read(new byte[1], 0, 1));
        }

        [Theory]
        [InlineData("6222C1E207C6E64446C72486C163A4EF0C", "DB_Highscore_Lv01")]
        [InlineData("6222C16386A48667A62486074446E606676484A7C6", "DB_Levelfreischaltung")]
        public void TestEncodeDecode(string encoded, string decoded)
        {
            var encodedBytes = Convert.FromHexString(encoded);
            
            var tdbStream = new TdbStream(encodedBytes);
            var decodedBytes = new byte[encodedBytes.Length];
            tdbStream.Read(decodedBytes);
            Assert.Equal(Encoding.ASCII.GetBytes(decoded), decodedBytes);
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01")] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default")] // Encoded "Mr. Default" with trailing null character
        public void TestSeek(string encoded, string decoded)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            
            var tdbStream = new TdbStream(encodedBytes);
            Assert.Equal(decoded[0], tdbStream.ReadByte());
            
            tdbStream.Seek(-1, SeekOrigin.Current);
            Assert.Equal(decoded[0], tdbStream.ReadByte());
            
            tdbStream.Seek(encoded.Length - 1, SeekOrigin.Begin);
            Assert.Equal(-1, tdbStream.ReadByte());
            
            tdbStream.Seek(1, SeekOrigin.Begin);
            Assert.Equal(decoded[1], tdbStream.ReadByte());
            
            var rand = new Random();
            var randIndex = rand.Next(0, encodedBytes.Length - 1 - 1); // Make sure next read will read something, aka. Read() will not spitting out -1.
            tdbStream.Seek(randIndex, SeekOrigin.Begin);
            Assert.Equal(decoded[randIndex], tdbStream.ReadByte());

            Assert.Throws<IOException>(() => tdbStream.Seek(-1, SeekOrigin.Begin));
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01", 10)] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default", 10)] // Encoded "Mr. Default" with trailing null character
        public async Task TestRead(string encoded, string decoded, int times)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            var decodedBytes = Encoding.ASCII.GetBytes(decoded);

            for (var i = 0; i < times; i++)
            {
                var rand = new Random();
                var randSize = rand.Next(0, decoded.Length);

                var buffer = new byte[randSize];
                var tdbStream = new TdbStream(encodedBytes);
                tdbStream.Read(buffer, 0, randSize);
                Assert.Equal(decodedBytes[..randSize], buffer);

                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(decodedBytes[..randSize], buffer);
            }
        }
    }
}