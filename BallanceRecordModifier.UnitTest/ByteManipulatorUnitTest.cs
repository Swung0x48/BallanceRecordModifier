using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Text;

namespace BallanceRecordModifier.UnitTest
{
    public class ByteManipulatorUnitTest
    {
        [Theory]
        [InlineData("YiLB4gfG5kRGxySGwWOk7ww=", "DB_Highscore_Lv01")]
        [InlineData("", "")]
        public void TestDecode(string sample, string expected)
        {
            Assert.Equal(ByteManipulator.Decode(
                    Convert.FromBase64String(sample)),
                Encoding.ASCII.GetBytes(expected)
            );
        }

        [Theory]
        [InlineData("DB_Highscore_Lv01", "YiLB4gfG5kRGxySGwWOk7ww=")]
        [InlineData("", "")]
        public void TestEncode(string sample, string expected)
        {
            Assert.Equal(ByteManipulator.Encode(
                Encoding.ASCII.GetBytes(sample)),
                    Convert.FromBase64String(expected)
            );
        }

        [Theory]
        [InlineData(null, null)]
        public void TestEncodeWithNullArray(byte[]? sample, byte[]? expected)
        {
            try
            {
                ByteManipulator.Decode(sample);
            }
            catch (NullReferenceException)
            {
                Assert.Null(sample);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[] { })]
        [InlineData(new byte[] { 11, 22, 33 })]
        public void TestDecodeWithNullArray(byte[]? sample)
        {
            try
            {
                ByteManipulator.Decode(sample);
            }
            catch (ArgumentNullException)
            {
                Assert.Equal(sample, new byte[] { });
            }
            catch (NullReferenceException)
            {
                Assert.Null(sample);
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(new byte[]{}, "")]
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
        public void TestReadString(byte[]? sample, string? expected)
        {
            try
            {
                var bm = ByteManipulator.Create(ByteManipulator.Encode(sample));
                Assert.Equal(bm.ReadString(), expected);
            }
            catch (NullReferenceException)
            {
                Assert.Null(sample);
            }
        }

        [Theory]
        [InlineData("", new string[] {})]
        [InlineData("Hello\0World\0", new string[] {"Hello", "World"})]
        [InlineData("Hello World\0", new string[] {"Hello World"})]
        [InlineData(
            "Hello\0from\0the\0other\0side", 
            new string[] {"Hello", "from", "the", "other", "side"})]
        public void TestMultipleReadString(string? sample, string[]? expected)
        {
            try
            {
                string[] read = {};
                ByteManipulator bm = ByteManipulator.Create(
                    ByteManipulator.Encode(
                        Encoding.ASCII.GetBytes(sample)
                    ));
                for (var i = 0; i < expected.Length; i++)
                {
                    read.Append(bm.ReadString());
                }
            }
            catch (NullReferenceException)
            {
                Assert.Null(sample);
            }
        }
    }
}