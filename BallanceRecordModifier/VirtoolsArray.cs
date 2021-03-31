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

        public List<Tuple<string, FieldType>> Headers { get; private set; } = null!;
        public object[,] Cells { get; set; } = null!;

        private VirtoolsArray(string sheetName, int chunkSize)
        {
            SheetName = sheetName;
            _chunkSize = chunkSize;
        }

        private static Task<VirtoolsArray> ReadMetadataAsync(BinaryReader tdbReader)
            => Task.Run(() => 
                new VirtoolsArray(tdbReader.ReadString(), tdbReader.ReadInt32()));

        private Task DetermineSizeAsync(BinaryReader tdbReader)
            => Task.Run(() => {
                ColumnCount = tdbReader.ReadInt32();
                RowCount = tdbReader.ReadInt32();
                Headers = new List<Tuple<string, FieldType>>(ColumnCount);
                Cells = new object[ColumnCount, RowCount];
                
                tdbReader.ReadInt32(); // Skip EOF byte (-1 / 0x FFFF FFFF).
            });

        private Task SetHeaderAsync(BinaryReader tdbReader)
            => Task.Run(() => {
                for (var i = 0; i < ColumnCount; i++)
                {
                    var headerName = tdbReader.ReadString();
                    var headerType = (FieldType) tdbReader.ReadInt32();
                    var header = new Tuple<string, FieldType>(headerName, headerType);
                    Headers.Add(header);
                }
            });

        public Task PopulateCellsAsync(BinaryReader tdbReader)
            => Task.Run(() => {
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

        public static async Task<VirtoolsArray> CreateAsync(TdbReader tdbReader, bool populateCells)
        {
            var ret = await ReadMetadataAsync(tdbReader);
            await ret.DetermineSizeAsync(tdbReader);
            await ret.SetHeaderAsync(tdbReader);
            if (populateCells)
                await ret.PopulateCellsAsync(tdbReader);
            return ret;
        }
    
        public Task<long> WriteToStreamAsync(Stream? stream)
            => Task.Run(() => {
                if (stream is not null && !stream.CanSeek)
                    throw new NotSupportedException("Cannot write to a not seekable stream");
                TdbStream tdbStream = stream is not TdbStream ? 
                    new TdbStream(true, false, stream) : 
                    (TdbStream) stream;

                var arrayBeginPosition = tdbStream.Position;
                var tdbWriter = new TdbWriter(tdbStream);
                tdbWriter.Write(SheetName);
                var chunkSizePosition = tdbStream.Position;
                tdbWriter.Write(0);    // Write a padding for chunk size
                var chunkBegin = tdbStream.Position;
                
                tdbWriter.Write(ColumnCount);
                tdbWriter.Write(RowCount);
                tdbWriter.Write(-1);    // Write padding.

                foreach (var (item1, item2) in Headers)
                {
                    tdbWriter.Write(item1); // Write header name.
                    tdbWriter.Write((int) item2);   // Write header type.
                }
                
                for (var i = 0; i < ColumnCount; i++)
                {
                    for (var j = 0; j < RowCount; j++)
                    {
                        switch (Headers[i].Item2)
                        {
                            case FieldType.Int32: tdbWriter.Write((int) Cells[i, j]); break;
                            case FieldType.Float: tdbWriter.Write((float) Cells[i, j]); break;
                            case FieldType.String: tdbWriter.Write((string) Cells[i, j]); break;
                            default:
                                throw new InvalidOperationException($"Cannot cast {Headers[i].Item2} to a type.");
                        }
                    }
                }

                var arrayEndPosition = tdbStream.Position;
                _chunkSize = (int) (arrayEndPosition - chunkBegin);
                tdbStream.Position = chunkSizePosition;
                tdbWriter.Write(_chunkSize);
                tdbStream.Position = arrayEndPosition;
                return arrayEndPosition - arrayBeginPosition;
            });
        
        public async Task<byte[]> ToArray()
            => await Task.Run(async () =>
            {
                var tdbStream = new TdbStream(true, false);
                var arraySize = await WriteToStreamAsync(tdbStream);
                tdbStream.Seek(-arraySize, SeekOrigin.Current); // Seek to beginning of the array
                var ret = new byte[arraySize];
                await tdbStream.ReadAsync(ret.AsMemory(0, (int) arraySize));
                return ret;
            });
    

        public static int ConvertKeyToIndex(string key) => keys.IndexOf(key);
        public static string ConvertIndexToKey(int index) => keys[index];
    }
}