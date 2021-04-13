using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace BallanceRecordModifier.UnitTest
{
    public class VirtoolsArrayUnitTest
    {
        [Theory]
        [InlineData(
            "YiLB4gfG5kRGxySGwWOk7wz1svX19Sr19fUr9fX11dXV1eNnBgWGJKcGh4b1SvX19ePHB6dkRPUK9fX1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1+cv19eur9fXla/X19yv19eEL9fXzyvX17ar19f9q9fXpSvX1+wr19Q=="
        )]
        public async Task TestCreateHighScoreArray(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var tdbStream = new TdbStream(false, true, sampleBytes);
            using var tdbReader = new TdbReader(tdbStream);

            var virtoolsArray = await VirtoolsArray.CreateAsync(tdbReader, true);
            Assert.Equal("DB_Highscore_Lv01", virtoolsArray.SheetName);

            tdbStream.Position = 0;
            virtoolsArray = await VirtoolsArray.CreateAsync(tdbReader, false);
            byte[] buffer = new byte[virtoolsArray.ChunkSize];
            tdbStream.Read(buffer);
            var task = virtoolsArray.PopulateAsync(buffer);
            task.Wait();
            Assert.Equal("DB_Highscore_Lv01", virtoolsArray.SheetName);
        }

        [Theory]
        [InlineData(
            "YiLB4gfG5kRGxySGwWOk7wz1svX19Sr19fUr9fX11dXV1eNnBgWGJKcGh4b1SvX19ePHB6dkRPUK9fX1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1gySv6WKGpgaEZ2T1+cv19eur9fXla/X19yv19eEL9fXzyvX17ar19f9q9fXpSvX1+wr19Q=="
        )]
        public async Task TestCreateArrayOnExhausted(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var tdbStream = new TdbStream(false, true, sampleBytes);
            var tdbReader = new TdbReader(tdbStream);
            var va = await VirtoolsArray.CreateAsync(tdbReader, true);
            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                if (tdbStream.Position == tdbStream.Length)
                    throw new EndOfStreamException();
                var buffer = new byte[va.ChunkSize];
                await tdbStream.ReadAsync(buffer);
                // tdbStream.Read(buffer);
                await va.PopulateAsync(buffer);
            });
        }

        [Theory]
        [InlineData(
            "REJfTGV2ZWxmcmVpc2NoYWx0dW5nAFAAAAABAAAADAAAAP////9GcmVpZ2VzY2hhbHRldD8AAQAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==")]
        public async Task TestCreateDB_Levelfreischaltung(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            await using var tdbStream = new TdbStream(false,false, sampleBytes);
            using var tdbReader = new TdbReader(tdbStream);
            var va = await VirtoolsArray.CreateAsync(tdbReader, true);
            Assert.Equal("DB_Levelfreischaltung", va.SheetName);
        }

        [Theory]
        [InlineData(
            "YiLBw+dkB8enRPUU9fX1S/X19Qr19fXV1dXVoMdnhIeG9Sr19fVABadG5ulkx+lARiSGhqfN9Qr19fVDhgXposckxAYkZvUK9fX1Q4YF6SIGRkfEBiRm9Qr19fVDhgXpY4amZPUK9fX1Q4YF6SAHxuZk9Qr19fVDhgXpIMdkBmSG6UIGh/UK9fX1Q4YF6WMHpmTpQgaH9Qr19fUDp6SGJGTpQgaH6SDHZAZkB8enzfUK9fX1YwZEZONnBgWGJPVK9fX1QmfHhGZjBgWGJM31CvX19UxMTM319fX1YvX19YL19fWi9fX1wvX19c719fWM9fX19fX19acGh4b19fX19Q=="
        )]
        public async Task TestCreateDB_Options(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            await using var tdbStream = new TdbStream(false,true, sampleBytes);
            using var tdbReader = new TdbReader(tdbStream);
            var va = await VirtoolsArray.CreateAsync(tdbReader, true);
            Assert.Equal("DB_Options", va.SheetName);
            Assert.Equal(0.7f, va.Cells[0, 0]);
        }
        
        [Theory]
        [InlineData(
            "YiLBw+dkB8enRPUU9fX1S/X19Qr19fXV1dXVoMdnhIeG9Sr19fVABadG5ulkx+lARiSGhqfN9Qr19fVDhgXposckxAYkZvUK9fX1Q4YF6SIGRkfEBiRm9Qr19fVDhgXpY4amZPUK9fX1Q4YF6SAHxuZk9Qr19fVDhgXpIMdkBmSG6UIGh/UK9fX1Q4YF6WMHpmTpQgaH9Qr19fUDp6SGJGTpQgaH6SDHZAZkB8enzfUK9fX1YwZEZONnBgWGJPVK9fX1QmfHhGZjBgWGJM31CvX19UxMTM319fX1YvX19YL19fWi9fX1wvX19c719fWM9fX19fX19acGh4b19fX19Q=="
        )]
        public async Task TestCreateDB_OptionsWithCreateList(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var ms = new MemoryStream(sampleBytes);
            var list = await VirtoolsArray.CreateListAsync(ms);
            Assert.Equal("DB_Options", list[0].SheetName);
            Assert.Equal(0.7f, list[0].Cells[0, 0]);
        }

        [Theory]
        [InlineData(
            "YiLBw+dkB8enRPUU9fX1S/X19Qr19fXV1dXVoMdnhIeG9Sr19fVABadG5ulkx+lARiSGhqfN9Qr19fVDhgXposckxAYkZvUK9fX1Q4YF6SIGRkfEBiRm9Qr19fVDhgXpY4amZPUK9fX1Q4YF6SAHxuZk9Qr19fVDhgXpIMdkBmSG6UIGh/UK9fX1Q4YF6WMHpmTpQgaH9Qr19fUDp6SGJGTpQgaH6SDHZAZkB8enzfUK9fX1YwZEZONnBgWGJPVK9fX1QmfHhGZjBgWGJM31CvX19UxMTM319fX1YvX19YL19fWi9fX1wvX19c719fWM9fX19fX19acGh4b19fX19Q=="
        )]
        public async Task TestToArray(string sample)
        {
            var sampleBytes = Convert.FromBase64String(sample);
            var tdbStream = new TdbStream(false, true, sampleBytes);
            var tdbReader = new TdbReader(tdbStream);
            var va = await VirtoolsArray.CreateAsync(tdbReader, true);
            Assert.Equal(sampleBytes, await va.ToArrayAsync());
        }

        [Theory]
        [InlineData("1", 0)]
        public void TestKeyConversions(string key, int index)
        {
            Assert.Equal(key, VirtoolsArray.ConvertIndexToKey(index));
            Assert.Equal(index, VirtoolsArray.ConvertKeyToIndex(key));
        }
    }
}