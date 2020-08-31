using System;
using System.Collections;
using System.Collections.Generic;

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

        private string _sheetName;
        private int _chunkSize;
        private int _columnCount;
        private int _rowCount;
        private List<Tuple<string, FieldType>> _headers;
        private object[,] _cells; // Should be tuples

        #endregion

        #region Accessors

        public string SheetName => _sheetName;
        public List<Tuple<string, FieldType>> Headers => _headers;
        public object[,] Cells => _cells;

        #endregion
        
        public VirtoolsArray(string sheetName, int chunkSize, int columnCount, int rowCount)
        {
            _sheetName = sheetName;
            _chunkSize = chunkSize;
            _columnCount = columnCount;
            _rowCount = rowCount;
            _headers = new List<Tuple<string, FieldType>>(columnCount);
            _cells = new object[columnCount, rowCount];
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
                _headers.Add(header);
            }
        }

        public void PopulateCells(ByteManipulator bm)
        {
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    switch (_headers[i].Item2)
                    {
                        case FieldType.Int32: _cells[i, j] = bm.ReadInt(); break;
                        case FieldType.Float: _cells[i, j] = bm.ReadFloat(); break;
                        case FieldType.String: _cells[i, j] = bm.ReadString(); break;
                    }
                }
            }
        }
    }
}
