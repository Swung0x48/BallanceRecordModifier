using System;
using Xunit;
using System.Text;

namespace BallanceRecordModifier.UnitTest
{
    public class ByteManipulatorUnitTest
    {
        [Theory]
        [InlineData("YiLB4gfG5kRGxySGwWOk7ww=", "DB_Highscore_Lv01")]
        public void TestDecode(string sample, string expected)
        {
            Assert.Equal(ByteManipulator.Decode(
                    Convert.FromBase64String(sample)),
                Encoding.ASCII.GetBytes(expected)
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

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[] { })]
        [InlineData(new byte[] {11, 22, 33})]
        public void TestEncodeWithNullArray(byte[]? sample)
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
    }
}