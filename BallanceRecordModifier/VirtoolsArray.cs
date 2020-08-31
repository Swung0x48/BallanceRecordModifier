using System;
using System.Collections.Generic;
using System.Text;

namespace BallanceRecordModifier
{
    public class VirtoolsArray
    {
        #region Constants & enums

        public enum FieldType : int
        {
            Int32 = 1,
            Float = 2,
            String = 3
        }

        private static readonly List<string> keys = new List<string>
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "BackSpace", "Tab", "Q", "W", "E", "R", "T",
            "Y", "U", "I", "O", "P",
            "[", "]", "Ctrl", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "`", "Shift", "\\", "Z", "X", "C",
            "V", "B", "N", "M", ",", ".", "/",
            "Right Shift", "Alt", "Space", "Num 7", "Num 8", "Num 9", "Num -", "Num 4", "Num 5", "Num 6", "Num +",
            "Num 1", "Num 2", "Num 3", "Num 0", "Num Del", "<", "Up", "Down", "Left", "Right"
        };

        #endregion

        #region Member variables

        public string SheetName { get; private set; }
        private int _chunkSize;
        private int _columnCount;
        private int _rowCount;
        public List<Tuple<string, FieldType>> Headers { get; private set; }
        public object[,] Cells { get; private set; }

        #endregion

        public VirtoolsArray(string sheetName, int chunkSize, int columnCount, int rowCount)
        {
            SheetName = sheetName;
            _chunkSize = chunkSize;
            _columnCount = columnCount;
            _rowCount = rowCount;
            Headers = new List<Tuple<string, FieldType>>(columnCount);
            Cells = new object[columnCount, rowCount];
        }

        public static VirtoolsArray Create(ByteManipulator bm)
        {
            var sheetName = bm.ReadString();
            var chunkSize = bm.ReadInt();
            var columnCount = bm.ReadInt();
            var rowCount = bm.ReadInt();
            bm.ReadInt(); // Skip EOF byte (-1 / 0x FFFF FFFF).

            return new VirtoolsArray(sheetName, chunkSize, columnCount, rowCount);
        }

        public void SetHeader(ByteManipulator bm)
        {
            for (int i = 0; i < _columnCount; i++)
            {
                var headerName = bm.ReadString();
                var headerType = (FieldType)bm.ReadInt();
                var header = new Tuple<string, FieldType>(headerName, headerType);
                Headers.Add(header);
            }
        }

        public void PopulateCells(ByteManipulator bm)
        {
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    switch (Headers[i].Item2)
                    {
                        case FieldType.Int32: Cells[i, j] = bm.ReadInt(); break;
                        case FieldType.Float: Cells[i, j] = bm.ReadFloat(); break;
                        case FieldType.String: Cells[i, j] = bm.ReadString(); break;
                    }
                }
            }
        }

        public byte[] ToByteArray()
        {
            var ret = new List<byte>(Encoding.ASCII.GetBytes(SheetName + '\0')); // Write sheet name.
            
            var tmp = new List<byte>();
            tmp.AddRange(BitConverter.GetBytes(_columnCount));
            tmp.AddRange(BitConverter.GetBytes(_rowCount));
            tmp.AddRange(BitConverter.GetBytes(-1));    // Write separator.
            
            foreach (var item in Headers)
            {
                tmp.AddRange(Encoding.ASCII.GetBytes(item.Item1 + '\0')); // Write header name.
                tmp.AddRange(BitConverter.GetBytes((int) item.Item2));       // Write header type.
            }
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    switch (Headers[i].Item2)
                    {
                        case FieldType.Int32: tmp.AddRange(BitConverter.GetBytes((int) Cells[i, j])); break;
                        case FieldType.Float: tmp.AddRange(BitConverter.GetBytes((float) Cells[i, j])); break;
                        case FieldType.String: tmp.AddRange(Encoding.ASCII.GetBytes((string) Cells[i, j] + '\0')); break;
                    }
                }
            }

            _chunkSize = tmp.Count;
            ret.AddRange(BitConverter.GetBytes(_chunkSize)); // Write chunk size.
            ret.AddRange(tmp); // Write the rest (headers and cells).

            return ret.ToArray();
        }

        public static int ConvertKeyToIndex(string key) => keys.IndexOf(key);
        public static string ConvertIndexToKey(int index) => keys[index];

    }
}
