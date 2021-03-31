using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BallanceRecordModifier.UnitTest
{
    public class TdbStreamUnitTest
    {
        [Fact]
        public void TestEmpty()
        {
            var tdbStream = new TdbStream(false, true);
            Assert.Equal(0, tdbStream.Read(new byte[1]));
            Assert.Equal(0, tdbStream.Read(new byte[1], 0, 1));
            tdbStream = new TdbStream(false, true, System.Array.Empty<byte>());
            Assert.Equal(0, tdbStream.Read(new byte[1]));
            Assert.Equal(0, tdbStream.Read(new byte[1], 0, 1));
            Assert.Equal(Task.CompletedTask, tdbStream.WriteAsync(Array.Empty<byte>(), 0, 0));
        }

        [Theory]
        [InlineData("6222C1E207C6E64446C72486C163A4EF0C", "DB_Highscore_Lv01")]
        [InlineData("6222C16386A48667A62486074446E606676484A7C6", "DB_Levelfreischaltung")]
        public void TestEncodeDecode(string encoded, string decoded)
        {
            var encodedBytes = Convert.FromHexString(encoded);
            
            var tdbStream = new TdbStream(false, true, encodedBytes);
            var decodedBytes = new byte[encodedBytes.Length];
            Assert.Equal(decoded.Length, tdbStream.Read(decodedBytes));
            Assert.Equal(Encoding.ASCII.GetBytes(decoded), decodedBytes);
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01")] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default")] // Encoded "Mr. Default" with trailing null character
        public void TestSeek(string encoded, string decoded)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            
            var tdbStream = new TdbStream(false, true, encodedBytes);
            Assert.Equal(decoded[0], tdbStream.ReadByte());
            
            tdbStream.Seek(-1, SeekOrigin.Current);
            Assert.Equal(decoded[0], tdbStream.ReadByte());
            
            tdbStream.Seek(encoded.Length - 1, SeekOrigin.Begin);
            Assert.Equal(-1, tdbStream.ReadByte());
            
            tdbStream.Seek(1, SeekOrigin.Begin);
            Assert.Equal(decoded[1], tdbStream.ReadByte());
            
            var rand = new Random();
            var randIndex = rand.Next(0, encodedBytes.Length - 1 - 1); // Make sure next read will read something, aka. ReadAsync() will not spitting out -1.
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
            var tdbStream = new TdbStream(false, true, encodedBytes);
            
            for (var i = 0; i < times; i++)
            {
                var rand = new Random();
                var randSize = rand.Next(decoded.Length);

                var buffer = new byte[randSize];

                tdbStream.ReadAsEncoded = false;
                tdbStream.Seek(0, SeekOrigin.Begin);
                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(decodedBytes[..randSize], buffer);

                tdbStream.Seek(0, SeekOrigin.Begin);
                Assert.Equal(randSize, tdbStream.Read(buffer, 0, randSize));
                Assert.Equal(decodedBytes[..randSize], buffer);
                
                tdbStream.Seek(0, SeekOrigin.Begin);
                Assert.Equal(buffer.Length, tdbStream.Read(buffer));
                Assert.Equal(decodedBytes[..buffer.Length], buffer);

                tdbStream.Seek(0, SeekOrigin.Begin);
                await tdbStream.ReadAsync(buffer.AsMemory(0, randSize));
                Assert.Equal(decodedBytes[..randSize], buffer);

                tdbStream.ReadAsEncoded = true;
                tdbStream.Seek(0, SeekOrigin.Begin);
                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(encodedBytes[..randSize], buffer);

                tdbStream.Seek(0, SeekOrigin.Begin);
                Assert.Equal(randSize, tdbStream.Read(buffer, 0, randSize));
                Assert.Equal(encodedBytes[..randSize], buffer);
                
                tdbStream.Seek(0, SeekOrigin.Begin);
                Assert.Equal(buffer.Length, tdbStream.Read(buffer));
                Assert.Equal(encodedBytes[..buffer.Length], buffer);

                tdbStream.Seek(0, SeekOrigin.Begin);
                await tdbStream.ReadAsync(buffer.AsMemory(0, randSize));
                Assert.Equal(encodedBytes[..randSize], buffer);
            }
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01", 10)] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default", 10)] // Encoded "Mr. Default" with trailing null character
        public void TestWrite(string encoded, string decoded, int times)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            var decodedBytes = Encoding.ASCII.GetBytes(decoded);
            var tdbStream = new TdbStream(false, true);
            var buffer = new byte[decodedBytes.Length];
            
            for (var i = 0; i < times; i++)
            {
                var rand = new Random();
                var randSize = rand.Next(encodedBytes.Length);

                tdbStream.WriteAsEncoded = true;
                tdbStream.Write(encodedBytes);
                tdbStream.Seek(-encodedBytes.Length, SeekOrigin.Current);
                Assert.Equal(buffer.Length, tdbStream.Read(buffer));
                Assert.Equal(Convert.ToHexString(decodedBytes), Convert.ToHexString(buffer));

                tdbStream.Write(encodedBytes, 0, randSize);
                tdbStream.Seek(-randSize, SeekOrigin.Current);
                tdbStream.Read(buffer, 0, randSize);
                Assert.Equal(Convert.ToHexString(decodedBytes[..randSize]), Convert.ToHexString(buffer[..randSize]));
                
                tdbStream.WriteAsEncoded = false;
                tdbStream.Write(decodedBytes);
                tdbStream.Seek(-decodedBytes.Length, SeekOrigin.Current);
                Assert.Equal(decodedBytes.Length, tdbStream.Read(buffer));
                Assert.Equal(Convert.ToHexString(decodedBytes), Convert.ToHexString(buffer));
                
                tdbStream.Write(decodedBytes, 0, randSize);
                tdbStream.Seek(-randSize, SeekOrigin.Current);
                tdbStream.Read(buffer, 0, randSize);
                Assert.Equal(Convert.ToHexString(decodedBytes[..randSize]), Convert.ToHexString(buffer[..randSize]));
            }
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01", 10)] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default", 10)] // Encoded "Mr. Default" with trailing null character
        public async Task TestWriteAsync(string encoded, string decoded, int times)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            var decodedBytes = Encoding.ASCII.GetBytes(decoded);
            var tdbStream = new TdbStream(false, true);
            var buffer = new byte[decodedBytes.Length];
            
            for (var i = 0; i < times; i++)
            {
                var rand = new Random();
                var randSize = rand.Next(encodedBytes.Length);

                tdbStream.WriteAsEncoded = true;
                await tdbStream.WriteAsync(encodedBytes, 0, randSize);
                tdbStream.Seek(-randSize, SeekOrigin.Current);
                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(Convert.ToHexString(decodedBytes[..randSize]), Convert.ToHexString(buffer[..randSize]));
                
                await tdbStream.WriteAsync(encodedBytes.AsMemory(0, randSize));
                tdbStream.Seek(-randSize, SeekOrigin.Current);
                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(Convert.ToHexString(decodedBytes[..randSize]), Convert.ToHexString(buffer[..randSize]));
                
                tdbStream.WriteAsEncoded = false;
                await tdbStream.WriteAsync(decodedBytes, 0, randSize);
                tdbStream.Seek(-randSize, SeekOrigin.Current);
                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(Convert.ToHexString(decodedBytes[..randSize]), Convert.ToHexString(buffer[..randSize]));
                
                await tdbStream.WriteAsync(decodedBytes.AsMemory(0, randSize));
                tdbStream.Seek(-randSize, SeekOrigin.Current);
                await tdbStream.ReadAsync(buffer, 0, randSize);
                Assert.Equal(Convert.ToHexString(decodedBytes[..randSize]), Convert.ToHexString(buffer[..randSize]));

            }
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C", "DB_Highscore_Lv01")] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default")] // Encoded "Mr. Default" with trailing null character
        public void TestWriteByte(string encoded, string decoded)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            var decodedBytes = Encoding.ASCII.GetBytes(decoded);

            var tdbStream = new TdbStream(false, true);
            foreach (var t in encodedBytes)
                tdbStream.WriteByte(t);
            
            tdbStream.Seek(0, SeekOrigin.Begin);
            foreach (var t in decodedBytes)
                Assert.Equal(t, tdbStream.ReadByte());

            tdbStream.WriteAsEncoded = false;
            foreach (var decodedByte in decodedBytes)
                tdbStream.WriteByte(decodedByte);

            tdbStream.Position = 0;
            
            foreach (var t in decodedBytes)
                Assert.Equal(t, tdbStream.ReadByte());
        }

        [Theory]
        [InlineData("6222C1E2 07C6E644 46C72486 C163A4EF 0C",
            "DB_Highscore_Lv01")] // Encoded "DB_Highscore_Lv01" without trailing null character ('\0')
        [InlineData("8324AFE9 6286A606 846764F5", "Mr. Default")] // Encoded "Mr. Default" with trailing null character
        public async Task TestCopyTo(string encoded, string decoded)
        {
            var encodedBytes = Convert.FromHexString(encoded.Replace(" ", ""));
            var decodedBytes = Encoding.ASCII.GetBytes(decoded);

            var tdbStream = new TdbStream(false, true, encodedBytes);
            tdbStream.ReadByte();
            
            var copiedStream = new TdbStream(false, true);
            // copiedStream.WriteByte(1);
            tdbStream.CopyTo(copiedStream);
            copiedStream.Seek(0, SeekOrigin.Begin);
            Assert.Equal(decodedBytes[1], copiedStream.ReadByte());

            copiedStream.Seek(0, SeekOrigin.Begin);
            var asyncCopiedStream = new TdbStream(false, true);
            await copiedStream.CopyToAsync(asyncCopiedStream);
            asyncCopiedStream.Seek(0, SeekOrigin.Begin);
            Assert.Equal(decodedBytes[1], asyncCopiedStream.ReadByte());
        }
    }
}