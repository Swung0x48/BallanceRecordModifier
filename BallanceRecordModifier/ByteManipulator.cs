using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    public class ByteManipulator: IEnumerable
    {
        private byte[] Array { get; }
        private int _index;

        private ByteManipulator(byte[] array)
        {
            Array = array;
            _index = 0;
        }

        public static ByteManipulator Create(byte[] encoded)
        {
            if (encoded is null) throw new NullReferenceException("Attempting to create ByteManipulator with null array.");
            
            return new ByteManipulator(Decode(encoded));
        }
        
        internal static byte[] Decode(byte[]? array)
        {
            if (array is null) throw new NullReferenceException();
            
            for (int index = 0; index < array.Length; index++)
            {
                array[index] = (byte) (array[index] << 3 | array[index] >> 5);
                array[index] = (byte) (-(array[index] ^ 0xAF));
            }

            return array;
        }

        internal static byte[] Encode(byte[]? array)
        {
            if (array is null) throw new NullReferenceException();

            for (var index = 0; index < array.Length; index++)
            {
                array[index] = (byte) (-(array[index]) ^ 0xAF);
                array[index] = (byte) (array[index] << 5 | array[index] >> 3);
            }

            return array;
        }

        public IEnumerator GetEnumerator()
        {
            return Array.GetEnumerator(); // TODO
        }

        public string ReadString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (; _index < Array.Length; _index++)
            {
                if (Array[_index] == 0) break;
                stringBuilder.Append((char)Array[_index]);
            }

            return stringBuilder.ToString();
        }
    }
}