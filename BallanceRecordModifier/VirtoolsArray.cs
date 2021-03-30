using System;
using System.Collections.Generic;
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

        public List<Tuple<string, FieldType>> Headers { get; }
        public object[,] Cells { get; set; }

        public VirtoolsArray(string sheetName, int chunkSize)
        {
            SheetName = sheetName;
            _chunkSize = chunkSize;
        }

        private async Task DetermineSize(TdbStream tdbStream)
            => await Task.Run(() =>
            {
                tdbStream.ReadAsEncoded = false;
                using var tdbReader = new TdbReader(tdbStream);
                ColumnCount = tdbReader.ReadInt32();
                RowCount = tdbReader.ReadInt32();
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

    public async Task PopulateCells(TdbStream tdbStream)
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

        public static int ConvertKeyToIndex(string key) => keys.IndexOf(key);
        public static string ConvertIndexToKey(int index) => keys[index];
    }
}