using System;
using System.Collections;
using System.Collections.Generic;

namespace BallanceRecordModifier
{
    public class VirtoolsArray
    {
        private static readonly List<string> keys = new List<string>
        {"1","2","3","4","5","6","7","8","9","0","-","=","BackSpace","Tab","Q","W","E","R","T","Y","U","I","O","P",
            "[","]","Ctrl","A","S","D","F","G","H","J","K","L",";","'","`","Shift","\\","Z","X","C","V","B","N","M",",",".","/",
            "Right Shift","Alt","Space","Num 7","Num 8","Num 9","Num -","Num 4","Num 5","Num 6","Num +","Num 1","Num 2","Num 3","Num 0","Num Del","<","Up","Down","Left","Right"};
        
        private string _sheetName;
        private Int32 _chunkSize;
        private Int32 _columnCount;
        private Int32 _rowCount;

        private List<RowBase> _rows;
        
        private abstract class RowBase
        {
            private string HeaderName { get; set; }
            private List<object> Cells { get; set; }
        }
        
        private enum FieldType: int
        {
            Int32 = 1,
            Float = 2,
            String = 3
        }

        public VirtoolsArray()
        {
            
        }

    }
}
