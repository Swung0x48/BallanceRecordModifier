using System;
using Xunit;

namespace BallanceRecordModifier.UnitTest
{
    public class VirtoolsArrayUnitTest
    {
        [Theory]
        [InlineData(
            "YiLB4gfG5kRGxySGwWOk7wz1svX19Sr19fUr9fX11dXV1eNnBgWGJKcGh4b1SvX19ePHB6dkRPUK9fX1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1+cv19eur9fXla/X19yv19eEL9fXzyvX17ar19f9q9fXpSvX1+wr19Q=="
        )]
        public void TestCreateHighScoreArray(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var bm = ByteManipulator.Create(sampleBytes);

            var va = VirtoolsArray.Create(bm);
            va.SetHeader(bm);
            va.PopulateCells(bm);
        }
        
        [Theory]
        [InlineData(
            "YiLB4gfG5kRGxySGwWOk7wz1svX19Sr19fUr9fX11dXV1eNnBgWGJKcGh4b1SvX19ePHB6dkRPUK9fX1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1+cv19eur9fXla/X19yv19eEL9fXzyvX17ar19f9q9fXpSvX1+wr19Q=="
        )]
        public void TestCreateArrayOnExhausted(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var bm = ByteManipulator.Create(sampleBytes);

            var va = VirtoolsArray.Create(bm);
            va.SetHeader(bm);
            va.PopulateCells(bm);
            
            Assert.Throws<InvalidOperationException>(() => VirtoolsArray.Create(bm));
        }
    }
}