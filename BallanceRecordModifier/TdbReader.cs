using System.IO;
using System.Text;

namespace BallanceRecordModifier
{
    public class TdbReader: BinaryReader
    {
        public TdbReader(Stream input) : base(input)
        {
        }

        public override string ReadString()
        {
            var sb = new StringBuilder();

            try
            {
                var ch = base.ReadChar();
                while (ch != 0)
                {
                    sb.Append(ch);
                    ch = base.ReadChar();
                }
            }
            catch (EndOfStreamException) {}

            return sb.ToString();
        }
    }
}