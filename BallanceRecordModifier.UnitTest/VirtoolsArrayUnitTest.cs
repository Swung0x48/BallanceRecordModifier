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
            Assert.Equal("DB_Highscore_Lv01", va.SheetName);
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

        [Theory]
        [InlineData(
            "REJfTGV2ZWxmcmVpc2NoYWx0dW5nAFAAAAABAAAADAAAAP////9GcmVpZ2VzY2hhbHRldD8AAQAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==")]
        public void TestCreateDB_Levelfreischaltung(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var bm = ByteManipulator.Create(ByteManipulator.Encode(sampleBytes));
            var va = VirtoolsArray.Create(bm);
            va.SetHeader(bm);
            va.PopulateCells(bm);
            Assert.Equal("DB_Levelfreischaltung", va.SheetName);
        }

        [Theory]
        [InlineData(
            "YiLBw+dkB8enRPUU9fX1S/X19Qr19fXV1dXVoMdnhIeG9Sr19fVABadG5ulkx+lARiSGhqfN9Qr19fVDhgXposckxAYkZvUK9fX1Q4YF6SIGRkfEBiRm9Qr19fVDhgXpY4amZPUK9fX1Q4YF6SAHxuZk9Qr19fVDhgXpIMdkBmSG6UIGh/UK9fX1Q4YF6WMHpmTpQgaH9Qr19fUDp6SGJGTpQgaH6SDHZAZkB8enzfUK9fX1YwZEZONnBgWGJPVK9fX1QmfHhGZjBgWGJM31CvX19UxMTM319fX1YvX19YL19fWi9fX1wvX19c719fWM9fX19fX19acGh4b19fX19Q=="
        )]
        public void TestCreateDB_Options(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var bm = ByteManipulator.Create(sampleBytes);
            var va = VirtoolsArray.Create(bm);
            va.SetHeader(bm);
            va.PopulateCells(bm);
            Assert.Equal("DB_Options", va.SheetName);
            Assert.Equal(0.7f, va.Cells[0, 0]);
        }

        [Theory]
        [InlineData(
            "YiLBw+dkB8enRPUU9fX1S/X19Qr19fXV1dXVoMdnhIeG9Sr19fVABadG5ulkx+lARiSGhqfN9Qr19fVDhgXposckxAYkZvUK9fX1Q4YF6SIGRkfEBiRm9Qr19fVDhgXpY4amZPUK9fX1Q4YF6SAHxuZk9Qr19fVDhgXpIMdkBmSG6UIGh/UK9fX1Q4YF6WMHpmTpQgaH9Qr19fUDp6SGJGTpQgaH6SDHZAZkB8enzfUK9fX1YwZEZONnBgWGJPVK9fX1QmfHhGZjBgWGJM31CvX19UxMTM319fX1YvX19YL19fWi9fX1wvX19c719fWM9fX19fX19acGh4b19fX19Q=="
        )]
        public void TestToArray(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var bm = ByteManipulator.Create(sampleBytes);
            var va = VirtoolsArray.Create(bm);
            va.SetHeader(bm);
            va.PopulateCells(bm);
            Assert.Equal(sampleBytes, va.ToByteArray());
        }
    }
}