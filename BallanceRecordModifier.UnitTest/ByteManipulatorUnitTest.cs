using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Text;
using System.Threading.Tasks;

namespace BallanceRecordModifier.UnitTest
{
    public class ByteManipulatorUnitTest
    {
        [Fact]
        public async Task TestCreateFromNullArray()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => ByteManipulator.Create(null));
        }

        [Theory]
        [InlineData("YiLB4gfG5kRGxySGwWOk7ww=", "DB_Highscore_Lv01")]
        [InlineData("gySv6WKGpgaEZ2Q=", "Mr. Default")]
        public void TestDecode(string sample, string expected)
        {
            Assert.Equal(
                Encoding.ASCII.GetBytes(expected),
                ByteManipulator.Decode(
                    Convert.FromBase64String(sample))
            );
        }

        [Theory]
        [InlineData("DB_Highscore_Lv01", "YiLB4gfG5kRGxySGwWOk7ww=")]
        public void TestEncode(string sample, string expected)
        {
            Assert.Equal(ByteManipulator.Encode(
                Encoding.ASCII.GetBytes(sample)),
                    Convert.FromBase64String(expected)
            );
        }

        [Fact]
        public void TestEncodeWithNullOrEmptyArray()
        {
            Assert.Throws<NullReferenceException>(() => ByteManipulator.Encode(null));
            Assert.Throws<ArgumentNullException>(() => ByteManipulator.Encode(new byte[]{}));
        }

        [Fact]
        public void TestDecodeWithNullOrEmptyArray()
        {
            Assert.Throws<NullReferenceException>(() => ByteManipulator.Decode(null));
            Assert.Throws<ArgumentNullException>(() => ByteManipulator.Decode(new byte[]{}));
        }

        [Theory]
        [InlineData(new byte[] {0, 11, 22, 33}, "")]
        [InlineData(
            new byte[] {(byte)'a', (byte)'b', (byte)'c', 0, (byte)'d', (byte)'e'}, 
            "abc"
        )]
        [InlineData(
            new byte[] {(byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', 0}, 
            "abcde"
        )]
        [InlineData(
            new byte[] {(byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e'}, 
            "abcde"
        )]
        public async Task TestReadString(byte[]? sample, string? expected)
        {
            var bm = await ByteManipulator.Create(ByteManipulator.Encode(sample));
            Assert.Equal(expected, bm.ReadString());
        }

        [Theory]
        [InlineData("Hello\0World\0", new string[] {"Hello", "World"})]
        [InlineData("Hello\0\0\0\0World\0\0", new string[] {"Hello", "", "", "", "World", ""})]
        [InlineData("Hello World\0", new string[] {"Hello World"})]
        [InlineData(
            "Hello\0from\0the\0other\0side", 
            new string[] {"Hello", "from", "the", "other", "side"})]
        public async Task TestMultipleReadString(string? sample, string[]? expected)
        {
            string[] read = {};
            ByteManipulator bm = await ByteManipulator.Create(
                ByteManipulator.Encode(
                    Encoding.ASCII.GetBytes(sample ?? "")
                ));

            if (expected != null)
                for (var i = 0; i < expected.Length; i++)
                {
                    read.Append(bm.ReadString());
                }
        }

        [Fact]
        public async Task TestReadStringOnExhaustedBytes()
        {
            var bm = await ByteManipulator.Create(new byte[] {0, 1, 2, 3});
            bm.ReadInt();
            Assert.Throws<InvalidOperationException>(() => bm.ReadString());
        }

        [Theory]
        [InlineData(new int[]{0, 1, 2, 3}, new int[]{0, 1, 2, 3})]
        [InlineData(new int[]{4000, 3600, 3200}, new int[]{4000, 3600, 3200})]
        public async Task TestReadInt(int[] samples, int[] expected)
        {
            List<byte> sampleBytes = new List<byte>();
            foreach (var t in samples)
            {
                var bytes = BitConverter.GetBytes(t);
                for (var j = 0; j < 4; j++)
                {
                    sampleBytes.Add(bytes[j]);
                }
            }
            Assert.Equal(samples.Length * 4, sampleBytes.Count);
            
            ByteManipulator bm = await ByteManipulator.Create(ByteManipulator.Encode(sampleBytes.ToArray()));

            foreach (var i in expected) 
            { 
                Assert.Equal(i, bm.ReadInt());
            }
            
        }

        [Theory]
        [InlineData(
            "oA8AABAOAACADAAA8AoAAGAJAADQBwAAQAYAALAEAAAgAwAAkAEAAA==",
            new int[] {4000, 3600, 3200, 2800, 2400, 2000, 1600, 1200, 800, 400}
        )]
        public async Task TestReadIntFromRaw(string? sample, int[]? expected)
        {
            var sampleBytes =  Convert.FromBase64String(sample);
            var bm = await ByteManipulator.Create(ByteManipulator.Encode(sampleBytes));

            foreach (var i in expected)
            {
                Assert.Equal(i, bm.ReadInt());
            }
        }

        [Theory]
        [InlineData(new byte[] {11, 22})]
        public async Task TestReadIntFromExhaustedByteManipulator(byte[] sample)
        {
            var bm = await ByteManipulator.Create(sample);
            Assert.Throws<InvalidOperationException>(() => bm.ReadInt());
        }
        
        [Theory]
        [InlineData(new float[]{0, 0.1f, 0.2f, 0.3f}, new float[]{0, 0.1f, 0.2f, 0.3f})]
        [InlineData(new float[]{4000.0f, 3600.1f, 3200.2f}, new float[]{4000.0f, 3600.1f, 3200.2f})]
        public async Task TestReadFloat(float[] samples, float[] expected) 
        {
            List<byte> sampleBytes = new List<byte>();
            foreach (var t in samples)
            {
                var bytes = BitConverter.GetBytes(t);
                for (var j = 0; j < 4; j++)
                {
                    sampleBytes.Add(bytes[j]);
                }
            }
            Assert.Equal(samples.Length * 4, sampleBytes.Count);
            
            ByteManipulator bm = await ByteManipulator.Create(ByteManipulator.Encode(sampleBytes.ToArray()));

            foreach (var i in expected) 
            { 
                Assert.Equal(i, bm.ReadFloat());
            }
            
        }
        
        [Theory]
        [InlineData("ZmZmPw==", 0.9f)]
        public async Task TestReadFloatFromRaw(string? sample, float? expected)
        {
            var sampleBytes =  Convert.FromBase64String(sample);
            var bm = await ByteManipulator.Create(ByteManipulator.Encode(sampleBytes));
        
            Assert.Equal(expected, bm.ReadFloat());
        }
        
        [Theory]
        [InlineData(new byte[] {11, 22})]
        public async Task TestReadFloatFromExhaustedByteManipulator(byte[] sample)
        {
            var bm = await ByteManipulator.Create(sample);
            Assert.Throws<InvalidOperationException>(() => bm.ReadFloat());
        }

        [Theory]
        [InlineData(
            "REJfSGlnaHNjb3JlX0x2MDEAxgAAAAIAAAAKAAAA/////w==", 
            "DB_Highscore_Lv01",
            new int[] {198, 2, 10, -1}
            )]
        public async Task TestReadMixed(string sample, string expectedString, int[] expectedInts)
        {
            var bm = await ByteManipulator.Create(ByteManipulator.Encode(Convert.FromBase64String(sample)));
            Assert.Equal(expectedString, bm.ReadString());
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(expectedInts[i], bm.ReadInt());
            }
        }

        [Theory]
        [InlineData("Hello\0World", "HelloWorldHelloWorld")]
        public async Task TestReset(string sample, string expected)
        {
            ByteManipulator bm = await ByteManipulator.Create(
                ByteManipulator.Encode(
                    Encoding.ASCII.GetBytes(sample)
                ));

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(bm.ReadString());
            stringBuilder.Append(bm.ReadString());
            bm.Reset();
            stringBuilder.Append(bm.ReadString());
            stringBuilder.Append(bm.ReadString());
            Assert.Equal(expected, stringBuilder.ToString());
        }
    }
}