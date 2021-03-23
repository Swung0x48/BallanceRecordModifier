using System.IO;
using System.Linq;

namespace BallanceRecordModifier
{
    public class TdbWriter : BinaryWriter
    {
        public override void Write(string value)
        {
            Write((value + '\0').ToArray());
        }
    }
}