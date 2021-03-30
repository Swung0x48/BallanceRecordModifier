using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    public class VirtoolsArray
    {
        #region Constants & enums

        public enum FieldType
        {
            Int32 = 1,
            Float = 2,
            String = 3
        }

        private static readonly List<string> keys = new()
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "BackSpace", "Tab", "Q", "W", "E", "R", "T",
            "Y", "U", "I", "O", "P",
            "[", "]", "Ctrl", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "`", "Shift", "\\", "Z", "X", "C",
            "V", "B", "N", "M", ",", ".", "/",
            "Right Shift", "Alt", "Space", "Num 7", "Num 8", "Num 9", "Num -", "Num 4", "Num 5", "Num 6", "Num +",
            "Num 1", "Num 2", "Num 3", "Num 0", "Num Del", "<", "Up", "Down", "Left", "Right"
        };
        
        #endregion

        public string SheetName { get; set; }
        private int _chunkSize;
        public int ColumnCount { get; private set; }
        public int RowCount { get; private set; }

        public List<Tuple<string, FieldType>> Headers { get; private set; }
        public object[,] Cells { get; set; }

        private VirtoolsArray(string sheetName, int chunkSize)
        {
            SheetName = sheetName;
            _chunkSize = chunkSize;
        }

        private static async Task<VirtoolsArray> ReadMetadata(TdbStream tdbStream)
            => await Task.Run(() =>
            {
                tdbStream.ReadAsEncoded = false;
                var tdbReader = new TdbReader(tdbStream);
                return new VirtoolsArray(tdbReader.ReadString(), tdbReader.ReadInt32());
            });

            private async Task DetermineSize(TdbStream tdbStream)
            => await Task.Run(() =>
            {
                tdbStream.ReadAsEncoded = false;
                var tdbReader = new TdbReader(tdbStream);
                
                ColumnCount = tdbReader.ReadInt32();
                RowCount = tdbReader.ReadInt32();
                Headers = new List<Tuple<string, FieldType>>(ColumnCount);
                Cells = new object[ColumnCount, RowCount];
                
                tdbReader.ReadInt32(); // Skip EOF byte (-1 / 0x FFFF FFFF).
            });

        private async Task SetHeader(TdbStream tdbStream)
            => await Task.Run(() =>
            {
                tdbStream.ReadAsEncoded = false;
                var tdbReader = new TdbReader(tdbStream);
                for (var i = 0; i < ColumnCount; i++)
                {
                    var headerName = tdbReader.ReadString();
                    var headerType = (FieldType) tdbReader.ReadInt32();
                    var header = new Tuple<string, FieldType>(headerName, headerType);
                    Headers.Add(header);
                }
            });

        private async Task PopulateCells(TdbStream tdbStream)
            => await Task.Run(() => {
                tdbStream.ReadAsEncoded = false;
                var tdbReader = new TdbReader(tdbStream);

                for (var i = 0; i < ColumnCount; i++)
                {
                    for (var j = 0; j < RowCount; j++)
                    {
                        Cells[i, j] = Headers[i].Item2 switch
                        {
                            FieldType.Int32 => tdbReader.ReadInt32(),
                            FieldType.Float => tdbReader.ReadSingle(),
                            FieldType.String => tdbReader.ReadString(),
                            _ => Cells[i, j]
                        };
                    }
                }
            });

        public static async Task<VirtoolsArray> Read(TdbStream tdbStream)
        {
            var ret = await ReadMetadata(tdbStream);
            await ret.DetermineSize(tdbStream);
            await ret.SetHeader(tdbStream);
            await ret.PopulateCells(tdbStream);
            return ret;
        }
    
        public async Task<TdbStream> ToStream(Stream? stream)
            => await Task.Run(() =>
            {
                if (stream is not null && !stream.CanSeek)
                    throw new NotSupportedException("Cannot write to a not seekable stream");
                var tdbStream = new TdbStream(false, true, stream);
                using var tdbWriter = new TdbWriter(tdbStream);
                tdbWriter.Write(SheetName);
                var chunkSizePosition = tdbStream.Position;
                tdbWriter.Write(0);    // Write a padding for chunk size
                var chunkBegin = tdbStream.Position;
                
                // var tmp = new List<byte>();
                // tmp.AddRange(BitConverter.GetBytes(ColumnCount));
                // tmp.AddRange(BitConverter.GetBytes(RowCount));
                // tmp.AddRange(BitConverter.GetBytes(-1));    // Write padding.
                tdbWriter.Write(ColumnCount);
                tdbWriter.Write(RowCount);
                tdbWriter.Write(-1);    // Write padding.

                foreach (var (item1, item2) in Headers)
                {
                    tdbWriter.Write(item1); // Write header name.
                    tdbWriter.Write((int) item2);   // Write header type.
                    // tmp.AddRange(Encoding.ASCII.GetBytes(item1 + '\0')); // Write header name.
                    // tmp.AddRange(BitConverter.GetBytes((int) item2));       // Write header type.
                }
                
                for (var i = 0; i < ColumnCount; i++)
                {
                    for (var j = 0; j < RowCount; j++)
                    {
                        switch (Headers[i].Item2)
                        {
                            // case FieldType.Int32: tmp.AddRange(BitConverter.GetBytes((int) Cells[i, j])); break;
                            // case FieldType.Float: tmp.AddRange(BitConverter.GetBytes((float) Cells[i, j])); break;
                            // case FieldType.String: tmp.AddRange(Encoding.ASCII.GetBytes((string) Cells[i, j] + '\0')); break;
                            case FieldType.Int32: tdbWriter.Write((int) Cells[i, j]); break;
                            case FieldType.Float: tdbWriter.Write((float) Cells[i, j]); break;
                            case FieldType.String: tdbWriter.Write((string) Cells[i, j]); break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                _chunkSize = (int) (tdbStream.Position - chunkBegin);
                tdbStream.Position = chunkSizePosition;
                tdbWriter.Write(_chunkSize);
                // ret.AddRange(BitConverter.GetBytes(_chunkSize)); // Write chunk size.
                // ret.AddRange(tmp); // Write the rest (headers and cells).

                return tdbStream;
            });
    

        public static int ConvertKeyToIndex(string key) => keys.IndexOf(key);
        public static string ConvertIndexToKey(int index) => keys[index];
    }
}