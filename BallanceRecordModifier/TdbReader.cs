using System.IO;
using System.Text;

namespace BallanceRecordModifier
{
    public class TdbReader: BinaryReader
    {
        public TdbReader(Stream input) : base(input)
        {
        }

        public TdbReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public TdbReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override string ReadString()
        {
            StringBuilder sb = new ();
            var ch = base.ReadChar();
            while (ch != 0)
            {
                sb.Append(ch);
                ch = base.ReadChar();
            }

            return sb.ToString();
        }
    }
}